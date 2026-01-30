namespace GesFer.Application.DTOs.City;

/// <summary>
/// DTO para respuesta de ciudad
/// </summary>
public class CityDto
{
    public Guid Id { get; set; }
    public Guid StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> PostalCodes { get; set; } = new List<string>(); // Lista de códigos postales
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear ciudad
/// </summary>
public class CreateCityDto
{
    public Guid StateId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualizar ciudad
/// </summary>
public class UpdateCityDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

