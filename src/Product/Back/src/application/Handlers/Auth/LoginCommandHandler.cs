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
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
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

        // Resolver el idioma de forma segura, manejando nulls
        var resolvedLanguageId = user.LanguageId
            ?? user.Company?.LanguageId
            ?? user.Company?.Country?.LanguageId
            ?? user.Country?.LanguageId;

        // Cursor ID es el UserId convertido a string
        var cursorId = user.Id.ToString();

        // Generar token JWT
        var token = _jwtService.GenerateToken(
            cursorId: cursorId,
            username: user.Username,
            userId: user.Id,
            permissions: permissions.ToList()
        );

        return new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name ?? string.Empty,
            UserLanguageId = user.LanguageId,
            CompanyLanguageId = user.Company?.LanguageId,
            CountryLanguageId = user.Company?.Country?.LanguageId ?? user.Country?.LanguageId,
            EffectiveLanguageId = resolvedLanguageId,
            Permissions = permissions.ToList(),
            Token = token,
            CursorId = cursorId
        };
    }
}

