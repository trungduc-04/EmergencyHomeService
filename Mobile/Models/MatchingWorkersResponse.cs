using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class MatchingWorkersResponse
{
    public long RequestId { get; set; }

    public int ServiceId { get; set; }

    public string Status { get; set; } = string.Empty;

    public double SearchRadiusKm { get; set; }

    public int Total { get; set; }

    public List<MatchingWorker> Workers { get; set; } = new();
}
