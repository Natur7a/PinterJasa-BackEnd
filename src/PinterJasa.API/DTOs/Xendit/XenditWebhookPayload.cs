namespace PinterJasa.API.DTOs.Xendit;

public class XenditWebhookPayload
{
    public string Id { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public string PayerEmail { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentChannel { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string Currency { get; set; } = string.Empty;
}
