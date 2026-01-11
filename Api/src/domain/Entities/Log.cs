using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un log del sistema
/// </summary>
public class Log : BaseEntity
{
    /// <summary>
    /// Nivel del log (Debug, Information, Warning, Error, Fatal)
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje del log
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje de excepción si existe
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Propiedades adicionales del log en formato JSON
    /// </summary>
    public string? Properties { get; set; }

    /// <summary>
    /// Fuente del log (ej: "GesFer.Api.Controllers.CustomerController")
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Timestamp del log (puede diferir de CreatedAt si el log viene del frontend)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID de la empresa si el log está asociado a un tenant
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// ID del usuario si el log está asociado a un usuario
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Información del cliente (User-Agent, IP, etc.) en formato JSON
    /// </summary>
    public string? ClientInfo { get; set; }
}
