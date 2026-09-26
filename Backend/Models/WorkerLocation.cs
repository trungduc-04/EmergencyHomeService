using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("WorkerId", Name = "UQ_WorkerLocations_Worker", IsUnique = true)]
public partial class WorkerLocation
{
    [Key]
    public long LocationId { get; set; }

    public int WorkerId { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal Longitude { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("WorkerId")]
    [InverseProperty("WorkerLocation")]
    public virtual WorkerProfile Worker { get; set; } = null!;
}
