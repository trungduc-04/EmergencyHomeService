using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("ServiceName", Name = "UQ_Services_ServiceName", IsUnique = true)]
public partial class Service
{
    [Key]
    public int ServiceId { get; set; }

    [StringLength(100)]
    public string ServiceName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? IconUrl { get; set; }

    public bool IsActive { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Service")]
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    [InverseProperty("Service")]
    public virtual ICollection<WorkerService> WorkerServices { get; set; } = new List<WorkerService>();
}
