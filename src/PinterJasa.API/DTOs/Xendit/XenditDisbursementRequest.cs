namespace PinterJasa.API.DTOs.Xendit;

public class XenditDisbursementRequest
{
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
