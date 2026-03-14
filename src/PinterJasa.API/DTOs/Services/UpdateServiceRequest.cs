using System.ComponentModel.DataAnnotations;

namespace PinterJasa.API.DTOs.Services;

public class UpdateServiceRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal? Price { get; set; }

    [MaxLength(20)]
    public string? PriceUnit { get; set; }

    public string? Status { get; set; }
}
