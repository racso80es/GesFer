using GesFer.Application.Commands.Auth;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Auth;
using GesFer.Infrastructure.Services;

namespace GesFer.Application.Handlers.Auth;

/// <summary>
/// Handler para el comando de login
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto?>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponseDto?> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Empresa) ||
            string.IsNullOrWhiteSpace(command.Usuario) ||
            string.IsNullOrWhiteSpace(command.Contraseña))
        {
            return null;
        }

        var user = await _authService.AuthenticateAsync(
            command.Empresa,
            command.Usuario,
            command.Contraseña
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

        var resolvedLanguageId = user.LanguageId
            ?? user.Company!.LanguageId
            ?? user.Company.Country?.LanguageId
            ?? user.Country?.LanguageId;

        return new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CompanyId = user.CompanyId,
            CompanyName = user.Company.Name,
            UserLanguageId = user.LanguageId,
            CompanyLanguageId = user.Company.LanguageId,
            CountryLanguageId = user.Company.Country?.LanguageId ?? user.Country?.LanguageId,
            EffectiveLanguageId = resolvedLanguageId,
            Permissions = permissions.ToList(),
            Token = string.Empty // Para futura implementación de JWT
        };
    }
}

