using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.DTOs.Payouts;
using PinterJasa.API.DTOs.Xendit;
using PinterJasa.API.Models;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Services;

public class PayoutService : IPayoutService
{
    private readonly AppDbContext _db;
    private readonly IXenditService _xenditService;

    public PayoutService(AppDbContext db, IXenditService xenditService)
    {
        _db = db;
        _xenditService = xenditService;
    }

    public async Task<PayoutResponse> CreatePayoutAsync(Guid orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Service)
            .ThenInclude(s => s.Category)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.Status != "completed")
            throw new InvalidOperationException("Order must be completed before creating a payout.");

        if (await _db.Payouts.AnyAsync(p => p.OrderId == orderId))
            throw new InvalidOperationException("Payout already exists for this order.");

        var commissionRate = order.Service.Category.CommissionRate;
        var grossAmount = order.TotalPrice;
        var commissionAmount = Math.Round(grossAmount * commissionRate, 2);
        var netAmount = grossAmount - commissionAmount;

        var payout = new Payout
        {
            OrderId = orderId,
            ProviderId = order.ProviderId,
            GrossAmount = grossAmount,
            CommissionRate = commissionRate,
            CommissionAmount = commissionAmount,
            NetAmount = netAmount
        };

        _db.Payouts.Add(payout);
        await _db.SaveChangesAsync();
        return MapToResponse(payout);
    }

    public async Task<PayoutResponse> ProcessPayoutAsync(Guid payoutId)
    {
        var payout = await _db.Payouts.Include(p => p.Provider).FirstOrDefaultAsync(p => p.Id == payoutId)
            ?? throw new KeyNotFoundException($"Payout {payoutId} not found.");

        var provider = payout.Provider;

        if (string.IsNullOrEmpty(provider.BankCode) ||
            string.IsNullOrEmpty(provider.BankAccountNumber) ||
            string.IsNullOrEmpty(provider.BankAccountName))
        {
            throw new InvalidOperationException("Provider bank details are not configured.");
        }

        var disbursementRequest = new XenditDisbursementRequest
        {
            ExternalId = payout.Id.ToString(),
            Amount = payout.NetAmount,
            BankCode = provider.BankCode,
            AccountHolderName = provider.BankAccountName,
            AccountNumber = provider.BankAccountNumber,
            Description = $"Payout for order {payout.OrderId}"
        };

        var disbursement = await _xenditService.CreateDisbursementAsync(disbursementRequest);

        payout.XenditDisbursementId = disbursement.Id;
        payout.Status = "processing";
        payout.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(payout);
    }

    public async Task<IEnumerable<PayoutResponse>> GetMyPayoutsAsync(Guid userId)
    {
        var provider = await _db.Providers.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var payouts = await _db.Payouts.Where(p => p.ProviderId == provider.Id).ToListAsync();
        return payouts.Select(MapToResponse);
    }

    private static PayoutResponse MapToResponse(Payout p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        ProviderId = p.ProviderId,
        GrossAmount = p.GrossAmount,
        CommissionRate = p.CommissionRate,
        CommissionAmount = p.CommissionAmount,
        NetAmount = p.NetAmount,
        Status = p.Status,
        PaidAt = p.PaidAt,
        CreatedAt = p.CreatedAt
    };
}
