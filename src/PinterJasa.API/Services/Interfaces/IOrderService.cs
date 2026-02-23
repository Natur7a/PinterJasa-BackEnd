using PinterJasa.API.DTOs.Orders;

namespace PinterJasa.API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(Guid customerId, CreateOrderRequest request);
    Task<OrderResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(Guid customerId);
    Task<IEnumerable<OrderResponse>> GetProviderOrdersAsync(Guid providerId);
    Task<OrderResponse> UpdateStatusAsync(Guid orderId, string newStatus, Guid requesterId, string requesterRole);
}
