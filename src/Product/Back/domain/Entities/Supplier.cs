using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.Entities;

namespace GesFer.Product.Back.Domain.Entities;

/// <summary>
/// Entidad que representa un proveedor
/// </summary>
public class Supplier : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? BuyTariffId { get; set; } // Tarifa de compra opcional

    // Campos de dirección
    public Guid? PostalCodeId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }

    // Navegación (CompanyId FK a Admin)
    public Tariff? BuyTariff { get; set; }
    public GesFer.Shared.Back.Domain.Entities.PostalCode? PostalCode { get; set; }
    public GesFer.Shared.Back.Domain.Entities.City? City { get; set; }
    public GesFer.Shared.Back.Domain.Entities.State? State { get; set; }
    public GesFer.Shared.Back.Domain.Entities.Country? Country { get; set; }
    public ICollection<PurchaseDeliveryNote> PurchaseDeliveryNotes { get; set; } = new List<PurchaseDeliveryNote>();
}

