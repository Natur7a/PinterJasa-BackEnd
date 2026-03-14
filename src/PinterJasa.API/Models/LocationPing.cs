namespace PinterJasa.API.Models;

public class LocationPing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ProviderId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? AccuracyMeters { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
}
