namespace GesFer.Infrastructure.Services;

/// <summary>
/// Servicio para registrar logs de auditoría administrativa
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Registra un log de auditoría con el Cursor ID del administrador
    /// </summary>
    Task LogActionAsync(string cursorId, string username, string action, string httpMethod, string path, string? additionalData = null);
}
