using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

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

    // Navegación
    public Company Company { get; set; } = null!;
    public Tariff? BuyTariff { get; set; }
    public ICollection<PurchaseDeliveryNote> PurchaseDeliveryNotes { get; set; } = new List<PurchaseDeliveryNote>();
}

