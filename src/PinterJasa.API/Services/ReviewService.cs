using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Data;
using PinterJasa.API.DTOs.Reviews;
using PinterJasa.API.Models;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ReviewResponse> CreateReviewAsync(Guid reviewerId, CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating must be between 1 and 5.");

        var order = await _db.Orders.FindAsync(request.OrderId)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} not found.");

        if (order.CustomerId != reviewerId)
            throw new UnauthorizedAccessException("Only the customer who placed the order can review it.");

        if (order.Status != "completed")
            throw new InvalidOperationException("Order must be completed before leaving a review.");

        if (await _db.Reviews.AnyAsync(r => r.OrderId == request.OrderId))
            throw new InvalidOperationException("A review already exists for this order.");

        var review = new Review
        {
            OrderId = request.OrderId,
            ReviewerId = reviewerId,
            ProviderId = order.ProviderId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        // Recalculate provider average rating
        var provider = await _db.Providers.FindAsync(order.ProviderId);
        if (provider != null)
        {
            var allRatings = await _db.Reviews
                .Where(r => r.ProviderId == provider.Id)
                .Select(r => r.Rating)
                .ToListAsync();

            provider.TotalReviews = allRatings.Count;
            provider.AverageRating = allRatings.Count > 0
                ? Math.Round((decimal)allRatings.Sum() / allRatings.Count, 2)
                : 0;
            provider.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        await _db.Entry(review).Reference(r => r.Reviewer).LoadAsync();
        return MapToResponse(review);
    }

    public async Task<IEnumerable<ReviewResponse>> GetProviderReviewsAsync(Guid providerId)
    {
        var reviews = await _db.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.ProviderId == providerId)
            .ToListAsync();
        return reviews.Select(MapToResponse);
    }

    private static ReviewResponse MapToResponse(Review r) => new()
    {
        Id = r.Id,
        OrderId = r.OrderId,
        ReviewerId = r.ReviewerId,
        ReviewerName = r.Reviewer?.Name ?? string.Empty,
        ProviderId = r.ProviderId,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };
}
