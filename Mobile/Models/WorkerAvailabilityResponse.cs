using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class WorkerAvailabilityResponse
{
    public bool Success { get; set; }

    public bool IsOnline { get; set; }

    public bool IsAvailable { get; set; }
}
