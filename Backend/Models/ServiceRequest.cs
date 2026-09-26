using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("CustomerId", "CreatedAt", Name = "IX_ServiceRequests_Customer_CreatedAt", IsDescending = new[] { false, true })]
[Index("ServiceId", "Status", Name = "IX_ServiceRequests_Service_Status")]
[Index("WorkerId", "Status", Name = "IX_ServiceRequests_Worker_Status")]
public partial class ServiceRequest
{
    [Key]
    public long RequestId { get; set; }

    public int CustomerId { get; set; }

    public int? WorkerId { get; set; }

    public int ServiceId { get; set; }

    [StringLength(2000)]
    public string Description { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column(TypeName = "decimal(10, 7)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal Longitude { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Precision(0)]
    public DateTime? ScheduledAt { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? AcceptedAt { get; set; }

    [Precision(0)]
    public DateTime? CompletedAt { get; set; }

    [Precision(0)]
    public DateTime? CancelledAt { get; set; }

    [StringLength(500)]
    public string? CancellationReason { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("ServiceRequests")]
    public virtual CustomerProfile Customer { get; set; } = null!;

    [InverseProperty("Request")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Request")]
    public virtual Rating? Rating { get; set; }

    [InverseProperty("Request")]
    public virtual ICollection<RequestImage> RequestImages { get; set; } = new List<RequestImage>();

    [InverseProperty("Request")]
    public virtual ICollection<RequestStatusHistory> RequestStatusHistories { get; set; } = new List<RequestStatusHistory>();

    [ForeignKey("ServiceId")]
    [InverseProperty("ServiceRequests")]
    public virtual Service Service { get; set; } = null!;

    [ForeignKey("WorkerId")]
    [InverseProperty("ServiceRequests")]
    public virtual WorkerProfile? Worker { get; set; }
}
