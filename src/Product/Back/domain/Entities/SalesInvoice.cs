using GesFer.Shared.Back.Domain.Common;

namespace GesFer.Product.Back.Domain.Entities;

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

    // Navegación (CompanyId FK a Admin)
    public ICollection<SalesDeliveryNote> SalesDeliveryNotes { get; set; } = new List<SalesDeliveryNote>();
}

