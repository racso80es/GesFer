using GesFer.Application.DTOs.Auth;

namespace GesFer.Application.Services;

/// <summary>
/// Servicio de aplicación para autenticación
/// </summary>
public interface IAuthApplicationService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
}

