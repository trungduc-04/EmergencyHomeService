using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class WorkerAvailableRequestsResponse
{
    public int Total { get; set; }

    public double SearchRadiusKm { get; set; }

    public List<WorkerAvailableRequest> Requests { get; set; } = new();
}
