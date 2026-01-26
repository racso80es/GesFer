using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa una tarifa (de compra o venta)
/// </summary>
public enum TariffType
{
    Buy = 1,  // Compra
    Sell = 2  // Venta
}

public class Tariff : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TariffType Type { get; set; } // Compra o Venta

    // Navegación
    public Company Company { get; set; } = null!;
    public ICollection<TariffItem> TariffItems { get; set; } = new List<TariffItem>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}

