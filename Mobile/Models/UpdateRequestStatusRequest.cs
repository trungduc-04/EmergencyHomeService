using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models
{
    public class UpdateRequestStatusRequest
    {
        public string Status { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}
