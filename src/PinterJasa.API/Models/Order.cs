namespace PinterJasa.API.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid ProviderId { get; set; }
    public string Status { get; set; } = "created";
    public decimal TotalPrice { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User Customer { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
    public Payment? Payment { get; set; }
    public Payout? Payout { get; set; }
    public Review? Review { get; set; }
}
