namespace PinterJasa.API.DTOs.Payouts;

public class PayoutResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProviderId { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
