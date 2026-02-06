using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Domain.Services;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de gestión de stock
/// </summary>
public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;

    public StockService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Aumenta el stock de un artículo (para albaranes de compra)
    /// </summary>
    public async Task IncreaseStockAsync(Guid articleId, decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(quantity));

        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId && a.DeletedAt == null);

        if (article == null)
            throw new InvalidOperationException($"El artículo con ID {articleId} no existe");

        ApplyStockIncrease(article, quantity);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Disminuye el stock de un artículo (para albaranes de venta)
    /// </summary>
    public async Task DecreaseStockAsync(Guid articleId, decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(quantity));

        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId && a.DeletedAt == null);

        if (article == null)
            throw new InvalidOperationException($"El artículo con ID {articleId} no existe");

        ApplyStockDecrease(article, quantity);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Verifica si hay stock suficiente para una venta
    /// </summary>
    public async Task<bool> HasEnoughStockAsync(Guid articleId, decimal quantity)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId && a.DeletedAt == null);

        if (article == null)
            return false;

        return article.Stock >= quantity;
    }

    /// <inheritdoc />
    public void ApplyStockIncrease(Article article, decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(quantity));

        article.Stock += quantity;
        article.UpdatedAt = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public void ApplyStockDecrease(Article article, decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(quantity));

        if (article.Stock < quantity)
            throw new InvalidOperationException(
                $"Stock insuficiente. Stock disponible: {article.Stock}, Cantidad solicitada: {quantity}");

        article.Stock -= quantity;
        article.UpdatedAt = DateTime.UtcNow;
    }
}
