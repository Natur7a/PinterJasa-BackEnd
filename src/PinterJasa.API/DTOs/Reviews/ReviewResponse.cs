namespace PinterJasa.API.DTOs.Reviews;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
