using PinterJasa.API.DTOs.Xendit;

namespace PinterJasa.API.Services.Interfaces;

public interface IXenditService
{
    Task<XenditInvoiceResponse> CreateInvoiceAsync(XenditInvoiceRequest request);
    Task<XenditDisbursementResponse> CreateDisbursementAsync(XenditDisbursementRequest request);
    bool VerifyWebhookToken(string token);
}
