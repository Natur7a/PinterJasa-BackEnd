namespace PinterJasa.API.Models;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid ProviderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
}
