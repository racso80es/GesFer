using GesFer.Domain.Entities;
using GesFer.Domain.Services;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Services;

/// <summary>
/// Servicio de aplicación para gestión de albaranes de compra
/// </summary>
public class PurchaseDeliveryNoteService : IPurchaseDeliveryNoteService
{
    private readonly ApplicationDbContext _context;
    private readonly IStockService _stockService;

    public PurchaseDeliveryNoteService(
        ApplicationDbContext context,
        IStockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    /// <summary>
    /// Crea un albarán de compra y actualiza el stock automáticamente
    /// </summary>
    public async Task<PurchaseDeliveryNote> CreatePurchaseDeliveryNoteAsync(
        Guid companyId,
        Guid supplierId,
        DateTime date,
        string? reference,
        List<PurchaseDeliveryNoteLineDto> lines)
    {
        // Validar que el proveedor existe y pertenece a la empresa
        var supplier = await _context.Suppliers
            .Include(s => s.BuyTariff)
                .ThenInclude(t => t!.TariffItems)
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.CompanyId == companyId && !s.IsDeleted);

        if (supplier == null)
            throw new InvalidOperationException($"El proveedor con ID {supplierId} no existe o no pertenece a la empresa");

        // Crear el albarán
        var deliveryNote = new PurchaseDeliveryNote
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            Date = date,
            Reference = reference,
            BillingStatus = BillingStatus.Pending
        };

        _context.PurchaseDeliveryNotes.Add(deliveryNote);

        // Crear las líneas y calcular precios
        foreach (var lineDto in lines)
        {
            var article = await _context.Articles
                .Include(a => a.Family)
                .FirstOrDefaultAsync(a => a.Id == lineDto.ArticleId && a.CompanyId == companyId && !a.IsDeleted);

            if (article == null)
                throw new InvalidOperationException($"El artículo con ID {lineDto.ArticleId} no existe");

            // Determinar el precio: del DTO, de la tarifa del proveedor, o del artículo base
            decimal price = lineDto.Price ?? GetPriceFromTariffOrArticle(supplier, article);

            // Calcular importes
            var subtotal = lineDto.Quantity * price;
            var ivaAmount = subtotal * (article.Family.IvaPercentage / 100);
            var total = subtotal + ivaAmount;

            var line = new PurchaseDeliveryNoteLine
            {
                PurchaseDeliveryNoteId = deliveryNote.Id,
                ArticleId = lineDto.ArticleId,
                Quantity = lineDto.Quantity,
                Price = price,
                Subtotal = subtotal,
                IvaAmount = ivaAmount,
                Total = total
            };

            deliveryNote.Lines.Add(line);

            // AUMENTAR el stock del artículo
            await _stockService.IncreaseStockAsync(article.Id, lineDto.Quantity);
        }

        await _context.SaveChangesAsync();

        // Cargar relaciones para devolver el objeto completo
        await _context.Entry(deliveryNote)
            .Collection(dn => dn.Lines)
            .LoadAsync();

        await _context.Entry(deliveryNote)
            .Reference(dn => dn.Supplier)
            .LoadAsync();

        return deliveryNote;
    }

    /// <summary>
    /// Confirma un albarán de compra (si no estaba confirmado) y actualiza el stock
    /// Nota: En este caso, el stock ya se actualizó al crear el albarán,
    /// pero este método puede usarse para validaciones adicionales
    /// </summary>
    public async Task ConfirmPurchaseDeliveryNoteAsync(Guid deliveryNoteId)
    {
        var deliveryNote = await _context.PurchaseDeliveryNotes
            .Include(dn => dn.Lines)
            .FirstOrDefaultAsync(dn => dn.Id == deliveryNoteId && !dn.IsDeleted);

        if (deliveryNote == null)
            throw new InvalidOperationException($"El albarán con ID {deliveryNoteId} no existe");

        // Si el albarán ya está confirmado, no hacer nada
        // (En este caso, el stock ya se actualizó al crear el albarán)

        // Aquí podrías agregar lógica adicional de confirmación si es necesario
        deliveryNote.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Obtiene el precio de la tarifa del proveedor o del artículo base
    /// </summary>
    private decimal GetPriceFromTariffOrArticle(Supplier supplier, Article article)
    {
        // Si el proveedor tiene una tarifa de compra, buscar el precio en la tarifa
        if (supplier.BuyTariffId.HasValue && supplier.BuyTariff != null)
        {
            var tariffItem = supplier.BuyTariff.TariffItems
                .FirstOrDefault(ti => ti.ArticleId == article.Id && !ti.IsDeleted);

            if (tariffItem != null)
                return tariffItem.Price;
        }

        // Si no hay tarifa o no hay precio específico, usar el precio de compra del artículo
        return article.BuyPrice;
    }
}

