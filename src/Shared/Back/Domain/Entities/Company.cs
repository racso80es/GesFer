using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.ValueObjects;

namespace GesFer.Shared.Back.Domain.Entities;

/// <summary>
/// Entidad que representa una empresa (Tenant) en el sistema multi-tenant.
/// Esta es la entidad base utilizada por Admin y Shared Kernel.
/// Product extiende esta entidad para añadir colecciones específicas de negocio.
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
    public PostalCode? PostalCode { get; set; }
    public City? City { get; set; }
    public State? State { get; set; }
    public Country? Country { get; set; }
    public Language? Language { get; set; }

    // NOTA: Las colecciones de User, Family, Article, etc. pertenecen al dominio de Product
    // y no deberían estar en Shared si Shared no conoce esas entidades.
    // Sin embargo, para Admin, Company es una entidad raíz.
    // De momento, eliminamos las referencias a entidades de Product para mantener Shared limpio.
}
