using PinterJasa.API.Models.Interfaces;

namespace PinterJasa.API.Models;

public class Payment : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public bool IsDeleted { get; set; } = false;
    public string? GatewayRef { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? XenditInvoiceId { get; set; }
    public string? XenditInvoiceUrl { get; set; }

    public Order Order { get; set; } = null!;
    public User Customer { get; set; } = null!;
}
