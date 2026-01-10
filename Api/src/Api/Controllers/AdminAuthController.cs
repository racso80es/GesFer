using GesFer.Application.DTOs.Auth;
using GesFer.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para autenticación administrativa
/// </summary>
[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IAdminAuthService _adminAuthService;
    private readonly IAdminJwtService _adminJwtService;
    private readonly ILogger<AdminAuthController> _logger;

    public AdminAuthController(
        IAdminAuthService adminAuthService,
        IAdminJwtService adminJwtService,
        ILogger<AdminAuthController> logger)
    {
        _adminAuthService = adminAuthService;
        _adminJwtService = adminJwtService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza el login del usuario administrativo
    /// </summary>
    /// <param name="request">Datos de login administrativo (Usuario, Contraseña)</param>
    /// <returns>Información del usuario administrativo autenticado y token JWT con role: Admin</returns>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/admin/auth/login
    ///     {
    ///         "usuario": "admin",
    ///         "contraseña": "admin123"
    ///     }
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AdminLoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Contraseña))
            {
                return BadRequest(new { message = "Usuario y contraseña son requeridos" });
            }

            var adminUser = await _adminAuthService.AuthenticateAsync(request.Usuario, request.Contraseña);

            if (adminUser == null)
            {
                return Unauthorized(new { message = "Credenciales administrativas inválidas" });
            }

            // Cursor ID es el UserId convertido a string
            var cursorId = adminUser.Id.ToString();

            // Generar token JWT administrativo con claim role: Admin
            var token = _adminJwtService.GenerateAdminToken(
                cursorId: cursorId,
                username: adminUser.Username,
                userId: adminUser.Id
            );

            var response = new AdminLoginResponseDto
            {
                UserId = adminUser.Id.ToString(), // Convertir Guid a string para compatibilidad con JavaScript
                CursorId = cursorId,
                Username = adminUser.Username,
                FirstName = adminUser.FirstName,
                LastName = adminUser.LastName,
                Email = adminUser.Email,
                Role = "Admin",
                Token = token
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar login administrativo para usuario: {Usuario}. Error: {Message}", 
                request.Usuario, ex.Message);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }
}
