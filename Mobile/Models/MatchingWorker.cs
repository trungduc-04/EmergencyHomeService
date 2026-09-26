using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class MatchingWorker
{
    public int WorkerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalJobs { get; set; }

    public bool IsOnline { get; set; }

    public bool IsAvailable { get; set; }

    public double DistanceKm { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
