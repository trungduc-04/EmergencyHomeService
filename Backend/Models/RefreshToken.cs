using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

[Index("UserId", Name = "IX_RefreshTokens_User")]
[Index("Token", Name = "UQ_RefreshTokens_Token", IsUnique = true)]
public partial class RefreshToken
{
    [Key]
    public long RefreshTokenId { get; set; }

    public int UserId { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string Token { get; set; } = null!;

    [Precision(0)]
    public DateTime ExpiresAt { get; set; }

    [Precision(0)]
    public DateTime? RevokedAt { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual User User { get; set; } = null!;
}
