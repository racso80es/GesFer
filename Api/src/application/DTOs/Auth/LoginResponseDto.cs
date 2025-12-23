namespace GesFer.Application.DTOs.Auth;

/// <summary>
/// DTO de respuesta del login
/// </summary>
public class LoginResponseDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid? UserLanguageId { get; set; }
    public Guid? CompanyLanguageId { get; set; }
    public Guid? CountryLanguageId { get; set; }
    public Guid? EffectiveLanguageId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public string Token { get; set; } = string.Empty; // Para futura implementación de JWT
}

