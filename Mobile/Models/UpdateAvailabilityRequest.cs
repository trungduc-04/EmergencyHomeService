using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models;

public class UpdateAvailabilityRequest
{
    public bool IsOnline { get; set; }

    public bool IsAvailable { get; set; }
}