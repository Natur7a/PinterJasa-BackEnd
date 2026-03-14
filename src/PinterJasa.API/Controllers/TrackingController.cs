using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinterJasa.API.DTOs.Tracking;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Controllers;

[ApiController]
[Route("api/orders/{orderId:guid}/tracking")]
[Authorize]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;

    public TrackingController(ITrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    /// <summary>
    /// Provider posts a GPS location ping for an active order.
    /// </summary>
    [HttpPost("ping")]
    [Authorize(Roles = "provider")]
    public async Task<IActionResult> Ping(Guid orderId, [FromBody] TrackingPingRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ping = await _trackingService.AddPingAsync(orderId, userId, request);
        return CreatedAtAction(nameof(GetLatest), new { orderId }, ping);
    }

    /// <summary>
    /// Buyer, seller, or admin retrieves the latest GPS location for an order.
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest(Guid orderId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var ping = await _trackingService.GetLatestPingAsync(orderId, userId, role);
        return Ok(ping);
    }

    /// <summary>
    /// Buyer, seller, or admin retrieves bounded ping history for an order.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(Guid orderId, [FromQuery] int limit = 50)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var pings = await _trackingService.GetPingHistoryAsync(orderId, userId, role, limit);
        return Ok(pings);
    }
}
