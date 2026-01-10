using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Servicio de autenticación para usuarios administrativos
/// </summary>
public class AdminAuthService : IAdminAuthService
{
    private readonly ApplicationDbContext _context;

    public AdminAuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Autentica un usuario administrativo por username y contraseña
    /// </summary>
    public async Task<AdminUser?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        // Normalizar username
        var normalizedUsername = username.Trim();

        // Buscar el usuario administrativo
        var adminUser = await _context.AdminUsers
            .Where(u => u.Username == normalizedUsername
                && u.IsActive
                && u.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (adminUser == null)
            return null;

        // Verificar contraseña usando BCrypt
        if (!BCrypt.Net.BCrypt.Verify(password, adminUser.PasswordHash))
            return null;

        return adminUser;
    }
}
