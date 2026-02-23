using PinterJasa.API.DTOs.Payouts;

namespace PinterJasa.API.Services.Interfaces;

public interface IPayoutService
{
    Task<PayoutResponse> CreatePayoutAsync(Guid orderId);
    Task<PayoutResponse> ProcessPayoutAsync(Guid payoutId);
    Task<IEnumerable<PayoutResponse>> GetMyPayoutsAsync(Guid providerId);
}
