using GesFer.Application.DTOs.Log;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Text.Json;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para recibir logs del frontend (telemetría)
/// </summary>
[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(ILogger<TelemetryController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Recibe logs estructurados del frontend y los procesa con Serilog
    /// </summary>
    /// <param name="logDto">Log estructurado del frontend</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ReceiveLog([FromBody] CreateLogDto logDto)
    {
        try
        {
            // Mapear nivel numérico de Pino a LogEventLevel de Serilog
            var logLevel = MapPinoLevelToSerilogLevel(logDto.Level);

            // Serializar propiedades adicionales si existen
            string? propertiesJson = null;
            if (logDto.Properties != null && logDto.Properties.Any())
            {
                propertiesJson = JsonSerializer.Serialize(logDto.Properties);
            }

            // Serializar información del cliente si existe
            string? clientInfoJson = null;
            if (logDto.ClientInfo != null && logDto.ClientInfo.Any())
            {
                clientInfoJson = JsonSerializer.Serialize(logDto.ClientInfo);
            }

            // Obtener información del contexto HTTP
            var companyId = User.FindFirst("CompanyId")?.Value;
            var userId = User.FindFirst("UserId")?.Value;

            // Crear contexto enriquecido para el log
            using (LogContext.PushProperty("Source", logDto.Source ?? "Frontend"))
            using (LogContext.PushProperty("Properties", propertiesJson))
            using (LogContext.PushProperty("ClientInfo", clientInfoJson))
            using (LogContext.PushProperty("CompanyId", companyId))
            using (LogContext.PushProperty("UserId", userId))
            {
                // Escribir el log usando Serilog
                if (!string.IsNullOrEmpty(logDto.Exception))
                {
                    // Si hay excepción, usar el método con excepción
                    var exception = new Exception(logDto.Exception);
                    Log.Write(logLevel, exception, logDto.Message);
                }
                else
                {
                    // Log normal sin excepción
                    Log.Write(logLevel, logDto.Message);
                }
            }

            return Ok(new { message = "Log recibido correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar log de telemetría");
            return BadRequest(new { message = "Error al procesar el log", error = ex.Message });
        }
    }

    /// <summary>
    /// Mapea el nivel numérico de Pino al LogEventLevel de Serilog
    /// </summary>
    /// <param name="pinoLevel">Nivel numérico de Pino (10=Trace, 20=Debug, 30=Info, 40=Warn, 50=Error, 60=Fatal)</param>
    /// <returns>LogEventLevel correspondiente</returns>
    private static LogEventLevel MapPinoLevelToSerilogLevel(int pinoLevel)
    {
        return pinoLevel switch
        {
            10 => LogEventLevel.Verbose,      // Trace
            20 => LogEventLevel.Debug,         // Debug
            30 => LogEventLevel.Information,   // Info
            40 => LogEventLevel.Warning,       // Warn
            50 => LogEventLevel.Error,         // Error
            60 => LogEventLevel.Fatal,         // Fatal
            _ => LogEventLevel.Information     // Por defecto Information
        };
    }
}
