using GesFer.Domain.Entities;

namespace GesFer.Application.Services;

/// <summary>
/// Servicio de aplicación para gestión de albaranes de compra
/// </summary>
public interface IPurchaseDeliveryNoteService
{
    /// <summary>
    /// Crea un albarán de compra y actualiza el stock automáticamente
    /// </summary>
    Task<PurchaseDeliveryNote> CreatePurchaseDeliveryNoteAsync(
        Guid companyId,
        Guid supplierId,
        DateTime date,
        string? reference,
        List<PurchaseDeliveryNoteLineDto> lines);

    /// <summary>
    /// Confirma un albarán de compra (si no estaba confirmado) y actualiza el stock
    /// </summary>
    Task ConfirmPurchaseDeliveryNoteAsync(Guid deliveryNoteId);
}

/// <summary>
/// DTO para crear líneas de albarán de compra
/// </summary>
public class PurchaseDeliveryNoteLineDto
{
    public Guid ArticleId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; } // Si es null, se toma de la tarifa del proveedor o del artículo base
}

