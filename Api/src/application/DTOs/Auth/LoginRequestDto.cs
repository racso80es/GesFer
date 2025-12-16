using System.ComponentModel;

namespace GesFer.Application.DTOs.Auth;

/// <summary>
/// DTO para el login (requiere Empresa, Usuario y Contraseña)
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Nombre de la empresa (tenant)
    /// </summary>
    /// <example>Empresa Demo</example>
    [DefaultValue("Empresa Demo")]
    public string Empresa { get; set; } = "Empresa Demo";
    
    /// <summary>
    /// Nombre de usuario
    /// </summary>
    /// <example>admin</example>
    [DefaultValue("admin")]
    public string Usuario { get; set; } = "admin";
    
    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    /// <example>admin123</example>
    [DefaultValue("admin123")]
    public string Contraseña { get; set; } = "admin123";
}
