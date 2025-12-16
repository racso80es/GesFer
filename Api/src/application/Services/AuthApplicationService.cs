using GesFer.Application.DTOs.Auth;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Services;

namespace GesFer.Application.Services;

/// <summary>
/// Servicio de aplicación para autenticación
/// </summary>
public class AuthApplicationService : IAuthApplicationService
{
    private readonly IAuthService _authService;

    public AuthApplicationService(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realiza el login del usuario con empresa, usuario y contraseña
    /// </summary>
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Empresa) ||
            string.IsNullOrWhiteSpace(request.Usuario) ||
            string.IsNullOrWhiteSpace(request.Contraseña))
        {
            return null;
        }

        var user = await _authService.AuthenticateAsync(
            request.Empresa,
            request.Usuario,
            request.Contraseña
        );

        if (user == null)
            return null;

        // Verificar que la empresa esté cargada
        if (user.Company == null)
        {
            throw new InvalidOperationException($"La empresa del usuario {user.Username} no está disponible.");
        }

        // Obtener todos los permisos del usuario (directos + de grupos)
        var permissions = await _authService.GetUserPermissionsAsync(user.Id);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CompanyId = user.CompanyId,
            CompanyName = user.Company.Name,
            Permissions = permissions.ToList(),
            Token = string.Empty // Para futura implementación de JWT
        };
    }

    /// <summary>
    /// Obtiene todos los permisos de un usuario
    /// </summary>
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = await _authService.GetUserPermissionsAsync(userId);
        return permissions.ToList();
    }
}

