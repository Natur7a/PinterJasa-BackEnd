namespace PinterJasa.API.DTOs.Services;

public class CreateServiceRequest
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = "per_job";
}
