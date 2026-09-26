namespace EmergencyHomeService.API.DTOs.ServiceRequest;

public class UpdateRequestStatusRequest
{
    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }
}