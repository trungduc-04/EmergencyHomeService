using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

public partial class CustomerProfile
{
    [Key]
    public int CustomerId { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerProfile")]
    public virtual User Customer { get; set; } = null!;

    [InverseProperty("Customer")]
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [InverseProperty("Customer")]
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
