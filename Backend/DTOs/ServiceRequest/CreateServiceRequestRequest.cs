namespace EmergencyHomeService.API.DTOs.ServiceRequest;

public class CreateServiceRequestRequest
{
    public int ServiceId { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string? Address { get; set; }

    public DateTime? ScheduledAt { get; set; }
}