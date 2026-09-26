using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class UserProfileResponse
{
    public int UserId { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Role { get; set; } = "";

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }
}
