using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("Classroom")]
public partial class Classroom
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public int? Capacity { get; set; }

    public int BlockId { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedUpdatedBy { get; set; }

    [ForeignKey("BlockId")]
    [InverseProperty("Classrooms")]
    public virtual Block Block { get; set; } = null!;

    [InverseProperty("Classroom")]
    public virtual ICollection<Computer> Computers { get; set; } = new List<Computer>();
}
