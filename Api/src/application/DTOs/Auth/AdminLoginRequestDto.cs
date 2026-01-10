using System.ComponentModel;

namespace GesFer.Application.DTOs.Auth;

/// <summary>
/// DTO para el login administrativo (solo Usuario y Contraseña)
/// </summary>
public class AdminLoginRequestDto
{
    /// <summary>
    /// Nombre de usuario administrativo
    /// </summary>
    /// <example>admin</example>
    [DefaultValue("admin")]
    public string Usuario { get; set; } = "admin";
    
    /// <summary>
    /// Contraseña del usuario administrativo
    /// </summary>
    /// <example>admin123</example>
    [DefaultValue("admin123")]
    public string Contraseña { get; set; } = "admin123";
}
