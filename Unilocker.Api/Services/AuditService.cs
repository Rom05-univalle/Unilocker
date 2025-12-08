using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Unilocker.Api.Data;
using Unilocker.Api.Models;

namespace Unilocker.Api.Services;

/// <summary>
/// Servicio para registrar cambios automáticamente en AuditLog
/// </summary>
public class AuditService
{
    /// <summary>
    /// Crea registros de auditoría para los cambios detectados en el contexto
    /// </summary>
    public static List<AuditLog> CreateAuditLogs(
        DbContext context,
        int? userId,
        string? ipAddress)
    {
        var auditLogs = new List<AuditLog>();
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .Where(e => e.Entity.GetType().Name != "AuditLog") // No auditar los logs de auditoría
            .ToList();

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType();
            var tableName = entityType.Name;

            // Obtener el ID del registro
            var idProperty = entityType.GetProperty("Id");
            if (idProperty == null) continue;
            var recordId = idProperty.GetValue(entry.Entity);
            if (recordId == null) continue;

            string actionType = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => "UNKNOWN"
            };

            // Crear diccionario con los cambios
            var changes = new Dictionary<string, object>();
            changes["action"] = actionType.ToLower();

            if (entry.State == EntityState.Modified)
            {
                var modifiedProperties = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(
                        p => p.Metadata.Name,
                        p => new
                        {
                            OldValue = p.OriginalValue?.ToString() ?? "null",
                            NewValue = p.CurrentValue?.ToString() ?? "null"
                        }
                    );
                changes["modified"] = modifiedProperties;
            }
            else if (entry.State == EntityState.Added)
            {
                // Para nuevos registros, agregar algunos campos clave
                var properties = entry.Properties
                    .Where(p => p.Metadata.Name != "Id" && 
                               p.Metadata.Name != "CreatedAt" && 
                               p.Metadata.Name != "UpdatedAt" &&
                               p.CurrentValue != null)
                    .Take(3) // Solo los primeros 3 campos para no hacer el JSON muy grande
                    .ToDictionary(
                        p => p.Metadata.Name,
                        p => p.CurrentValue?.ToString() ?? ""
                    );
                if (properties.Any())
                {
                    changes["data"] = properties;
                }
            }

            var auditLog = new AuditLog
            {
                AffectedTable = tableName,
                RecordId = Convert.ToInt32(recordId),
                ActionType = actionType,
                ResponsibleUserId = userId,
                ActionDate = DateTime.Now,
                ChangeDetails = JsonSerializer.Serialize(changes),
                IpAddress = ipAddress ?? "unknown"
            };

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }
}
