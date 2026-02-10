
using GesFer.Admin.Infrastructure.Services;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GesFer.Admin.Infrastructure.DTOs;

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
    private readonly AdminDbContext _context;
    private readonly IProductApiClient _productClient;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        AdminDbContext context,
        IProductApiClient productClient,
        IAuditLogService auditLogService,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _productClient = productClient;
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

            // 1. Obtener métricas propias de Admin (Companies)
            var totalCompanies = await _context.Companies.CountAsync();

            // 2. Obtener métricas remotas de Product (Users, Articles, etc.)
            var productStats = await _productClient.GetDashboardStatsAsync();

            // 3. Combinar
            var summary = new DashboardSummaryDto
            {
                TotalCompanies = totalCompanies,
                TotalUsers = productStats.TotalUsers,
                ActiveUsers = productStats.ActiveUsers,
                TotalArticles = productStats.TotalArticles,
                TotalSuppliers = productStats.TotalSuppliers,
                TotalCustomers = productStats.TotalCustomers,
                GeneratedAt = DateTime.UtcNow
            };

            // Registrar log de auditoría con el Cursor ID
            var method = HttpContext.Request.Method;
            var path = HttpContext.Request.Path;

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
                // Loguear error pero no detener la respuesta al usuario si la auditoría falla (fail-open)
                _logger.LogError(ex, "Error al registrar audit log");
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

