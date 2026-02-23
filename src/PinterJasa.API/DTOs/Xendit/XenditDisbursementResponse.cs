namespace PinterJasa.API.DTOs.Xendit;

public class XenditDisbursementResponse
{
    public string Id { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
