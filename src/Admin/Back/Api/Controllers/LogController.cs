


using GesFer.Admin.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Admin.Api.Controllers;

/// <summary>
/// Controlador para gestión de logs del sistema
/// </summary>
[ApiController]
[Route("api/admin/logs")]
[Authorize(Policy = "AdminOnly")] // Requiere rol Admin
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
    /// Obtiene logs paginados con filtros opcionales
    /// </summary>
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

            var query = _context.Logs.AsQueryable();

            // Aplicar filtros
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
}

// DTOs temporales
public class LogDto
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime TimeStamp { get; set; }
    public string? Source { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? UserId { get; set; }
}

public class LogsPagedResponseDto
{
    public List<LogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PurgeLogsResponseDto
{
    public int DeletedCount { get; set; }
    public DateTime DateLimit { get; set; }
}
