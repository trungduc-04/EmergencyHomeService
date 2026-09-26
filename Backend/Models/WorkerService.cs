using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[PrimaryKey("WorkerId", "ServiceId")]
[Index("ServiceId", "WorkerId", Name = "IX_WorkerServices_Service_Worker")]
public partial class WorkerService
{
    [Key]
    public int WorkerId { get; set; }

    [Key]
    public int ServiceId { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ServiceId")]
    [InverseProperty("WorkerServices")]
    public virtual Service Service { get; set; } = null!;

    [ForeignKey("WorkerId")]
    [InverseProperty("WorkerServices")]
    public virtual WorkerProfile Worker { get; set; } = null!;
}
