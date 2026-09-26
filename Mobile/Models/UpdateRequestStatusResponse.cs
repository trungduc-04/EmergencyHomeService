using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models
{
    public class UpdateRequestStatusResponse
    {
        public bool Success { get; set; }

        public long RequestId { get; set; }

        public int WorkerId { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? CompletedAt { get; set; }
    }
}
