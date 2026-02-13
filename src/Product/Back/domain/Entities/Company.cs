using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.ValueObjects;

namespace GesFer.Product.Back.Domain.Entities;

/// <summary>
/// Entidad que representa una empresa (Tenant) en el sistema multi-tenant.
/// Esta clase extiende la entidad base de Shared para incluir las colecciones específicas de Product.
/// Usar alias explícito si se requiere acceso a la entidad base en el mismo contexto.
/// </summary>
public class Company : GesFer.Shared.Back.Domain.Entities.Company
{
    // Heredamos de Shared.Company, pero añadimos las colecciones específicas de Product
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
