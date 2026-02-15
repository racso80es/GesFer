using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Infrastructure.Services;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Servicio de autenticación y autorización
/// </summary>
public interface IAuthService
{
    Task<User?> AuthenticateAsync(string companyName, string username, string password);
    Task<HashSet<string>> GetUserPermissionsAsync(Guid userId);
    Task<bool> HasPermissionAsync(Guid userId, string permissionKey);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IAdminApiClient _adminApiClient;

    public AuthService(ApplicationDbContext context, IAdminApiClient adminApiClient)
    {
        _context = context;
        _adminApiClient = adminApiClient;
    }

    /// <summary>
    /// Autentica un usuario verificando empresa (vía Admin API), usuario y contraseña
    /// </summary>
    public async Task<User?> AuthenticateAsync(string companyName, string username, string password)
    {
        var normalizedCompanyName = companyName?.Trim() ?? string.Empty;
        var normalizedUsername = username?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedCompanyName) || string.IsNullOrWhiteSpace(normalizedUsername))
            return null;

        // Empresa: única fuente Admin API; Product solo conoce la que le corresponde (por nombre en login)
        var company = await _adminApiClient.GetCompanyByNameAsync(normalizedCompanyName);
        if (company == null || !company.IsActive)
            return null;

        var user = await _context.Users
            .Include(u => u.Country)
                .ThenInclude(c => c!.Language)
            .Include(u => u.Language)
            .Where(u => u.Username == normalizedUsername
                && u.CompanyId == company.Id
                && u.IsActive
                && u.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        // Verificar contraseña
        if (string.IsNullOrWhiteSpace(password) || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }

    /// <summary>
    /// Obtiene todos los permisos de un usuario (directos + de grupos)
    /// </summary>
    public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = new HashSet<string>();

        // Permisos directos del usuario
        var directPermissions = await _context.UserPermissions
            .Include(up => up.Permission)
            .Where(up => up.UserId == userId && up.DeletedAt == null)
            .Select(up => up.Permission.Key)
            .ToListAsync();

        foreach (var perm in directPermissions)
        {
            permissions.Add(perm);
        }

        // Permisos de los grupos a los que pertenece el usuario
        var groupPermissions = await _context.UserGroups
            .Include(ug => ug.Group)
                .ThenInclude(g => g!.GroupPermissions)
                    .ThenInclude(gp => gp!.Permission)
            .Where(ug => ug.UserId == userId && ug.DeletedAt == null && ug.Group != null)
            .SelectMany(ug => ug.Group!.GroupPermissions
                .Where(gp => gp != null && gp.DeletedAt == null && gp.Permission != null)
                .Select(gp => gp!.Permission!.Key))
            .ToListAsync();

        foreach (var perm in groupPermissions)
        {
            if (!string.IsNullOrEmpty(perm))
            {
                permissions.Add(perm);
            }
        }

        return permissions;
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico
    /// </summary>
    public async Task<bool> HasPermissionAsync(Guid userId, string permissionKey)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionKey);
    }
}

