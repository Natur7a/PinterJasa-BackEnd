using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.Models;

namespace PinterJasa.Tests;

/// <summary>
/// Creates an in-memory AppDbContext pre-seeded with common test data:
/// a customer user, a provider user+profile, a category, a service, and an order.
/// </summary>
public static class TestDbFactory
{
    public static readonly Guid CustomerId = Guid.NewGuid();
    public static readonly Guid ProviderUserId = Guid.NewGuid();
    public static readonly Guid ProviderId = Guid.NewGuid();
    public static readonly Guid CategoryId = Guid.NewGuid();
    public static readonly Guid ServiceId = Guid.NewGuid();
    public static readonly Guid OrderId = Guid.NewGuid();
    public static readonly Guid OtherCustomerId = Guid.NewGuid();
    public static readonly Guid OtherProviderUserId = Guid.NewGuid();
    public static readonly Guid OtherProviderId = Guid.NewGuid();

    public static AppDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var db = new AppDbContext(options);

        var customerUser = new User
        {
            Id = CustomerId,
            Name = "Test Customer",
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = "customer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var providerUser = new User
        {
            Id = ProviderUserId,
            Name = "Test Provider",
            Email = "provider@test.com",
            PasswordHash = "hash",
            Role = "provider",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var otherProviderUser = new User
        {
            Id = OtherProviderUserId,
            Name = "Other Provider",
            Email = "other.provider@test.com",
            PasswordHash = "hash",
            Role = "provider",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var otherCustomerUser = new User
        {
            Id = OtherCustomerId,
            Name = "Other Customer",
            Email = "other.customer@test.com",
            PasswordHash = "hash",
            Role = "customer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var provider = new Provider
        {
            Id = ProviderId,
            UserId = ProviderUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var otherProvider = new Provider
        {
            Id = OtherProviderId,
            UserId = OtherProviderUserId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var category = new Category
        {
            Id = CategoryId,
            Name = "Test Category",
            CommissionRate = 0.15m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var service = new Service
        {
            Id = ServiceId,
            ProviderId = ProviderId,
            CategoryId = CategoryId,
            Title = "Test Service",
            Price = 100_000m,
            PriceUnit = "per_job",
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var order = new Order
        {
            Id = OrderId,
            CustomerId = CustomerId,
            ServiceId = ServiceId,
            ProviderId = ProviderId,
            Status = "accepted",
            TotalPrice = 100_000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(customerUser, providerUser, otherProviderUser, otherCustomerUser);
        db.Providers.AddRange(provider, otherProvider);
        db.Categories.Add(category);
        db.Services.Add(service);
        db.Orders.Add(order);
        db.SaveChanges();

        return db;
    }
}
