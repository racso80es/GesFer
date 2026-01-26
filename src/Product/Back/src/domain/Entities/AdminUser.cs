using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un usuario administrativo del sistema
/// Usuarios administrativos tienen acceso a funcionalidades administrativas
/// con autenticación separada del sistema multi-tenant principal
/// </summary>
public class AdminUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = "Admin"; // Rol del usuario administrativo
    public DateTime? LastLoginAt { get; set; } // Último inicio de sesión
    public string? LastLoginIp { get; set; } // IP del último inicio de sesión
}
