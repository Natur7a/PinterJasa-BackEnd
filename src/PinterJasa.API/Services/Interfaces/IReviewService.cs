using PinterJasa.API.DTOs.Reviews;

namespace PinterJasa.API.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewResponse> CreateReviewAsync(Guid reviewerId, CreateReviewRequest request);
    Task<IEnumerable<ReviewResponse>> GetProviderReviewsAsync(Guid providerId);
}
