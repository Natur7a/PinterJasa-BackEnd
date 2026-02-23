namespace PinterJasa.API.DTOs.Orders;

public class CreateOrderRequest
{
    public Guid ServiceId { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime? ScheduledAt { get; set; }
}
