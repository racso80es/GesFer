using GesFer.Domain.Entities;
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

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Autentica un usuario verificando empresa, usuario y contraseña
    /// </summary>
    public async Task<User?> AuthenticateAsync(string companyName, string username, string password)
    {
        // Buscar el usuario con la empresa incluida
        // Verificar primero que la empresa exista y esté activa
        var company = await _context.Companies
            .Where(c => c.Name == companyName && c.IsActive && c.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (company == null)
            return null;

        // Buscar el usuario con la empresa cargada
        var user = await _context.Users
            .Include(u => u.Company)
                .ThenInclude(c => c.Language)
            .Include(u => u.Company)
                .ThenInclude(c => c.Country)
                    .ThenInclude(c => c.Language)
            .Include(u => u.Country)
                .ThenInclude(c => c.Language)
            .Include(u => u.Language)
            .Where(u => u.Username == username
                && u.CompanyId == company.Id
                && u.IsActive
                && u.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        // Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
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
                .ThenInclude(g => g.GroupPermissions)
                    .ThenInclude(gp => gp.Permission)
            .Where(ug => ug.UserId == userId && ug.DeletedAt == null)
            .SelectMany(ug => ug.Group.GroupPermissions
                .Where(gp => gp.DeletedAt == null)
                .Select(gp => gp.Permission.Key))
            .ToListAsync();

        foreach (var perm in groupPermissions)
        {
            permissions.Add(perm);
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

