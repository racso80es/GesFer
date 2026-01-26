using GesFer.Domain.Entities;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Servicio de autenticación para usuarios administrativos
/// </summary>
public interface IAdminAuthService
{
    /// <summary>
    /// Autentica un usuario administrativo por username y contraseña
    /// </summary>
    Task<AdminUser?> AuthenticateAsync(string username, string password);
}
