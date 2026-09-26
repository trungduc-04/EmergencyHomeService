using System;
using System.Collections.Generic;
using System.Text;

namespace EmergencyHomeServiceMobile.Models
{
    public class RatingResponse
    {
        public long RatingId { get; set; }

        public long RequestId { get; set; }

        public int CustomerId { get; set; }

        public int WorkerId { get; set; }

        public int Score { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
