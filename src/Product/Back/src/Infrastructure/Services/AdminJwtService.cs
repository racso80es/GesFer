using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Servicio para generar tokens JWT administrativos con claim role: Admin
/// </summary>
public class AdminJwtService : IAdminJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public AdminJwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? throw new InvalidOperationException("JwtSettings:SecretKey no está configurado");

        // Validar que la clave tenga al menos 32 caracteres (256 bits) para SHA-256 (HS256)
        if (_secretKey.Length < 32)
        {
            throw new InvalidOperationException(
                $"JwtSettings:SecretKey debe tener al menos 32 caracteres (256 bits) para cumplir con el algoritmo SHA-256 (HS256). " +
                $"Longitud actual: {_secretKey.Length} caracteres.");
        }

        _issuer = _configuration["JwtSettings:Issuer"] 
            ?? throw new InvalidOperationException("JwtSettings:Issuer no está configurado");
        _audience = _configuration["JwtSettings:Audience"] 
            ?? throw new InvalidOperationException("JwtSettings:Audience no está configurado");
        _expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
    }

    /// <summary>
    /// Genera un token JWT administrativo con el claim role: Admin
    /// </summary>
    public string GenerateAdminToken(string cursorId, string username, Guid userId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Cursor ID como identificador principal (NameIdentifier)
            new Claim(ClaimTypes.NameIdentifier, cursorId),
            // Username
            new Claim(ClaimTypes.Name, username),
            // UserId como Guid
            new Claim("UserId", userId.ToString()),
            // Rol administrativo (requerido para la política de autorización)
            new Claim(ClaimTypes.Role, "Admin"),
            // JWT ID único
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
