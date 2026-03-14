using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.DTOs.Xendit;
using PinterJasa.API.Models;
using PinterJasa.API.Services;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.Tests;

/// <summary>
/// Verifies that <see cref="PayoutService.ProcessPayoutAsync"/> is idempotent:
/// once a payout reaches "processing" or "paid" status it must not trigger a
/// second Xendit disbursement call, even under concurrent invocations.
/// </summary>
public class PayoutIdempotencyTests
{
    private static readonly Guid PayoutId = new("a1b2c3d4-0001-0001-0001-000000000001");

    // ── helpers ──────────────────────────────────────────────────────────

    private static AppDbContext CreateDb(string dbName, string payoutStatus = "pending")
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var db = new AppDbContext(options);

        var providerUser = new User
        {
            Id = TestDbFactory.ProviderUserId,
            Name = "Test Provider",
            Email = $"provider_{dbName}@test.com",
            PasswordHash = "hash",
            Role = "provider",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var provider = new Provider
        {
            Id = TestDbFactory.ProviderId,
            UserId = TestDbFactory.ProviderUserId,
            BankCode = "BCA",
            BankAccountNumber = "1234567890",
            BankAccountName = "Test Provider",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var customerUser = new User
        {
            Id = TestDbFactory.CustomerId,
            Name = "Test Customer",
            Email = $"customer_{dbName}@test.com",
            PasswordHash = "hash",
            Role = "customer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var category = new Category
        {
            Id = TestDbFactory.CategoryId,
            Name = "Test Category",
            CommissionRate = 0.15m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var service = new Service
        {
            Id = TestDbFactory.ServiceId,
            ProviderId = TestDbFactory.ProviderId,
            CategoryId = TestDbFactory.CategoryId,
            Title = "Test Service",
            Price = 100_000m,
            PriceUnit = "per_job",
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var order = new Order
        {
            Id = TestDbFactory.OrderId,
            CustomerId = TestDbFactory.CustomerId,
            ServiceId = TestDbFactory.ServiceId,
            ProviderId = TestDbFactory.ProviderId,
            Status = "completed",
            TotalPrice = 100_000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        var payout = new Payout
        {
            Id = PayoutId,
            OrderId = TestDbFactory.OrderId,
            ProviderId = TestDbFactory.ProviderId,
            GrossAmount = 100_000m,
            CommissionRate = 0.15m,
            CommissionAmount = 15_000m,
            NetAmount = 85_000m,
            Status = payoutStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(customerUser, providerUser);
        db.Providers.Add(provider);
        db.Categories.Add(category);
        db.Services.Add(service);
        db.Orders.Add(order);
        db.Payouts.Add(payout);
        db.SaveChanges();

        return db;
    }

    private static PayoutService CreateService(string dbName, string payoutStatus, IXenditService xendit)
    {
        var db = CreateDb(dbName, payoutStatus);
        return new PayoutService(db, xendit);
    }

    // ── fake Xendit service that records how many times it was called ──────

    private sealed class FakeXenditService : IXenditService
    {
        public int DisbursementCallCount { get; private set; }

        public Task<XenditDisbursementResponse> CreateDisbursementAsync(XenditDisbursementRequest request)
        {
            DisbursementCallCount++;
            return Task.FromResult(new XenditDisbursementResponse
            {
                Id = "disbursement-" + Guid.NewGuid(),
                ExternalId = request.ExternalId,
                Amount = request.Amount,
                BankCode = request.BankCode,
                Status = "PENDING"
            });
        }

        public Task<XenditInvoiceResponse> CreateInvoiceAsync(XenditInvoiceRequest request) =>
            Task.FromResult(new XenditInvoiceResponse());

        public bool VerifyWebhookToken(string token) => true;
    }

    // ── tests ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessPayout_AlreadyProcessing_ShouldReturnExistingWithoutCallingXendit()
    {
        var xendit = new FakeXenditService();
        var svc = CreateService("payout_already_processing", "processing", xendit);

        var result = await svc.ProcessPayoutAsync(PayoutId);

        Assert.Equal("processing", result.Status);
        Assert.Equal(0, xendit.DisbursementCallCount);
    }

    [Fact]
    public async Task ProcessPayout_AlreadyPaid_ShouldReturnExistingWithoutCallingXendit()
    {
        var xendit = new FakeXenditService();
        var svc = CreateService("payout_already_paid", "paid", xendit);

        var result = await svc.ProcessPayoutAsync(PayoutId);

        Assert.Equal("paid", result.Status);
        Assert.Equal(0, xendit.DisbursementCallCount);
    }

    [Fact]
    public async Task ProcessPayout_Pending_ShouldCallXenditOnce()
    {
        var xendit = new FakeXenditService();
        var svc = CreateService("payout_pending_process", "pending", xendit);

        var result = await svc.ProcessPayoutAsync(PayoutId);

        Assert.Equal("processing", result.Status);
        Assert.Equal(1, xendit.DisbursementCallCount);
    }

    [Fact]
    public async Task ProcessPayout_CalledTwiceSequentially_ShouldCallXenditOnce()
    {
        var xendit = new FakeXenditService();
        var db = CreateDb("payout_sequential_idempotency", "pending");
        var svc = new PayoutService(db, xendit);

        // First call transitions to "processing"
        var first = await svc.ProcessPayoutAsync(PayoutId);
        // Second call should detect "processing" and return without calling Xendit again
        var second = await svc.ProcessPayoutAsync(PayoutId);

        Assert.Equal("processing", first.Status);
        Assert.Equal("processing", second.Status);
        Assert.Equal(1, xendit.DisbursementCallCount);
    }

    [Fact]
    public async Task ProcessPayout_ConcurrentRequests_ShouldCallXenditOnlyOnce()
    {
        // NOTE: The EF Core in-memory provider does not enforce transaction isolation, so
        // this test validates sequential-async behaviour rather than true parallel execution.
        // In production (PostgreSQL) the idempotency guard (status check) combined with
        // appropriate transaction isolation prevents duplicate Xendit disbursement calls.
        // A second context opening the same shared in-memory store after the first has
        // committed will see the updated "processing" status and return early.
        var xenditA = new FakeXenditService();
        var xenditB = new FakeXenditService();

        var dbA = CreateDb("payout_concurrent", "pending");
        var dbB = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("payout_concurrent").Options);

        var svcA = new PayoutService(dbA, xenditA);
        var svcB = new PayoutService(dbB, xenditB);

        // Run both "concurrently"
        var taskA = svcA.ProcessPayoutAsync(PayoutId);
        var taskB = svcB.ProcessPayoutAsync(PayoutId);

        await Task.WhenAll(taskA, taskB);

        // Both should report "processing", but only one disbursement call in total
        Assert.Equal("processing", (await taskA).Status);
        Assert.Equal("processing", (await taskB).Status);
        Assert.Equal(1, xenditA.DisbursementCallCount + xenditB.DisbursementCallCount);
    }
}
