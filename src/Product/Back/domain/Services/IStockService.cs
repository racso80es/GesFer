using GesFer.Product.Back.Domain.Entities;

namespace GesFer.Product.Back.Domain.Services;

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

    /// <summary>
    /// Aplica el aumento de stock en la entidad en memoria sin guardar en base de datos.
    /// Útil para operaciones masivas donde el guardado se realiza externamente.
    /// </summary>
    void ApplyStockIncrease(Article article, decimal quantity);

    /// <summary>
    /// Aplica la disminución de stock en la entidad en memoria sin guardar en base de datos.
    /// Útil para operaciones masivas donde el guardado se realiza externamente.
    /// </summary>
    void ApplyStockDecrease(Article article, decimal quantity);
}
