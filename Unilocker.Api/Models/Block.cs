using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("Block")]
public partial class Block
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Address { get; set; }

    public int BranchId { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedUpdatedBy { get; set; }

    [ForeignKey("BranchId")]
    [InverseProperty("Blocks")]
    public virtual Branch Branch { get; set; } = null!;

    [InverseProperty("Block")]
    public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}
