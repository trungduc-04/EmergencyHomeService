using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class UpdateWorkerLocationRequest
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }
}