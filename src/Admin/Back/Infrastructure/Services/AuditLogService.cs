using GesFer.Admin.Back.Domain.Entities;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GesFer.Admin.Infrastructure.Services;

/// <summary>
/// Servicio para registrar logs de auditoría administrativa
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(AdminDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Registra un log de auditoría con el Cursor ID del administrador
    /// Usa Sequential GUIDs para el Id del log
    /// </summary>
    public async Task LogActionAsync(string cursorId, string username, string action, string httpMethod, string path, string? additionalData = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                // El Id se generará automáticamente como Sequential GUID por ApplicationDbContext
                CursorId = cursorId,
                Username = username,
                Action = action,
                HttpMethod = httpMethod,
                Path = path,
                AdditionalData = additionalData,
                ActionTimestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log el error pero no fallar la operación principal
            _logger.LogError(ex, "Error al registrar log de auditoría para CursorId: {CursorId}, Action: {Action}", cursorId, action);
        }
    }
}
