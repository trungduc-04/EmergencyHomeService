using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("RequestId", "CreatedAt", Name = "IX_RequestStatusHistories_Request_CreatedAt")]
public partial class RequestStatusHistory
{
    [Key]
    public long HistoryId { get; set; }

    public long RequestId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [StringLength(500)]
    public string? Note { get; set; }

    public int ChangedBy { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("RequestStatusHistories")]
    public virtual User ChangedByNavigation { get; set; } = null!;

    [ForeignKey("RequestId")]
    [InverseProperty("RequestStatusHistories")]
    public virtual ServiceRequest Request { get; set; } = null!;
}
