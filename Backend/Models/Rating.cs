using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("RequestId", Name = "UQ_Ratings_Request", IsUnique = true)]
public partial class Rating
{
    [Key]
    public long RatingId { get; set; }

    public long RequestId { get; set; }

    public int CustomerId { get; set; }

    public int WorkerId { get; set; }

    public int Score { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Ratings")]
    public virtual CustomerProfile Customer { get; set; } = null!;

    [ForeignKey("RequestId")]
    [InverseProperty("Rating")]
    public virtual ServiceRequest Request { get; set; } = null!;

    [ForeignKey("WorkerId")]
    [InverseProperty("Ratings")]
    public virtual WorkerProfile Worker { get; set; } = null!;
}
