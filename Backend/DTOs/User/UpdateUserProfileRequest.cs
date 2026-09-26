namespace EmergencyHomeService.API.DTOs.User;

public class UpdateUserProfileRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }
}