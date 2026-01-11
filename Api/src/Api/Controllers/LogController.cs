using GesFer.Application.Commands.Log;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Log;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para gestión de logs del sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")] // Requiere rol Admin
public class LogController : ControllerBase
{
    private readonly ICommandHandler<GetLogsCommand, LogsPagedResponseDto> _getLogsHandler;
    private readonly ILogger<LogController> _logger;

    public LogController(
        ICommandHandler<GetLogsCommand, LogsPagedResponseDto> getLogsHandler,
        ILogger<LogController> logger)
    {
        _getLogsHandler = getLogsHandler;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene logs paginados con filtros opcionales
    /// </summary>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="level">Nivel del log: Debug, Information, Warning, Error, Fatal (opcional)</param>
    /// <param name="companyId">ID de la empresa (opcional)</param>
    /// <param name="userId">ID del usuario (opcional)</param>
    /// <param name="pageNumber">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto: 50)</param>
    /// <returns>Lista paginada de logs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(LogsPagedResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? level = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // Validar parámetros
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1 || pageSize > 1000)
                pageSize = 50;

            var command = new GetLogsCommand
            {
                FromDate = fromDate,
                ToDate = toDate,
                Level = level,
                CompanyId = companyId,
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _getLogsHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener logs");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}
