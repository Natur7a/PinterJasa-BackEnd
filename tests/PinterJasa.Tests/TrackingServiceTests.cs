using PinterJasa.API.DTOs.Tracking;
using PinterJasa.API.Services;

namespace PinterJasa.Tests;

public class TrackingServiceTests
{
    private static readonly TrackingPingRequest ValidPing = new()
    {
        Latitude = -6.2088,
        Longitude = 106.8456,
        AccuracyMeters = 10.0
    };

    private static TrackingService CreateService(string dbName, string orderStatus = "on_the_way")
    {
        var db = TestDbFactory.Create(dbName);
        var order = db.Orders.Find(TestDbFactory.OrderId)!;
        order.Status = orderStatus;
        db.SaveChanges();
        return new TrackingService(db);
    }

    // ── Ping (write) ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("accepted")]
    [InlineData("on_the_way")]
    [InlineData("in_progress")]
    public async Task AddPing_InActiveStatus_Succeeds(string status)
    {
        var svc = CreateService($"ping_active_{status}", status);
        var result = await svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, ValidPing);
        Assert.Equal(TestDbFactory.OrderId, result.OrderId);
        Assert.Equal(ValidPing.Latitude, result.Latitude);
        Assert.Equal(ValidPing.Longitude, result.Longitude);
    }

    [Theory]
    [InlineData("created")]
    [InlineData("awaiting_payment")]
    [InlineData("paid")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public async Task AddPing_InInactiveStatus_Throws(string status)
    {
        var svc = CreateService($"ping_inactive_{status}", status);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, ValidPing));
    }

    [Fact]
    public async Task AddPing_WrongProvider_Throws()
    {
        var svc = CreateService("ping_wrong_provider");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.OtherProviderUserId, ValidPing));
    }

    [Fact]
    public async Task AddPing_CustomerRole_Throws()
    {
        // Customers have no provider profile, so provider lookup fails → unauthorized
        var svc = CreateService("ping_customer");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.CustomerId, ValidPing));
    }

    [Fact]
    public async Task AddPing_OrderNotFound_Throws()
    {
        var svc = CreateService("ping_order_not_found");
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.AddPingAsync(Guid.NewGuid(), TestDbFactory.ProviderUserId, ValidPing));
    }

    // ── GetLatest (read) ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetLatest_BuyerCanRead_OwnOrder()
    {
        var svc = CreateService("get_latest_buyer");
        await svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, ValidPing);
        var result = await svc.GetLatestPingAsync(TestDbFactory.OrderId, TestDbFactory.CustomerId, "customer");
        Assert.Equal(TestDbFactory.OrderId, result.OrderId);
    }

    [Fact]
    public async Task GetLatest_SellerCanRead_OwnOrder()
    {
        var svc = CreateService("get_latest_seller");
        await svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, ValidPing);
        var result = await svc.GetLatestPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, "provider");
        Assert.Equal(TestDbFactory.OrderId, result.OrderId);
    }

    [Fact]
    public async Task GetLatest_AdminCanRead_AnyOrder()
    {
        var svc = CreateService("get_latest_admin");
        await svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, ValidPing);
        var result = await svc.GetLatestPingAsync(TestDbFactory.OrderId, Guid.NewGuid(), "admin");
        Assert.Equal(TestDbFactory.OrderId, result.OrderId);
    }

    [Fact]
    public async Task GetLatest_OtherCustomer_Throws()
    {
        var svc = CreateService("get_latest_other_customer");
        await svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, ValidPing);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.GetLatestPingAsync(TestDbFactory.OrderId, TestDbFactory.OtherCustomerId, "customer"));
    }

    [Fact]
    public async Task GetLatest_OtherProvider_Throws()
    {
        var svc = CreateService("get_latest_other_provider");
        await svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, ValidPing);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.GetLatestPingAsync(TestDbFactory.OrderId, TestDbFactory.OtherProviderUserId, "provider"));
    }

    [Fact]
    public async Task GetLatest_NoPingsYet_Throws()
    {
        var svc = CreateService("get_latest_no_pings");
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.GetLatestPingAsync(TestDbFactory.OrderId, TestDbFactory.CustomerId, "customer"));
    }

    // ── GetHistory (read) ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetHistory_ReturnsPingsInDescendingOrder()
    {
        var svc = CreateService("get_history_order");
        for (int i = 0; i < 3; i++)
        {
            await svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, new TrackingPingRequest
            {
                Latitude = -6.0 + i * 0.001,
                Longitude = 106.0 + i * 0.001
            });
        }
        var result = (await svc.GetPingHistoryAsync(
            TestDbFactory.OrderId, TestDbFactory.CustomerId, "customer", 10)).ToList();
        Assert.Equal(3, result.Count);
        // Most recent first
        for (int i = 0; i < result.Count - 1; i++)
            Assert.True(result[i].TimestampUtc >= result[i + 1].TimestampUtc);
    }

    [Fact]
    public async Task GetHistory_LimitClamped()
    {
        var svc = CreateService("get_history_limit");
        for (int i = 0; i < 5; i++)
        {
            await svc.AddPingAsync(TestDbFactory.OrderId, TestDbFactory.ProviderUserId, new TrackingPingRequest
            {
                Latitude = -6.0 + i * 0.001,
                Longitude = 106.0
            });
        }
        var result = await svc.GetPingHistoryAsync(
            TestDbFactory.OrderId, TestDbFactory.CustomerId, "customer", 3);
        Assert.Equal(3, result.Count());
    }
}
