using GesFer.Application.DTOs.Admin;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GesFer.Api.Controllers;

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
    private readonly IAsyncLogPublisher _logPublisher;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context,
        IAsyncLogPublisher logPublisher,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _logPublisher = logPublisher;
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

            // Registrar log de auditoría de forma asíncrona y robusta (Fail-Open)
            try
            {
                await _logPublisher.PublishAuditLog(
                    cursorId: cursorId,
                    username: username,
                    action: "GetDashboardSummary",
                    httpMethod: HttpContext.Request.Method,
                    path: HttpContext.Request.Path,
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
                // Fail-open: loguear el error de auditoría y continuar (Audit Compliance)
                // Esto garantiza que el fallo del sistema de auditoría no bloquee la funcionalidad principal
                _logger.LogWarning(ex, "Fallo en auditoría de dashboard - Operación continúa (Fail-Open)");
            }

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen del dashboard. Error: {Message}", ex.Message);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}
