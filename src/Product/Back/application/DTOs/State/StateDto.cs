namespace GesFer.Application.DTOs.State;

/// <summary>
/// DTO para respuesta de provincia/estado
/// </summary>
public class StateDto
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear provincia/estado
/// </summary>
public class CreateStateDto
{
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

/// <summary>
/// DTO para actualizar provincia/estado
/// </summary>
public class UpdateStateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
}

