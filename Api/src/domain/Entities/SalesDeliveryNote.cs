using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un albarán de venta
/// </summary>
public class SalesDeliveryNote : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public BillingStatus BillingStatus { get; set; } = BillingStatus.Pending;
    public Guid? SalesInvoiceId { get; set; } // Nullable hasta que se facture

    // Navegación
    public Company Company { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public SalesInvoice? SalesInvoice { get; set; }
    public ICollection<SalesDeliveryNoteLine> Lines { get; set; } = new List<SalesDeliveryNoteLine>();
}

