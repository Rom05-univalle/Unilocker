using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("AuditLog")]
public partial class AuditLog
{
    [Key]
    public long Id { get; set; } //el id

    [StringLength(50)]
    public string AffectedTable { get; set; } = null!;

    public int RecordId { get; set; }

    [StringLength(10)]
    public string ActionType { get; set; } = null!;

    public int? ResponsibleUserId { get; set; }

    public DateTime ActionDate { get; set; }

    public string? ChangeDetails { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("AuditLogs")]
    public virtual User? ResponsibleUser { get; set; }
}
