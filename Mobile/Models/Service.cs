using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models
{
    public class Service
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? IconUrl { get; set; }
    }
}
