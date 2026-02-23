using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.DTOs.Xendit;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Controllers;

[ApiController]
[Route("api/webhooks/xendit")]
[AllowAnonymous]
public class XenditWebhookController : ControllerBase
{
    private readonly IXenditService _xenditService;
    private readonly AppDbContext _db;
    private readonly ILogger<XenditWebhookController> _logger;

    public XenditWebhookController(IXenditService xenditService, AppDbContext db, ILogger<XenditWebhookController> logger)
    {
        _xenditService = xenditService;
        _db = db;
        _logger = logger;
    }

    [HttpPost("invoice")]
    public async Task<IActionResult> InvoiceCallback([FromBody] XenditWebhookPayload payload)
    {
        var token = Request.Headers["x-callback-token"].FirstOrDefault();
        if (string.IsNullOrEmpty(token) || !_xenditService.VerifyWebhookToken(token))
        {
            _logger.LogWarning("Xendit invoice webhook received with invalid token.");
            return Unauthorized();
        }

        _logger.LogInformation("Xendit invoice webhook received: ExternalId={ExternalId}, Status={Status}", payload.ExternalId, payload.Status);

        if (!Guid.TryParse(payload.ExternalId, out var paymentId))
        {
            _logger.LogWarning("Xendit invoice webhook has invalid ExternalId: {ExternalId}", payload.ExternalId);
            return Ok();
        }

        var payment = await _db.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment is null)
        {
            _logger.LogWarning("Xendit invoice webhook: payment {PaymentId} not found.", paymentId);
            return Ok();
        }

        if (payload.Status == "PAID")
        {
            payment.Status = "paid";
            payment.GatewayRef = payload.Id;
            payment.PaidAt = DateTime.UtcNow;
            payment.Order.Status = "paid";
            payment.Order.UpdatedAt = DateTime.UtcNow;
        }
        else if (payload.Status is "EXPIRED" or "FAILED")
        {
            payment.Status = "failed";
            payment.Order.Status = "created";
            payment.Order.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("disbursement")]
    public async Task<IActionResult> DisbursementCallback([FromBody] XenditWebhookPayload payload)
    {
        var token = Request.Headers["x-callback-token"].FirstOrDefault();
        if (string.IsNullOrEmpty(token) || !_xenditService.VerifyWebhookToken(token))
        {
            _logger.LogWarning("Xendit disbursement webhook received with invalid token.");
            return Unauthorized();
        }

        _logger.LogInformation("Xendit disbursement webhook received: ExternalId={ExternalId}, Status={Status}", payload.ExternalId, payload.Status);

        if (!Guid.TryParse(payload.ExternalId, out var payoutId))
        {
            _logger.LogWarning("Xendit disbursement webhook has invalid ExternalId: {ExternalId}", payload.ExternalId);
            return Ok();
        }

        var payout = await _db.Payouts.FirstOrDefaultAsync(p => p.Id == payoutId);
        if (payout is null)
        {
            _logger.LogWarning("Xendit disbursement webhook: payout {PayoutId} not found.", payoutId);
            return Ok();
        }

        if (payload.Status == "COMPLETED")
        {
            payout.Status = "completed";
            payout.PaidAt = DateTime.UtcNow;
            payout.UpdatedAt = DateTime.UtcNow;
        }
        else if (payload.Status == "FAILED")
        {
            payout.Status = "failed";
            payout.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}
