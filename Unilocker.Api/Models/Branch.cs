using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("Branch")]
[Index("Code", Name = "UQ__Branch__A25C5AA779AFB938", IsUnique = true)]
public partial class Branch
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(10)]
    public string? Code { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedUpdatedBy { get; set; }

    [InverseProperty("Branch")]
    public virtual ICollection<Block> Blocks { get; set; } = new List<Block>();
}
