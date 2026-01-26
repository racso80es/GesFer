using GesFer.Application.Commands.SalesDeliveryNote;
using GesFer.Application.Common.Interfaces;
using GesFer.Domain.Entities;
using GesFer.Domain.Services;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.SalesDeliveryNote;

/// <summary>
/// Handler para crear un albarán de venta
/// </summary>
public class CreateSalesDeliveryNoteCommandHandler : ICommandHandler<CreateSalesDeliveryNoteCommand, Domain.Entities.SalesDeliveryNote>
{
    private readonly ApplicationDbContext _context;
    private readonly IStockService _stockService;

    public CreateSalesDeliveryNoteCommandHandler(
        ApplicationDbContext context,
        IStockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    public async Task<Domain.Entities.SalesDeliveryNote> HandleAsync(CreateSalesDeliveryNoteCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que el cliente existe y pertenece a la empresa
        var customer = await _context.Customers
            .Include(c => c.SellTariff)
                .ThenInclude(t => t!.TariffItems)
            .FirstOrDefaultAsync(c => c.Id == command.CustomerId && c.CompanyId == command.CompanyId && c.DeletedAt == null, cancellationToken);

        if (customer == null)
            throw new InvalidOperationException($"El cliente con ID {command.CustomerId} no existe o no pertenece a la empresa");

        // Verificar stock antes de crear el albarán
        foreach (var lineDto in command.Lines)
        {
            var hasStock = await _stockService.HasEnoughStockAsync(lineDto.ArticleId, lineDto.Quantity);
            if (!hasStock)
            {
                var article = await _context.Articles
                    .FirstOrDefaultAsync(a => a.Id == lineDto.ArticleId, cancellationToken);
                throw new InvalidOperationException(
                    $"Stock insuficiente para el artículo {article?.Name}. " +
                    $"Stock disponible: {article?.Stock}, Cantidad solicitada: {lineDto.Quantity}");
            }
        }

        // Crear el albarán
        var deliveryNote = new Domain.Entities.SalesDeliveryNote
        {
            CompanyId = command.CompanyId,
            CustomerId = command.CustomerId,
            Date = command.Date,
            Reference = command.Reference,
            BillingStatus = BillingStatus.Pending
        };

        _context.SalesDeliveryNotes.Add(deliveryNote);

        // Crear las líneas y calcular precios
        foreach (var lineDto in command.Lines)
        {
            var article = await _context.Articles
                .Include(a => a.Family)
                .FirstOrDefaultAsync(a => a.Id == lineDto.ArticleId && a.CompanyId == command.CompanyId && a.DeletedAt == null, cancellationToken);

            if (article == null)
                throw new InvalidOperationException($"El artículo con ID {lineDto.ArticleId} no existe");

            // Determinar el precio: del DTO, de la tarifa del cliente, o del artículo base
            decimal price = lineDto.Price ?? GetPriceFromTariffOrArticle(customer, article);

            // Calcular importes
            var subtotal = lineDto.Quantity * price;
            var ivaAmount = subtotal * (article.Family.IvaPercentage / 100);
            var total = subtotal + ivaAmount;

            var line = new SalesDeliveryNoteLine
            {
                SalesDeliveryNoteId = deliveryNote.Id,
                ArticleId = lineDto.ArticleId,
                Quantity = lineDto.Quantity,
                Price = price,
                Subtotal = subtotal,
                IvaAmount = ivaAmount,
                Total = total
            };

            deliveryNote.Lines.Add(line);

            // DISMINUIR el stock del artículo
            await _stockService.DecreaseStockAsync(article.Id, lineDto.Quantity);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Cargar relaciones para devolver el objeto completo
        await _context.Entry(deliveryNote)
            .Collection(dn => dn.Lines)
            .LoadAsync(cancellationToken);

        await _context.Entry(deliveryNote)
            .Reference(dn => dn.Customer)
            .LoadAsync(cancellationToken);

        return deliveryNote;
    }

    /// <summary>
    /// Obtiene el precio de la tarifa del cliente o del artículo base
    /// </summary>
    private decimal GetPriceFromTariffOrArticle(GesFer.Domain.Entities.Customer customer, Article article)
    {
        // Si el cliente tiene una tarifa de venta, buscar el precio en la tarifa
        if (customer.SellTariffId.HasValue && customer.SellTariff != null)
        {
            var tariffItem = customer.SellTariff.TariffItems
                .FirstOrDefault(ti => ti.ArticleId == article.Id && ti.DeletedAt == null);

            if (tariffItem != null)
                return tariffItem.Price;
        }

        // Si no hay tarifa o no hay precio específico, usar el precio de venta del artículo
        return article.SellPrice;
    }
}

