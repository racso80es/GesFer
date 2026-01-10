using System.Text.Json.Serialization;

namespace GesFer.Application.DTOs.Auth;

/// <summary>
/// DTO de respuesta del login administrativo
/// </summary>
public class AdminLoginResponseDto
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty; // Serializado como string para compatibilidad con JavaScript
    
    [JsonPropertyName("cursorId")]
    public string CursorId { get; set; } = string.Empty;
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("role")]
    public string Role { get; set; } = "Admin";
    
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty; // JWT Token con claim role: Admin
}
