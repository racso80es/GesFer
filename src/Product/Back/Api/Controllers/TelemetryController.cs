using Microsoft.AspNetCore.Mvc;
using GesFer.Infrastructure.Logging;
using System.Text.Json;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para recibir logs del frontend (telemetría)
/// Los logs se envían a Admin API mediante AsyncLogPublisher
/// </summary>
[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ILogger<TelemetryController> _logger;
    private readonly IAsyncLogPublisher _logPublisher;

    public TelemetryController(ILogger<TelemetryController> logger, IAsyncLogPublisher logPublisher)
    {
        _logger = logger;
        _logPublisher = logPublisher;
    }

    /// <summary>
    /// DTO para recibir logs del frontend
    /// </summary>
    public class CreateLogDto
    {
        public int Level { get; set; }           // Nivel numérico de Pino
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
        public string? Source { get; set; }
        public Dictionary<string, object>? ClientInfo { get; set; }
    }

    /// <summary>
    /// Recibe logs estructurados del frontend y los envía a Admin API
    /// </summary>
    /// <param name="logDto">Log estructurado del frontend</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceiveLog([FromBody] CreateLogDto logDto)
    {
        try
        {
            // Mapear nivel numérico de Pino a string de nivel
            var level = MapPinoLevelToString(logDto.Level);

            // Convertir propiedades a Dictionary si existen
            var properties = new Dictionary<string, object>();
            if (logDto.Properties != null)
            {
                foreach (var prop in logDto.Properties)
                {
                    properties[prop.Key] = prop.Value;
                }
            }
            if (logDto.ClientInfo != null)
            {
                properties["ClientInfo"] = JsonSerializer.Serialize(logDto.ClientInfo);
            }
            if (!string.IsNullOrEmpty(logDto.Source))
            {
                properties["Source"] = logDto.Source;
            }

            // Obtener información del contexto HTTP
            var companyId = User.FindFirst("CompanyId")?.Value;
            var userId = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(companyId))
            {
                properties["CompanyId"] = companyId;
            }
            if (!string.IsNullOrEmpty(userId))
            {
                properties["UserId"] = userId;
            }

            // Convertir excepción si existe
            Exception? exception = null;
            if (!string.IsNullOrEmpty(logDto.Exception))
            {
                exception = new Exception(logDto.Exception);
            }

            // Enviar log a Admin API mediante AsyncLogPublisher (Awaitable)
            await _logPublisher.PublishLogAsync(level, logDto.Message, exception, properties);

            return Ok(new { message = "Log recibido correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar log de telemetría");
            return BadRequest(new { message = "Error al procesar el log", error = ex.Message });
        }
    }

    /// <summary>
    /// Mapea el nivel numérico de Pino a string de nivel
    /// </summary>
    /// <param name="pinoLevel">Nivel numérico de Pino (10=Trace, 20=Debug, 30=Info, 40=Warn, 50=Error, 60=Fatal)</param>
    /// <returns>String de nivel correspondiente</returns>
    private static string MapPinoLevelToString(int pinoLevel)
    {
        return pinoLevel switch
        {
            10 => "Debug",        // Trace -> Debug
            20 => "Debug",        // Debug
            30 => "Information",  // Info
            40 => "Warning",      // Warn
            50 => "Error",        // Error
            60 => "Fatal",        // Fatal
            _ => "Information"    // Por defecto Information
        };
    }
}
