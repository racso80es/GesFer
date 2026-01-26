using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un artículo del catálogo
/// </summary>
public class Article : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid FamilyId { get; set; }
    public string Code { get; set; } = string.Empty; // 10 caracteres, único, obligatorio
    public string Name { get; set; } = string.Empty; // 50 caracteres, obligatorio
    public string? Description { get; set; } // 255 caracteres, opcional
    public decimal BuyPrice { get; set; } // Precio de compra (positivo)
    public decimal SellPrice { get; set; } // Precio de venta (positivo)
    public decimal Stock { get; set; } = 0; // Stock actual del artículo

    // Navegación
    public Company Company { get; set; } = null!;
    public Family Family { get; set; } = null!;
    public ICollection<TariffItem> TariffItems { get; set; } = new List<TariffItem>();
    public ICollection<PurchaseDeliveryNoteLine> PurchaseDeliveryNoteLines { get; set; } = new List<PurchaseDeliveryNoteLine>();
    public ICollection<SalesDeliveryNoteLine> SalesDeliveryNoteLines { get; set; } = new List<SalesDeliveryNoteLine>();
}

