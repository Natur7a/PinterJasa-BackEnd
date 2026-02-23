namespace PinterJasa.API.Models;

public class Payout
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ProviderId { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
}
