namespace PinterJasa.API.DTOs.Xendit;

public class XenditInvoiceRequest
{
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PayerEmail { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SuccessRedirectUrl { get; set; } = string.Empty;
    public string FailureRedirectUrl { get; set; } = string.Empty;
    public string Currency { get; set; } = "IDR";
    public int InvoiceDuration { get; set; } = 86400;
}
