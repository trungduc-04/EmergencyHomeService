using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("Role", "IsActive", Name = "IX_Users_Role_IsActive")]
[Index("Email", Name = "UQ_Users_Email", IsUnique = true)]
[Index("Phone", Name = "UQ_Users_Phone", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(150)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Phone { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Role { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Customer")]
    public virtual CustomerProfile? CustomerProfile { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<RequestStatusHistory> RequestStatusHistories { get; set; } = new List<RequestStatusHistory>();

    [InverseProperty("Worker")]
    public virtual WorkerProfile? WorkerProfile { get; set; }
}
