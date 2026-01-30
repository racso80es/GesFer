namespace MyCompany.SysAdmin.Infrastructure.Services;

/// <summary>
/// Servicio para generar y validar tokens JWT administrativos
/// </summary>
public interface IAdminJwtService
{
    /// <summary>
    /// Genera un token JWT administrativo con el claim role: Admin
    /// </summary>
    /// <param name="cursorId">Cursor ID del administrador (típicamente el Id convertido a string)</param>
    /// <param name="username">Username del administrador</param>
    /// <param name="userId">ID del administrador (Guid)</param>
    /// <returns>Token JWT como string</returns>
    string GenerateAdminToken(string cursorId, string username, Guid userId);
}
