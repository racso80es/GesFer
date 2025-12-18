using GesFer.Application.Common.Interfaces;
using GesFer.Domain.Entities;

namespace GesFer.Application.Commands.SalesDeliveryNote;

/// <summary>
/// Comando para crear un albarán de venta
/// </summary>
public class CreateSalesDeliveryNoteCommand : ICommand<Domain.Entities.SalesDeliveryNote>
{
    public Guid CompanyId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
    public List<SalesDeliveryNoteLineDto> Lines { get; set; } = new();
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

