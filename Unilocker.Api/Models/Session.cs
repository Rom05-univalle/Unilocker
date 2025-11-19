using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("Session")]
public partial class Session
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ComputerId { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? EndMethod { get; set; }

    public DateTime? LastHeartbeat { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ComputerId")]
    [InverseProperty("Sessions")]
    public virtual Computer Computer { get; set; } = null!;

    [InverseProperty("Session")]
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    [ForeignKey("UserId")]
    [InverseProperty("Sessions")]
    public virtual User User { get; set; } = null!;
}
