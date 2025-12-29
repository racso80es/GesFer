using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador de ejemplo protegido con [Authorize]
/// Demuestra cómo extraer el Cursor ID del contexto del usuario
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación JWT
public class ProfileController : ControllerBase
{
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(ILogger<ProfileController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado
    /// Extrae el Cursor ID del Claim NameIdentifier
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetMyProfile()
    {
        try
        {
            // Extraer el Cursor ID del Claim NameIdentifier
            var cursorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(cursorId))
            {
                return Unauthorized(new { message = "Cursor ID no encontrado en el token" });
            }

            // Extraer otros claims útiles
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = User.FindFirst("UserId")?.Value;
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();

            var profile = new
            {
                CursorId = cursorId,
                UserId = userId,
                Username = username,
                Permissions = permissions,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil del usuario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Endpoint de ejemplo que requiere autenticación
    /// </summary>
    [HttpGet("protected")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetProtectedData()
    {
        var cursorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new
        {
            message = "Este es un endpoint protegido",
            cursorId = cursorId,
            username = username,
            timestamp = DateTime.UtcNow
        });
    }
}

