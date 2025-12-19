namespace GesFer.Application.DTOs.PostalCode;

/// <summary>
/// DTO para respuesta de código postal
/// </summary>
public class PostalCodeDto
{
    public Guid Id { get; set; }
    public Guid CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public Guid StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear código postal
/// </summary>
public class CreatePostalCodeDto
{
    public Guid CityId { get; set; }
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualizar código postal
/// </summary>
public class UpdatePostalCodeDto
{
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

