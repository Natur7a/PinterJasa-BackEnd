using PinterJasa.API.DTOs.Tracking;

namespace PinterJasa.API.Services.Interfaces;

public interface ITrackingService
{
    Task<LocationPingResponse> AddPingAsync(Guid orderId, Guid requesterId, TrackingPingRequest request);
    Task<LocationPingResponse> GetLatestPingAsync(Guid orderId, Guid requesterId, string requesterRole);
    Task<IEnumerable<LocationPingResponse>> GetPingHistoryAsync(Guid orderId, Guid requesterId, string requesterRole, int limit = 50);
}
