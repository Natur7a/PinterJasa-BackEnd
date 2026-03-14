using PinterJasa.API.DTOs.Orders;
using PinterJasa.API.Services;

namespace PinterJasa.Tests;

public class OrderStatusTransitionTests
{
    private static OrderService CreateService(string dbName, string initialStatus = "accepted")
    {
        var db = TestDbFactory.Create(dbName);
        // Override order status for the test
        var order = db.Orders.Find(TestDbFactory.OrderId)!;
        order.Status = initialStatus;
        db.SaveChanges();
        return new OrderService(db);
    }

    [Theory]
    [InlineData("created", "awaiting_payment")]
    [InlineData("created", "cancelled")]
    [InlineData("awaiting_payment", "paid")]
    [InlineData("awaiting_payment", "cancelled")]
    [InlineData("paid", "accepted")]
    [InlineData("paid", "cancelled")]
    [InlineData("paid", "refunded")]
    [InlineData("accepted", "on_the_way")]
    [InlineData("accepted", "in_progress")]
    [InlineData("accepted", "cancelled")]
    [InlineData("on_the_way", "in_progress")]
    [InlineData("on_the_way", "cancelled")]
    [InlineData("in_progress", "completed")]
    public async Task ValidTransition_ShouldSucceed(string fromStatus, string toStatus)
    {
        var svc = CreateService($"transition_valid_{fromStatus}_{toStatus}", fromStatus);
        var result = await svc.UpdateStatusAsync(
            TestDbFactory.OrderId, toStatus, TestDbFactory.CustomerId, "admin");
        Assert.Equal(toStatus, result.Status);
    }

    [Theory]
    [InlineData("created", "in_progress")]
    [InlineData("accepted", "completed")]
    [InlineData("in_progress", "cancelled")]
    [InlineData("completed", "cancelled")]
    [InlineData("completed", "in_progress")]
    [InlineData("on_the_way", "accepted")]
    public async Task InvalidTransition_ShouldThrow(string fromStatus, string toStatus)
    {
        var svc = CreateService($"transition_invalid_{fromStatus}_{toStatus}", fromStatus);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateStatusAsync(TestDbFactory.OrderId, toStatus, TestDbFactory.CustomerId, "admin"));
    }

    [Fact]
    public async Task CustomerCannotUpdateOtherCustomerOrder()
    {
        var svc = CreateService("customer_auth_check");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.UpdateStatusAsync(TestDbFactory.OrderId, "cancelled", TestDbFactory.OtherCustomerId, "customer"));
    }

    [Fact]
    public async Task ProviderCannotUpdateUnassignedOrder()
    {
        var svc = CreateService("provider_auth_check");
        // OtherProviderUserId is not the assigned provider
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.UpdateStatusAsync(TestDbFactory.OrderId, "on_the_way", TestDbFactory.OtherProviderUserId, "provider"));
    }

    [Fact]
    public async Task AdminCanUpdateAnyOrder()
    {
        var svc = CreateService("admin_can_update");
        var result = await svc.UpdateStatusAsync(
            TestDbFactory.OrderId, "on_the_way", Guid.NewGuid(), "admin");
        Assert.Equal("on_the_way", result.Status);
    }
}
