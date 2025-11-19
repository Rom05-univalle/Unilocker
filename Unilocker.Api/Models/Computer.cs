using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("Computer")]
[Index("Uuid", Name = "UQ__Computer__65A475E665A2F2EB", IsUnique = true)]
public partial class Computer
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("UUID")]
    public Guid Uuid { get; set; }

    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [StringLength(50)]
    public string? Model { get; set; }

    public int ClassroomId { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedUpdatedBy { get; set; }

    [ForeignKey("ClassroomId")]
    [InverseProperty("Computers")]
    public virtual Classroom Classroom { get; set; } = null!;

    [InverseProperty("Computer")]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
