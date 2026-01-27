namespace GesFer.Admin.Back.Domain.Entities;

/// <summary>
/// Entidad que representa un log del sistema.
/// NO hereda de BaseEntity porque Serilog.Sinks.MySQL requiere Id INT AUTO_INCREMENT.
/// Esta entidad está diseñada específicamente para ser compatible con Serilog.
/// </summary>
public class Log
{
    /// <summary>
    /// Identificador único del log (INT AUTO_INCREMENT, administrado por MySQL/Serilog)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nivel del log (Debug, Information, Warning, Error, Fatal)
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje renderizado del log
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Template del mensaje (con placeholders) - usado por Serilog
    /// </summary>
    public string? Template { get; set; }

    /// <summary>
    /// Mensaje de excepción si existe
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Propiedades adicionales del log en formato JSON
    /// </summary>
    public string? Properties { get; set; }

    /// <summary>
    /// Timestamp del log (UTC) - Serilog usa TimeStamp con mayúscula
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fuente del log (ej: "GesFer.Api.Controllers.CustomerController")
    /// </summary>
    public string? Source { get; set; }

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
