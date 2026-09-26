namespace EmergencyHomeServiceMobile.Models;

public class WorkerAvailableRequest
{
    public long RequestId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? Address { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public double DistanceKm { get; set; }
}