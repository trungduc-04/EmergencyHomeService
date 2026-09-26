using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class WorkerLocationResponse
{
    public int WorkerId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTime UpdatedAt { get; set; }
}
