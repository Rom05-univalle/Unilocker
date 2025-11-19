using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Unilocker.Api.Models;

[Table("User")]
[Index("Username", Name = "UQ__User__536C85E4F2ACDD69", IsUnique = true)]
[Index("Email", Name = "UQ__User__A9D1053476EE5714", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string FirstName { get; set; } = null!;

    [StringLength(150)]
    public string LastName { get; set; } = null!;

    [StringLength(150)]
    public string? SecondLastName { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(256)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(20)]
    public string? Phone { get; set; }

    public int RoleId { get; set; }

    public DateTime? LastAccess { get; set; }

    public int? FailedLoginAttempts { get; set; }

    public bool? IsBlocked { get; set; }

    public bool Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedUpdatedBy { get; set; }

    [InverseProperty("ResponsibleUser")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
