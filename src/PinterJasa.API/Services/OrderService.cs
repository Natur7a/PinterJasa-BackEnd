using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.DTOs.Orders;
using PinterJasa.API.Models;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    private static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        ["created"] = new[] { "awaiting_payment", "cancelled" },
        ["awaiting_payment"] = new[] { "paid", "cancelled" },
        ["paid"] = new[] { "accepted", "cancelled", "refunded" },
        ["accepted"] = new[] { "on_the_way", "in_progress", "cancelled" },
        ["on_the_way"] = new[] { "in_progress", "cancelled" },
        ["in_progress"] = new[] { "completed" }
    };

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OrderResponse> CreateAsync(Guid customerId, CreateOrderRequest request)
    {
        var service = await _db.Services.FindAsync(request.ServiceId)
            ?? throw new KeyNotFoundException($"Service {request.ServiceId} not found.");

        if (service.Status != "active")
            throw new InvalidOperationException("Service is not active.");

        var order = new Order
        {
            CustomerId = customerId,
            ServiceId = service.Id,
            ProviderId = service.ProviderId,
            TotalPrice = service.Price,
            Address = request.Address,
            Notes = request.Notes,
            ScheduledAt = request.ScheduledAt
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _db.Entry(order).Reference(o => o.Service).LoadAsync();
        return MapToResponse(order);
    }

    public async Task<OrderResponse> GetByIdAsync(Guid id)
    {
        var order = await _db.Orders.Include(o => o.Service).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Order {id} not found.");
        return MapToResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(Guid customerId)
    {
        var orders = await _db.Orders
            .Include(o => o.Service)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetProviderOrdersAsync(Guid userId)
    {
        var provider = await _db.Providers.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var orders = await _db.Orders
            .Include(o => o.Service)
            .Where(o => o.ProviderId == provider.Id)
            .ToListAsync();
        return orders.Select(MapToResponse);
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid orderId, string newStatus, Guid requesterId, string requesterRole)
    {
        var order = await _db.Orders.Include(o => o.Service).FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        // Authorization: customers can only update their own orders; providers can only update their assigned orders
        if (requesterRole == "customer" && order.CustomerId != requesterId)
            throw new UnauthorizedAccessException("You are not authorized to update this order.");

        if (requesterRole == "provider")
        {
            var provider = await _db.Providers.FirstOrDefaultAsync(p => p.UserId == requesterId)
                ?? throw new UnauthorizedAccessException("Provider profile not found.");
            if (order.ProviderId != provider.Id)
                throw new UnauthorizedAccessException("You are not authorized to update this order.");
        }

        if (!ValidTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition from '{order.Status}' to '{newStatus}'.");

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (newStatus == "completed")
            order.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(Order o) => new()
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        ServiceId = o.ServiceId,
        ServiceTitle = o.Service?.Title ?? string.Empty,
        ProviderId = o.ProviderId,
        Status = o.Status,
        TotalPrice = o.TotalPrice,
        Address = o.Address,
        Notes = o.Notes,
        ScheduledAt = o.ScheduledAt,
        CompletedAt = o.CompletedAt,
        CreatedAt = o.CreatedAt
    };
}
