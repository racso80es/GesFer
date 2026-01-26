using GesFer.Domain.Entities;

namespace GesFer.Domain.Services;

/// <summary>
/// Servicio de dominio para gestión de stock
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Aumenta el stock de un artículo (para albaranes de compra)
    /// </summary>
    Task IncreaseStockAsync(Guid articleId, decimal quantity);

    /// <summary>
    /// Disminuye el stock de un artículo (para albaranes de venta)
    /// </summary>
    Task DecreaseStockAsync(Guid articleId, decimal quantity);

    /// <summary>
    /// Verifica si hay stock suficiente para una venta
    /// </summary>
    Task<bool> HasEnoughStockAsync(Guid articleId, decimal quantity);
}

