using System.Security.Claims;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para que el usuario gestione SU propia empresa (Proxy a Admin)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MyCompanyController : ControllerBase
{
    private readonly IAdminApiClient _adminClient;
    private readonly ILogger<MyCompanyController> _logger;

    public MyCompanyController(IAdminApiClient adminClient, ILogger<MyCompanyController> logger)
    {
        _adminClient = adminClient;
        _logger = logger;
    }

    private Guid GetCompanyId()
    {
        // Asumiendo que el CompanyId está en los claims (ajustar según implementación real de token)
        var companyIdClaim = User.FindFirst("company_id")?.Value
                             ?? User.FindFirst("CompanyId")?.Value;

        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            throw new UnauthorizedAccessException("No se encontró el ID de empresa en el token del usuario.");
        }
        return companyId;
    }

    /// <summary>
    /// Obtiene los datos de la empresa del usuario actual
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminCompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyCompany()
    {
        try
        {
            var companyId = GetCompanyId();
            var result = await _adminClient.GetCompanyAsync(companyId);

            if (result == null)
                return NotFound(new { message = "No se encontraron datos de la empresa." });

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mi empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza los datos de la empresa del usuario actual
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(AdminCompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMyCompany([FromBody] AdminUpdateCompanyDto dto)
    {
        try
        {
            var companyId = GetCompanyId();

            // Seguridad: El usuario no puede reactivar/desactivar su empresa, eso es de Admin
            // Forzamos el estado actual o ignoramos el campo en el DTO de entrada si la API de Admin lo permite
            // Aquí asumimos que Admin respetará lo que enviamos, así que deberíamos leer el estado actual primero
            // o que el DTO de UpdateCompanyDto tenga IsActive.

            // Para simplificar y proteger, podríamos obtener la empresa actual para preservar IsActive
            var current = await _adminClient.GetCompanyAsync(companyId);
            if (current != null)
            {
                dto.IsActive = current.IsActive; // Forzar estado actual
            }

            var result = await _adminClient.UpdateCompanyAsync(companyId, dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar mi empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
