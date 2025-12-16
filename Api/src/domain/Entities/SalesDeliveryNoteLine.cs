using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Línea de un albarán de venta
/// </summary>
public class SalesDeliveryNoteLine : BaseEntity
{
    public Guid SalesDeliveryNoteId { get; set; }
    public Guid ArticleId { get; set; }
    public decimal Quantity { get; set; } // Cantidad
    public decimal Price { get; set; } // Precio unitario
    public decimal Subtotal { get; set; } // Quantity * Price
    public decimal IvaAmount { get; set; } // Importe de IVA
    public decimal Total { get; set; } // Subtotal + IvaAmount

    // Navegación
    public SalesDeliveryNote SalesDeliveryNote { get; set; } = null!;
    public Article Article { get; set; } = null!;
}

