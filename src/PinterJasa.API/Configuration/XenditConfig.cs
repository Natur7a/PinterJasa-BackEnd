namespace PinterJasa.API.Configuration;

public class XenditConfig
{
    public string SecretApiKey { get; set; } = string.Empty;
    public string PublicApiKey { get; set; } = string.Empty;
    public string WebhookVerificationToken { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.xendit.co";
    public string SuccessRedirectUrl { get; set; } = string.Empty;
    public string FailureRedirectUrl { get; set; } = string.Empty;
}
