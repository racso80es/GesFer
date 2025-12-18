using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.SalesDeliveryNote;

/// <summary>
/// Comando para confirmar un albarán de venta
/// </summary>
public class ConfirmSalesDeliveryNoteCommand : ICommand
{
    public Guid DeliveryNoteId { get; set; }
}

