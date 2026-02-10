namespace GesFer.Infrastructure.Logging;

/// <summary>
/// Interfaz para publicar logs de forma asíncrona a la API de Admin
/// </summary>
public interface IAsyncLogPublisher
{
    /// <summary>
    /// Publica un log de forma asíncrona
    /// </summary>
    /// <param name="level">Nivel del log (Debug, Information, Warning, Error, Fatal)</param>
    /// <param name="message">Mensaje del log</param>
    /// <param name="exception">Excepción opcional</param>
    /// <param name="properties">Propiedades adicionales del log</param>
    /// <returns>Tarea asíncrona</returns>
    Task PublishLogAsync(string level, string message, Exception? exception, Dictionary<string, object> properties);

    /// <summary>
    /// Publica un log de auditoría de forma asíncrona
    /// </summary>
    /// <param name="cursorId">ID del cursor del administrador</param>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="action">Acción realizada</param>
    /// <param name="httpMethod">Método HTTP</param>
    /// <param name="path">Ruta del endpoint</param>
    /// <param name="additionalData">Datos adicionales</param>
    /// <returns>Tarea asíncrona</returns>
    Task PublishAuditLog(string cursorId, string username, string action, string httpMethod, string path, string? additionalData = null);
}
