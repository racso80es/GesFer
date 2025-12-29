using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Servicio para generar y validar tokens JWT
/// </summary>
public interface IJwtService
{
    string GenerateToken(string cursorId, string username, Guid userId, List<string> permissions);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? throw new InvalidOperationException("JwtSettings:SecretKey no está configurado");
        _issuer = _configuration["JwtSettings:Issuer"] 
            ?? throw new InvalidOperationException("JwtSettings:Issuer no está configurado");
        _audience = _configuration["JwtSettings:Audience"] 
            ?? throw new InvalidOperationException("JwtSettings:Audience no está configurado");
        _expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
    }

    /// <summary>
    /// Genera un token JWT con el Cursor ID como ClaimTypes.NameIdentifier
    /// </summary>
    public string GenerateToken(string cursorId, string username, Guid userId, List<string> permissions)
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
            // JWT ID único
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Agregar permisos como claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

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

