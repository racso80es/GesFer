using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa una empresa (Tenant) en el sistema multi-tenant
/// </summary>
public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Navegación
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Family> Families { get; set; } = new List<Family>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}

