namespace PinterJasa.API.Models;

public class Provider
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public string? City { get; set; }
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Payout> Payouts { get; set; } = new List<Payout>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
