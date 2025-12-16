using GesFer.Domain.Entities;

namespace GesFer.Application.Services;

/// <summary>
/// Servicio de aplicación para gestión de albaranes de venta
/// </summary>
public interface ISalesDeliveryNoteService
{
    /// <summary>
    /// Crea un albarán de venta y disminuye el stock automáticamente
    /// </summary>
    Task<SalesDeliveryNote> CreateSalesDeliveryNoteAsync(
        Guid companyId,
        Guid customerId,
        DateTime date,
        string? reference,
        List<SalesDeliveryNoteLineDto> lines);

    /// <summary>
    /// Confirma un albarán de venta (si no estaba confirmado) y actualiza el stock
    /// </summary>
    Task ConfirmSalesDeliveryNoteAsync(Guid deliveryNoteId);
}

/// <summary>
/// DTO para crear líneas de albarán de venta
/// </summary>
public class SalesDeliveryNoteLineDto
{
    public Guid ArticleId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; } // Si es null, se toma de la tarifa del cliente o del artículo base
}

