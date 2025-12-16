using GesFer.Domain.Entities;
using GesFer.Domain.Services;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Services;

/// <summary>
/// Servicio de aplicación para gestión de albaranes de venta
/// </summary>
public class SalesDeliveryNoteService : ISalesDeliveryNoteService
{
    private readonly ApplicationDbContext _context;
    private readonly IStockService _stockService;

    public SalesDeliveryNoteService(
        ApplicationDbContext context,
        IStockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    /// <summary>
    /// Crea un albarán de venta y disminuye el stock automáticamente
    /// </summary>
    public async Task<SalesDeliveryNote> CreateSalesDeliveryNoteAsync(
        Guid companyId,
        Guid customerId,
        DateTime date,
        string? reference,
        List<SalesDeliveryNoteLineDto> lines)
    {
        // Validar que el cliente existe y pertenece a la empresa
        var customer = await _context.Customers
            .Include(c => c.SellTariff)
                .ThenInclude(t => t!.TariffItems)
            .FirstOrDefaultAsync(c => c.Id == customerId && c.CompanyId == companyId && !c.IsDeleted);

        if (customer == null)
            throw new InvalidOperationException($"El cliente con ID {customerId} no existe o no pertenece a la empresa");

        // Verificar stock antes de crear el albarán
        foreach (var lineDto in lines)
        {
            var hasStock = await _stockService.HasEnoughStockAsync(lineDto.ArticleId, lineDto.Quantity);
            if (!hasStock)
            {
                var article = await _context.Articles
                    .FirstOrDefaultAsync(a => a.Id == lineDto.ArticleId);
                throw new InvalidOperationException(
                    $"Stock insuficiente para el artículo {article?.Name}. " +
                    $"Stock disponible: {article?.Stock}, Cantidad solicitada: {lineDto.Quantity}");
            }
        }

        // Crear el albarán
        var deliveryNote = new SalesDeliveryNote
        {
            CompanyId = companyId,
            CustomerId = customerId,
            Date = date,
            Reference = reference,
            BillingStatus = BillingStatus.Pending
        };

        _context.SalesDeliveryNotes.Add(deliveryNote);

        // Crear las líneas y calcular precios
        foreach (var lineDto in lines)
        {
            var article = await _context.Articles
                .Include(a => a.Family)
                .FirstOrDefaultAsync(a => a.Id == lineDto.ArticleId && a.CompanyId == companyId && !a.IsDeleted);

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

        await _context.SaveChangesAsync();

        // Cargar relaciones para devolver el objeto completo
        await _context.Entry(deliveryNote)
            .Collection(dn => dn.Lines)
            .LoadAsync();

        await _context.Entry(deliveryNote)
            .Reference(dn => dn.Customer)
            .LoadAsync();

        return deliveryNote;
    }

    /// <summary>
    /// Confirma un albarán de venta (si no estaba confirmado) y actualiza el stock
    /// Nota: En este caso, el stock ya se actualizó al crear el albarán,
    /// pero este método puede usarse para validaciones adicionales
    /// </summary>
    public async Task ConfirmSalesDeliveryNoteAsync(Guid deliveryNoteId)
    {
        var deliveryNote = await _context.SalesDeliveryNotes
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
    /// Obtiene el precio de la tarifa del cliente o del artículo base
    /// </summary>
    private decimal GetPriceFromTariffOrArticle(Customer customer, Article article)
    {
        // Si el cliente tiene una tarifa de venta, buscar el precio en la tarifa
        if (customer.SellTariffId.HasValue && customer.SellTariff != null)
        {
            var tariffItem = customer.SellTariff.TariffItems
                .FirstOrDefault(ti => ti.ArticleId == article.Id && !ti.IsDeleted);

            if (tariffItem != null)
                return tariffItem.Price;
        }

        // Si no hay tarifa o no hay precio específico, usar el precio de venta del artículo
        return article.SellPrice;
    }
}

