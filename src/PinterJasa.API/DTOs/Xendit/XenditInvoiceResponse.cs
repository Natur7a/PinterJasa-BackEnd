namespace PinterJasa.API.DTOs.Xendit;

public class XenditInvoiceResponse
{
    public string Id { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string InvoiceUrl { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
}
