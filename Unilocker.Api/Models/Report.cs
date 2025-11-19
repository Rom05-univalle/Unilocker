using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("Report")]
public partial class Report
{
    [Key]
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int ProblemTypeId { get; set; }

    [StringLength(1000)]
    public string Description { get; set; } = null!;

    public DateTime ReportDate { get; set; }

    [StringLength(20)]
    public string ReportStatus { get; set; } = null!;

    public DateTime? ResolutionDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ProblemTypeId")]
    [InverseProperty("Reports")]
    public virtual ProblemType ProblemType { get; set; } = null!;

    [ForeignKey("SessionId")]
    [InverseProperty("Reports")]
    public virtual Session Session { get; set; } = null!;
}
