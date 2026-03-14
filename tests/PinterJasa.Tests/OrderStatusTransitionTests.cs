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

    // ── Role-based transition enforcement ──────────────────────────────────

    [Fact]
    public async Task CustomerCanCancelOwnOrderDuringAwaitingPayment()
    {
        var svc = CreateService("customer_cancel_awaiting", "awaiting_payment");
        var result = await svc.UpdateStatusAsync(
            TestDbFactory.OrderId, "cancelled", TestDbFactory.CustomerId, "customer");
        Assert.Equal("cancelled", result.Status);
    }

    [Fact]
    public async Task CustomerCannotAcceptOrder_PaidToAccepted()
    {
        var svc = CreateService("customer_cannot_accept", "paid");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.UpdateStatusAsync(TestDbFactory.OrderId, "accepted", TestDbFactory.CustomerId, "customer"));
    }

    [Fact]
    public async Task CustomerCannotCompleteOrder()
    {
        var svc = CreateService("customer_cannot_complete", "in_progress");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.UpdateStatusAsync(TestDbFactory.OrderId, "completed", TestDbFactory.CustomerId, "customer"));
    }

    [Fact]
    public async Task ProviderCanAcceptOrder_PaidToAccepted()
    {
        var svc = CreateService("provider_can_accept", "paid");
        var result = await svc.UpdateStatusAsync(
            TestDbFactory.OrderId, "accepted", TestDbFactory.ProviderUserId, "provider");
        Assert.Equal("accepted", result.Status);
    }

    [Fact]
    public async Task ProviderCanCompleteOrder()
    {
        var svc = CreateService("provider_can_complete", "in_progress");
        var result = await svc.UpdateStatusAsync(
            TestDbFactory.OrderId, "completed", TestDbFactory.ProviderUserId, "provider");
        Assert.Equal("completed", result.Status);
    }

    [Fact]
    public async Task ProviderCannotRefundOrder()
    {
        var svc = CreateService("provider_cannot_refund", "paid");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.UpdateStatusAsync(TestDbFactory.OrderId, "refunded", TestDbFactory.ProviderUserId, "provider"));
    }

    [Fact]
    public async Task ProviderCannotCancelAcceptedOrder()
    {
        var svc = CreateService("provider_cannot_cancel_accepted", "accepted");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.UpdateStatusAsync(TestDbFactory.OrderId, "cancelled", TestDbFactory.ProviderUserId, "provider"));
    }

    [Fact]
    public async Task AdminCanRefundOrder()
    {
        var svc = CreateService("admin_can_refund", "paid");
        var result = await svc.UpdateStatusAsync(
            TestDbFactory.OrderId, "refunded", Guid.NewGuid(), "admin");
        Assert.Equal("refunded", result.Status);
    }

    [Fact]
    public async Task AdminCanCancelAcceptedOrder()
    {
        var svc = CreateService("admin_can_cancel_accepted", "accepted");
        var result = await svc.UpdateStatusAsync(
            TestDbFactory.OrderId, "cancelled", Guid.NewGuid(), "admin");
        Assert.Equal("cancelled", result.Status);
    }
}
