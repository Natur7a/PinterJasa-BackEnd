namespace PinterJasa.API.DTOs.Auth;

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = "customer";
    public string? Bio { get; set; }
    public string? City { get; set; }
}
