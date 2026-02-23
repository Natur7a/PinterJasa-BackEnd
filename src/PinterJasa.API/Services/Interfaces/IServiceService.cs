using PinterJasa.API.DTOs.Services;

namespace PinterJasa.API.Services.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<ServiceResponse>> GetActiveServicesAsync(Guid? categoryId);
    Task<ServiceResponse> GetByIdAsync(Guid id);
    Task<ServiceResponse> CreateAsync(Guid providerId, CreateServiceRequest request);
    Task<IEnumerable<ServiceResponse>> GetMyServicesAsync(Guid providerId);
}
