using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa una factura de venta
/// </summary>
public class SalesInvoice : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Subtotal { get; set; }
    public decimal IvaAmount { get; set; }
    public decimal Total { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    // Navegación
    public Company Company { get; set; } = null!;
    public ICollection<SalesDeliveryNote> SalesDeliveryNotes { get; set; } = new List<SalesDeliveryNote>();
}

