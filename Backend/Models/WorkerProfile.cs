using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("IsOnline", "IsAvailable", Name = "IX_WorkerProfiles_Online_Available")]
public partial class WorkerProfile
{
    [Key]
    public int WorkerId { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    public int ExperienceYears { get; set; }

    public bool IsOnline { get; set; }

    public bool IsAvailable { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal AverageRating { get; set; }

    public int TotalJobs { get; set; }

    public bool IsVerified { get; set; }

    [InverseProperty("Worker")]
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [InverseProperty("Worker")]
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    [ForeignKey("WorkerId")]
    [InverseProperty("WorkerProfile")]
    public virtual User Worker { get; set; } = null!;

    [InverseProperty("Worker")]
    public virtual WorkerLocation? WorkerLocation { get; set; }

    [InverseProperty("Worker")]
    public virtual ICollection<WorkerService> WorkerServices { get; set; } = new List<WorkerService>();
}
