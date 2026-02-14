using GesFer.Admin.Api.Attributes;
using GesFer.Admin.Application.Dtos.Logs;
using GesFer.Admin.Back.Domain.Entities;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GesFer.Admin.Api.Controllers;

/// <summary>
/// Controlador para gestión de logs del sistema
/// </summary>
[ApiController]
[Route("api/admin/logs")]
public class LogController : ControllerBase
{
    private readonly AdminDbContext _context;
    private readonly ILogger<LogController> _logger;

    public LogController(
        AdminDbContext context,
        ILogger<LogController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Recibe un log desde otros servicios (System)
    /// </summary>
    [HttpPost]
    [AuthorizeSystemOrAdmin]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReceiveLog([FromBody] CreateLogDto dto)
    {
        if (dto == null)
            return BadRequest(new { message = "El cuerpo de la petición es obligatorio" });
        if (string.IsNullOrWhiteSpace(dto.Level))
            return BadRequest(new { message = "Level es obligatorio" });
        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(new { message = "Message es obligatorio" });

        try
        {
            var log = new Log
            {
                Level = dto.Level,
                Message = dto.Message,
                Exception = dto.Exception,
                TimeStamp = dto.TimeStamp,
                Properties = dto.Properties != null ? JsonSerializer.Serialize(dto.Properties) : null,
                // Intentar extraer metadatos comunes de las propiedades si existen
                Source = ExtractProperty(dto.Properties, "SourceContext"),
                // CompanyId y UserId podrían venir en properties, pero por ahora lo dejamos simple
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar log recibido");
            // No devolvemos 500 para no afectar al emisor si es posible evitarlo,
            // pero el emisor (Product) ya hace fire-and-forget.
            return StatusCode(500, new { message = "Error interno al guardar log" });
        }
    }

    /// <summary>
    /// Recibe un log de auditoría desde otros servicios (System)
    /// </summary>
    [HttpPost]
    [Route("/api/admin/audit-logs")]
    [AuthorizeSystemOrAdmin]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReceiveAuditLog([FromBody] CreateAuditLogDto dto)
    {
        try
        {
            var auditLog = new AuditLog
            {
                CursorId = dto.CursorId,
                Username = dto.Username,
                Action = dto.Action,
                HttpMethod = dto.HttpMethod,
                Path = dto.Path,
                AdditionalData = dto.AdditionalData,
                ActionTimestamp = dto.ActionTimestamp
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar audit log recibido");
            return StatusCode(500, new { message = "Error interno al guardar audit log" });
        }
    }

    /// <summary>
    /// Obtiene logs paginados con filtros opcionales
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
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

            var query = _context.Logs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.TimeStamp >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(l => l.TimeStamp <= toDate.Value);
            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(l => l.Level == level);
            if (companyId.HasValue)
                query = query.Where(l => l.CompanyId == companyId.Value);
            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.TimeStamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LogDto
                {
                    Id = l.Id,
                    Level = l.Level,
                    Message = l.Message,
                    Exception = l.Exception,
                    TimeStamp = l.TimeStamp,
                    Source = l.Source,
                    CompanyId = l.CompanyId,
                    UserId = l.UserId
                })
                .ToListAsync();

            var result = new LogsPagedResponseDto
            {
                Logs = logs,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener logs");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Purga logs antiguos anteriores a la fecha límite especificada
    /// </summary>
    [HttpDelete]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PurgeLogsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PurgeLogs([FromQuery] DateTime dateLimit)
    {
        try
        {
            // Validar que no se puedan eliminar logs de los últimos 7 días
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            if (dateLimit > sevenDaysAgo)
            {
                return BadRequest(new { message = "No se pueden eliminar logs de los últimos 7 días" });
            }

            var logsToDelete = await _context.Logs
                .Where(l => l.TimeStamp < dateLimit)
                .ToListAsync();

            var count = logsToDelete.Count;

            if (count > 0)
            {
                _context.Logs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();
            }

            return Ok(new PurgeLogsResponseDto
            {
                DeletedCount = count,
                DateLimit = dateLimit
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al purgar logs con fecha límite: {DateLimit}", dateLimit);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    private string? ExtractProperty(Dictionary<string, object>? properties, string key)
    {
        if (properties != null && properties.TryGetValue(key, out var value))
        {
            return value?.ToString();
        }
        return null;
    }
}
