using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Línea de un albarán de compra
/// </summary>
public class PurchaseDeliveryNoteLine : BaseEntity
{
    public Guid PurchaseDeliveryNoteId { get; set; }
    public Guid ArticleId { get; set; }
    public decimal Quantity { get; set; } // Cantidad
    public decimal Price { get; set; } // Precio unitario (de tarifa o artículo base)
    public decimal Subtotal { get; set; } // Quantity * Price
    public decimal IvaAmount { get; set; } // Importe de IVA
    public decimal Total { get; set; } // Subtotal + IvaAmount

    // Navegación
    public PurchaseDeliveryNote PurchaseDeliveryNote { get; set; } = null!;
    public Article Article { get; set; } = null!;
}

