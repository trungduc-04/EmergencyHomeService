namespace EmergencyHomeService.API.DTOs.Worker;

public class WorkerProfileResponse
{
    public int WorkerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public int ExperienceYears { get; set; }

    public bool IsOnline { get; set; }

    public bool IsAvailable { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalJobs { get; set; }

    public bool IsVerified { get; set; }

    public List<ServiceResponse> Services { get; set; } = [];
}

public class ServiceResponse
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;
}