namespace PinterJasa.API.DTOs.Reviews;

public class CreateReviewRequest
{
    public Guid OrderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
