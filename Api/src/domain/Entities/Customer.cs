using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un cliente
/// </summary>
public class Customer : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? SellTariffId { get; set; } // Tarifa de venta opcional

    // Navegación
    public Company Company { get; set; } = null!;
    public Tariff? SellTariff { get; set; }
    public ICollection<SalesDeliveryNote> SalesDeliveryNotes { get; set; } = new List<SalesDeliveryNote>();
}

