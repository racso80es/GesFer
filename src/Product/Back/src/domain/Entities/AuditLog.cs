using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad para registrar logs de auditoría del sistema administrativo
/// Cada acción administrativa debe ser registrada con el Cursor ID del administrador
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Cursor ID del administrador que realizó la acción (del token JWT)
    /// </summary>
    public string CursorId { get; set; } = string.Empty;

    /// <summary>
    /// Username del administrador que realizó la acción
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint o acción realizada
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Método HTTP (GET, POST, etc.)
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Ruta completa del endpoint
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Información adicional en formato JSON (opcional)
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Timestamp de cuando se realizó la acción
    /// </summary>
    public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;
}
