
using MyCompany.SysAdmin.Infrastructure.Services;
using GesFer.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GesFer.Admin.Api.Controllers;

/// <summary>
/// Controlador para el dashboard administrativo
/// Todas las acciones requieren autenticación y rol Admin
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context,
        IAuditLogService auditLogService,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un resumen de métricas clave del sistema
    /// Cada petición registra un log de auditoría con el Cursor ID del administrador
    /// </summary>
    /// <returns>Métricas resumidas del sistema</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            // Extraer el Cursor ID del User.Identity (NameIdentifier claim)
            var cursorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            if (string.IsNullOrEmpty(cursorId))
            {
                _logger.LogWarning("Intento de acceso al dashboard sin Cursor ID en el token");
                return Unauthorized(new { message = "Cursor ID no encontrado en el token" });
            }

            // Obtener métricas del sistema
            var summary = new DashboardSummaryDto
            {
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive && u.DeletedAt == null),
                TotalArticles = await _context.Articles.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                GeneratedAt = DateTime.UtcNow
            };

            // Registrar log de auditoría con el Cursor ID (Fire and Forget)
            // No bloqueamos la respuesta del dashboard por el log
            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _auditLogService.LogActionAsync(
                        cursorId: cursorId,
                        username: username,
                        action: "GetDashboardSummary",
                        httpMethod: method,
                        path: path,
                        additionalData: System.Text.Json.JsonSerializer.Serialize(new
                        {
                            TotalCompanies = summary.TotalCompanies,
                            TotalUsers = summary.TotalUsers,
                            ActiveUsers = summary.ActiveUsers
                        })
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar audit log en background");
                }
            });

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen del dashboard. Error: {Message}", ex.Message);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}

// DTO temporal (debería estar en Application/DTOs)
public class DashboardSummaryDto
{
    public int TotalCompanies { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalArticles { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalCustomers { get; set; }
    public DateTime GeneratedAt { get; set; }
}
