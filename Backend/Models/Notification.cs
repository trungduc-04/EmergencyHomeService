using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("UserId", "IsRead", "CreatedAt", Name = "IX_Notifications_User_IsRead_CreatedAt", IsDescending = new[] { false, false, true })]
public partial class Notification
{
    [Key]
    public long NotificationId { get; set; }

    public int UserId { get; set; }

    public long? RequestId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(1000)]
    public string Message { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Type { get; set; } = null!;

    public bool IsRead { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("Notifications")]
    public virtual ServiceRequest? Request { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User User { get; set; } = null!;
}
