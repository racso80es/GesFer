using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa una empresa (Tenant) en el sistema multi-tenant
/// </summary>
public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string Address { get; set; } = string.Empty; // Obligatorio
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Campos de dirección
    public Guid? PostalCodeId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }

    // Navegación
    public PostalCode? PostalCode { get; set; }
    public City? City { get; set; }
    public State? State { get; set; }
    public Country? Country { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Family> Families { get; set; } = new List<Family>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}

