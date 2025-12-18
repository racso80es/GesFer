using GesFer.Application.Commands.PurchaseDeliveryNote;
using GesFer.Application.Common.Interfaces;
using GesFer.Domain.Entities;
using GesFer.Domain.Services;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.PurchaseDeliveryNote;

/// <summary>
/// Handler para crear un albarán de compra
/// </summary>
public class CreatePurchaseDeliveryNoteCommandHandler : ICommandHandler<CreatePurchaseDeliveryNoteCommand, Domain.Entities.PurchaseDeliveryNote>
{
    private readonly ApplicationDbContext _context;
    private readonly IStockService _stockService;

    public CreatePurchaseDeliveryNoteCommandHandler(
        ApplicationDbContext context,
        IStockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    public async Task<Domain.Entities.PurchaseDeliveryNote> HandleAsync(CreatePurchaseDeliveryNoteCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que el proveedor existe y pertenece a la empresa
        var supplier = await _context.Suppliers
            .Include(s => s.BuyTariff)
                .ThenInclude(t => t!.TariffItems)
            .FirstOrDefaultAsync(s => s.Id == command.SupplierId && s.CompanyId == command.CompanyId && s.DeletedAt == null, cancellationToken);

        if (supplier == null)
            throw new InvalidOperationException($"El proveedor con ID {command.SupplierId} no existe o no pertenece a la empresa");

        // Crear el albarán
        var deliveryNote = new Domain.Entities.PurchaseDeliveryNote
        {
            CompanyId = command.CompanyId,
            SupplierId = command.SupplierId,
            Date = command.Date,
            Reference = command.Reference,
            BillingStatus = BillingStatus.Pending
        };

        _context.PurchaseDeliveryNotes.Add(deliveryNote);

        // Crear las líneas y calcular precios
        foreach (var lineDto in command.Lines)
        {
            var article = await _context.Articles
                .Include(a => a.Family)
                .FirstOrDefaultAsync(a => a.Id == lineDto.ArticleId && a.CompanyId == command.CompanyId && a.DeletedAt == null, cancellationToken);

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

        await _context.SaveChangesAsync(cancellationToken);

        // Cargar relaciones para devolver el objeto completo
        await _context.Entry(deliveryNote)
            .Collection(dn => dn.Lines)
            .LoadAsync(cancellationToken);

        await _context.Entry(deliveryNote)
            .Reference(dn => dn.Supplier)
            .LoadAsync(cancellationToken);

        return deliveryNote;
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
                .FirstOrDefault(ti => ti.ArticleId == article.Id && ti.DeletedAt == null);

            if (tariffItem != null)
                return tariffItem.Price;
        }

        // Si no hay tarifa o no hay precio específico, usar el precio de compra del artículo
        return article.BuyPrice;
    }
}

