using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Models;

public partial class RequestImage
{
    [Key]
    public long ImageId { get; set; }

    public long RequestId { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string ImageUrl { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("RequestImages")]
    public virtual ServiceRequest Request { get; set; } = null!;
}
