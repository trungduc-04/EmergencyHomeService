using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class WorkerProfileResponse
{
    public int WorkerId { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public int ExperienceYears { get; set; }

    public bool IsOnline { get; set; }

    public bool IsAvailable { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalJobs { get; set; }

    public bool IsVerified { get; set; }

    public List<WorkerServiceItem> Services { get; set; } = new();
}

public class WorkerServiceItem
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = "";
}
