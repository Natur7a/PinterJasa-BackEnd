namespace PinterJasa.API.DTOs.Orders;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
