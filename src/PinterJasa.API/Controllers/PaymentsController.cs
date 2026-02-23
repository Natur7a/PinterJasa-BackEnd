using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var payment = await _paymentService.CreatePaymentAsync(userId, request.OrderId, request.Method);
        return Ok(payment);
    }

    [HttpPatch("{id:guid}/confirm")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Confirm(Guid id, [FromBody] ConfirmPaymentRequest request)
    {
        var payment = await _paymentService.ConfirmPaymentAsync(id, request.GatewayRef);
        return Ok(payment);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId)
    {
        var payment = await _paymentService.GetByOrderIdAsync(orderId);
        return Ok(payment);
    }
}

public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public string Method { get; set; } = string.Empty;
}

public class ConfirmPaymentRequest
{
    public string GatewayRef { get; set; } = string.Empty;
}
