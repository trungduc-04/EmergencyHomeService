namespace EmergencyHomeService.API.DTOs.Worker;

public class UpdateAvailabilityRequest
{
    public bool IsOnline { get; set; }

    public bool IsAvailable { get; set; }
}