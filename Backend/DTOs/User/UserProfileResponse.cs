namespace EmergencyHomeService.API.DTOs.User;

public class UserProfileResponse
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }
}