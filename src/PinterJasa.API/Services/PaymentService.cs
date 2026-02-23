using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.DTOs.Payments;
using PinterJasa.API.Models;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;

    public PaymentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(Guid customerId, Guid orderId, string method)
    {
        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.CustomerId != customerId)
            throw new UnauthorizedAccessException("Not your order.");

        if (order.Status != "created" && order.Status != "awaiting_payment")
            throw new InvalidOperationException("Order is not in a payable state.");

        if (await _db.Payments.AnyAsync(p => p.OrderId == orderId))
            throw new InvalidOperationException("Payment already exists for this order.");

        var payment = new Payment
        {
            OrderId = orderId,
            CustomerId = customerId,
            Amount = order.TotalPrice,
            Method = method
        };

        _db.Payments.Add(payment);

        order.Status = "awaiting_payment";
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(payment);
    }

    public async Task<PaymentResponse> ConfirmPaymentAsync(Guid paymentId, string gatewayRef)
    {
        var payment = await _db.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException($"Payment {paymentId} not found.");

        payment.Status = "paid";
        payment.GatewayRef = gatewayRef;
        payment.PaidAt = DateTime.UtcNow;

        payment.Order.Status = "paid";
        payment.Order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(payment);
    }

    public async Task<PaymentResponse> GetByOrderIdAsync(Guid orderId)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId)
            ?? throw new KeyNotFoundException($"Payment for order {orderId} not found.");
        return MapToResponse(payment);
    }

    private static PaymentResponse MapToResponse(Payment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        CustomerId = p.CustomerId,
        Amount = p.Amount,
        Method = p.Method,
        Status = p.Status,
        GatewayRef = p.GatewayRef,
        PaidAt = p.PaidAt,
        CreatedAt = p.CreatedAt
    };
}
