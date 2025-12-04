using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Unilocker.Api.Models;

namespace Unilocker.Api.Data;

public partial class UnilockerDbContext : DbContext
{
    public UnilockerDbContext()
    {
    }

    public UnilockerDbContext(DbContextOptions<UnilockerDbContext> options)
        : base(options)
    {
    }

    // DbSets tipados (tablas en singular según tu DB)
    public virtual DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public virtual DbSet<Block> Blocks { get; set; } = null!;
    public virtual DbSet<Branch> Branches { get; set; } = null!;
    public virtual DbSet<Classroom> Classrooms { get; set; } = null!;
    public virtual DbSet<Computer> Computers { get; set; } = null!;
    public virtual DbSet<ProblemType> ProblemTypes { get; set; } = null!;
    public virtual DbSet<Report> Reports { get; set; } = null!;
    public virtual DbSet<Role> Roles { get; set; } = null!;
    public virtual DbSet<Session> Sessions { get; set; } = null!;
    public virtual DbSet<User> Users { get; set; } = null!;
    public DbSet<TwoFactorCode> TwoFactorCodes { get; set; } = null!;


    // Contexto para auditoría
    public int? CurrentUserId { get; set; }
    public string? CurrentIpAddress { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     // conexión movida a Program.cs (recomendado)
    // }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AUDITLOG -> tabla AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC07DCFD8195");

            entity.ToTable("AuditLog");

            entity.Property(e => e.ActionDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ResponsibleUser)
                .WithMany(p => p.AuditLogs)
                .HasConstraintName("FK_AuditLog_User");
        });

        // BLOCK -> tabla Block
        modelBuilder.Entity<Block>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Block__3214EC07BA54E2FC");

            entity.ToTable("Block");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.Branch)
                .WithMany(p => p.Blocks)
                .HasConstraintName("FK_Block_Branch");
        });

        // BRANCH -> tabla Branch
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Branch__3214EC0790964C8F");

            entity.ToTable("Branch");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        // CLASSROOM -> tabla Classroom
        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Classroo__3214EC07524541AF");

            entity.ToTable("Classroom");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.Block)
                .WithMany(p => p.Classrooms)
                .HasConstraintName("FK_Classroom_Block");
        });

        // COMPUTER -> tabla Computer
        modelBuilder.Entity<Computer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Computer__3214EC07E0F3E795");

            entity.ToTable("Computer");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Uuid).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Classroom)
                .WithMany(p => p.Computers)
                .HasConstraintName("FK_Computer_Classroom");
        });

        // PROBLEMTYPE -> tabla ProblemType
        modelBuilder.Entity<ProblemType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProblemT__3214EC079C5CF980");

            entity.ToTable("ProblemType");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        // REPORT -> tabla Report
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Report__3214EC07443CDE24");

            entity.ToTable("Report");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ReportDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ReportStatus).HasDefaultValue("Pending");

            entity.HasOne(d => d.ProblemType)
                .WithMany(p => p.Reports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Report_ProblemType");

            entity.HasOne(d => d.Session)
                .WithMany(p => p.Reports)
                .HasConstraintName("FK_Report_Session");
        });

        // ROLE -> tabla Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC0786C3BB01");

            entity.ToTable("Role");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        // SESSION -> tabla Session
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Session__3214EC073E25DBE1");

            entity.ToTable("Session");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StartDateTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Computer)
                .WithMany(p => p.Sessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Session_Computer");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Sessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Session_User");
        });

        // USER -> tabla User (singular)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC073D0C19FF");

            entity.ToTable("User"); // ← IMPORTANTE: coincide con tu BD

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.IsBlocked).HasDefaultValue(false);
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    // ================== AUDITORÍA AUTOMÁTICA ==================

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog) continue;

            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                var entityType = entry.Entity.GetType();
                var tableName = entityType.Name;

                var audit = new AuditLog
                {
                    AffectedTable = tableName,
                    ActionType = entry.State.ToString(), // Added / Modified / Deleted
                    ActionDate = DateTime.UtcNow,
                    ResponsibleUserId = CurrentUserId,
                    IpAddress = CurrentIpAddress
                };

                // Id del registro afectado (si tiene propiedad Id)
                var idProp = entityType.GetProperty("Id");
                if (idProp != null)
                {
                    var idValue = idProp.GetValue(entry.Entity);
                    if (idValue is int intId)
                        audit.RecordId = intId;
                    else if (idValue is long longId)
                        audit.RecordId = (int)longId;
                }

                audit.ChangeDetails = BuildChangeDetails(entry);

                auditEntries.Add(audit);
            }
        }

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        CurrentUserId = null;
        CurrentIpAddress = null;

        return result;
    }

    private static string? BuildChangeDetails(EntityEntry entry)
    {
        try
        {
            var changes = new Dictionary<string, object?>();

            foreach (var prop in entry.Properties)
            {
                if (prop.IsTemporary) continue;
                var name = prop.Metadata.Name;

                if (entry.State == EntityState.Added)
                {
                    changes[name] = prop.CurrentValue;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    changes[name] = prop.OriginalValue;
                }
                else if (entry.State == EntityState.Modified && !Equals(prop.OriginalValue, prop.CurrentValue))
                {
                    changes[name] = new
                    {
                        Old = prop.OriginalValue,
                        New = prop.CurrentValue
                    };
                }
            }

            if (changes.Count == 0) return null;

            return System.Text.Json.JsonSerializer.Serialize(changes);
        }
        catch
        {
            return null;
        }
    }
}
