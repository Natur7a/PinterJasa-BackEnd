using PinterJasa.API.DTOs.Payments;

namespace PinterJasa.API.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePaymentAsync(Guid customerId, Guid orderId, string method);
    Task<PaymentResponse> ConfirmPaymentAsync(Guid paymentId, string gatewayRef);
    Task<PaymentResponse> GetByOrderIdAsync(Guid orderId);
}
