using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Controllers;

[ApiController]
[Route("api/payouts")]
[Authorize]
public class PayoutsController : ControllerBase
{
    private readonly IPayoutService _payoutService;

    public PayoutsController(IPayoutService payoutService)
    {
        _payoutService = payoutService;
    }

    [HttpPost("{orderId:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(Guid orderId)
    {
        var payout = await _payoutService.CreatePayoutAsync(orderId);
        return Ok(payout);
    }

    [HttpPatch("{id:guid}/process")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Process(Guid id)
    {
        var payout = await _payoutService.ProcessPayoutAsync(id);
        return Ok(payout);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "provider")]
    public async Task<IActionResult> GetMine()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var payouts = await _payoutService.GetMyPayoutsAsync(userId);
        return Ok(payouts);
    }
}
