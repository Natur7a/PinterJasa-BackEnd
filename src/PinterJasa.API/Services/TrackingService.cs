using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.DTOs.Tracking;
using PinterJasa.API.Models;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Services;

public class TrackingService : ITrackingService
{
    private static readonly HashSet<string> TrackingAllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "accepted", "on_the_way", "in_progress"
    };

    private readonly AppDbContext _db;

    public TrackingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LocationPingResponse> AddPingAsync(Guid orderId, Guid requesterId, TrackingPingRequest request)
    {
        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        // Only the assigned provider can post location pings
        var provider = await _db.Providers.FirstOrDefaultAsync(p => p.UserId == requesterId)
            ?? throw new UnauthorizedAccessException("Provider profile not found.");

        if (order.ProviderId != provider.Id)
            throw new UnauthorizedAccessException("You are not authorized to post tracking for this order.");

        if (!TrackingAllowedStatuses.Contains(order.Status))
            throw new InvalidOperationException(
                $"Tracking is only allowed when the order status is one of: {string.Join(", ", TrackingAllowedStatuses)}. Current status: '{order.Status}'.");

        var ping = new LocationPing
        {
            OrderId = orderId,
            ProviderId = provider.Id,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AccuracyMeters = request.AccuracyMeters,
            TimestampUtc = DateTime.UtcNow
        };

        _db.LocationPings.Add(ping);
        await _db.SaveChangesAsync();

        return MapToResponse(ping);
    }

    public async Task<LocationPingResponse> GetLatestPingAsync(Guid orderId, Guid requesterId, string requesterRole)
    {
        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        await AuthorizeReadAccessAsync(order, requesterId, requesterRole);

        var ping = await _db.LocationPings
            .Where(lp => lp.OrderId == orderId)
            .OrderByDescending(lp => lp.TimestampUtc)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"No tracking data found for order {orderId}.");

        return MapToResponse(ping);
    }

    public async Task<IEnumerable<LocationPingResponse>> GetPingHistoryAsync(Guid orderId, Guid requesterId, string requesterRole, int limit = 50)
    {
        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        await AuthorizeReadAccessAsync(order, requesterId, requesterRole);

        var clampedLimit = Math.Clamp(limit, 1, 200);

        var pings = await _db.LocationPings
            .Where(lp => lp.OrderId == orderId)
            .OrderByDescending(lp => lp.TimestampUtc)
            .Take(clampedLimit)
            .ToListAsync();

        return pings.Select(MapToResponse);
    }

    private async Task AuthorizeReadAccessAsync(Order order, Guid requesterId, string requesterRole)
    {
        if (requesterRole == "admin")
            return;

        if (requesterRole == "customer")
        {
            if (order.CustomerId != requesterId)
                throw new UnauthorizedAccessException("You are not authorized to view tracking for this order.");
            return;
        }

        if (requesterRole == "provider")
        {
            var provider = await _db.Providers.FirstOrDefaultAsync(p => p.UserId == requesterId)
                ?? throw new UnauthorizedAccessException("Provider profile not found.");
            if (order.ProviderId != provider.Id)
                throw new UnauthorizedAccessException("You are not authorized to view tracking for this order.");
            return;
        }

        throw new UnauthorizedAccessException("Unauthorized.");
    }

    private static LocationPingResponse MapToResponse(LocationPing lp) => new()
    {
        Id = lp.Id,
        OrderId = lp.OrderId,
        ProviderId = lp.ProviderId,
        Latitude = lp.Latitude,
        Longitude = lp.Longitude,
        AccuracyMeters = lp.AccuracyMeters,
        TimestampUtc = lp.TimestampUtc
    };
}
