using System.ComponentModel.DataAnnotations;

namespace GesFer.Admin.Application.DTOs.Company;

/// <summary>
/// DTO para respuesta de empresa
/// </summary>
public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? PostalCodeId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? LanguageId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear empresa
/// </summary>
public class CreateCompanyDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TaxId { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }
    public Guid? PostalCodeId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? LanguageId { get; set; }
}

/// <summary>
/// DTO para actualizar empresa
/// </summary>
public class UpdateCompanyDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TaxId { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria")]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }
    public Guid? PostalCodeId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? LanguageId { get; set; }
    public bool IsActive { get; set; }
}
