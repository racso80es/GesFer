namespace GesFer.Product.Back.Infrastructure.DTOs;

public class AdminCompanyDto
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

public class AdminUpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string Address { get; set; } = string.Empty; // Obligatorio
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? PostalCodeId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? LanguageId { get; set; }
    public bool IsActive { get; set; }
}
