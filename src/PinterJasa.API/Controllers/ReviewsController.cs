using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinterJasa.API.DTOs.Reviews;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var review = await _reviewService.CreateReviewAsync(userId, request);
        return Ok(review);
    }

    [HttpGet("provider/{providerId:guid}")]
    public async Task<IActionResult> GetByProvider(Guid providerId)
    {
        var reviews = await _reviewService.GetProviderReviewsAsync(providerId);
        return Ok(reviews);
    }
}
