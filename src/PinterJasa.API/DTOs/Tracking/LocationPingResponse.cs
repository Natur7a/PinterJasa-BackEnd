namespace PinterJasa.API.DTOs.Tracking;

public class LocationPingResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProviderId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? AccuracyMeters { get; set; }
    public DateTime TimestampUtc { get; set; }
}
