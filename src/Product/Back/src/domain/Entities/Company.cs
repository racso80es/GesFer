using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.ValueObjects;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa una empresa (Tenant) en el sistema multi-tenant
/// </summary>
public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public TaxId? TaxId { get; set; }
    public string Address { get; set; } = string.Empty; // Obligatorio
    public string? Phone { get; set; }
    public Email? Email { get; set; }

    // Campos de dirección
    public Guid? PostalCodeId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? LanguageId { get; set; }

    // Navegación
    public GesFer.Shared.Back.Domain.Entities.PostalCode? PostalCode { get; set; }
    public GesFer.Shared.Back.Domain.Entities.City? City { get; set; }
    public GesFer.Shared.Back.Domain.Entities.State? State { get; set; }
    public GesFer.Shared.Back.Domain.Entities.Country? Country { get; set; }
    public GesFer.Shared.Back.Domain.Entities.Language? Language { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Family> Families { get; set; } = new List<Family>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}

