using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un albarán de compra
/// </summary>
public enum BillingStatus
{
    Pending = 1,   // Pendiente de facturar
    Invoiced = 2  // Facturado
}

public enum PaymentStatus
{
    Paid = 1,     // Pagado
    Pending = 2   // Pendiente
}

public class PurchaseDeliveryNote : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid SupplierId { get; set; }
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public BillingStatus BillingStatus { get; set; } = BillingStatus.Pending;
    public Guid? PurchaseInvoiceId { get; set; } // Nullable hasta que se facture

    // Navegación
    public Company Company { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public PurchaseInvoice? PurchaseInvoice { get; set; }
    public ICollection<PurchaseDeliveryNoteLine> Lines { get; set; } = new List<PurchaseDeliveryNoteLine>();
}

