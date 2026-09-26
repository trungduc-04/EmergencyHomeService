using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models
{
    public class ServiceRequestResponse
    {
        public long RequestId { get; set; }

        public int CustomerId { get; set; }

        public int? WorkerId { get; set; }

        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? Address { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
