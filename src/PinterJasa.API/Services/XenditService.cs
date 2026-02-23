using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PinterJasa.API.Configuration;
using PinterJasa.API.DTOs.Xendit;
using PinterJasa.API.Services.Interfaces;

namespace PinterJasa.API.Services;

public class XenditService : IXenditService
{
    private readonly HttpClient _httpClient;
    private readonly XenditConfig _config;
    private readonly ILogger<XenditService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public XenditService(HttpClient httpClient, IOptions<XenditConfig> config, ILogger<XenditService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.SecretApiKey}:"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
    }

    public async Task<XenditInvoiceResponse> CreateInvoiceAsync(XenditInvoiceRequest request)
    {
        var payload = new
        {
            external_id = request.ExternalId,
            amount = request.Amount,
            payer_email = request.PayerEmail,
            description = request.Description,
            success_redirect_url = request.SuccessRedirectUrl,
            failure_redirect_url = request.FailureRedirectUrl,
            currency = request.Currency,
            invoice_duration = request.InvoiceDuration
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v2/invoices", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Xendit CreateInvoice failed: {StatusCode} {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Xendit invoice creation failed: {response.StatusCode}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<XenditInvoiceResponse>(responseJson, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize Xendit invoice response.");

        return result;
    }

    public async Task<XenditDisbursementResponse> CreateDisbursementAsync(XenditDisbursementRequest request)
    {
        var payload = new
        {
            external_id = request.ExternalId,
            amount = request.Amount,
            bank_code = request.BankCode,
            account_holder_name = request.AccountHolderName,
            account_number = request.AccountNumber,
            description = request.Description
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/disbursements", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Xendit CreateDisbursement failed: {StatusCode} {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Xendit disbursement creation failed: {response.StatusCode}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<XenditDisbursementResponse>(responseJson, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize Xendit disbursement response.");

        return result;
    }

    public bool VerifyWebhookToken(string token)
    {
        return string.Equals(token, _config.WebhookVerificationToken, StringComparison.Ordinal);
    }
}
