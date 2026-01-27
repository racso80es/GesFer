using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.PurchaseDeliveryNote;

/// <summary>
/// Comando para confirmar un albarán de compra
/// </summary>
public class ConfirmPurchaseDeliveryNoteCommand : ICommand
{
    public Guid DeliveryNoteId { get; set; }
}

