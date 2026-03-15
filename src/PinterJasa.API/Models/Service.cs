using PinterJasa.API.Models.Interfaces;

namespace PinterJasa.API.Models;

public class Service : ISoftDeletable, IHasUpdatedAt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProviderId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string PriceUnit { get; set; } = "per_job";
    public string Status { get; set; } = "active";
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Provider Provider { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
