using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("ProblemType")]
[Index("Name", Name = "UQ__ProblemT__737584F636E9F078", IsUnique = true)]
public partial class ProblemType
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Description { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedUpdatedBy { get; set; }

    [InverseProperty("ProblemType")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
