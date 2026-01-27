using GesFer.Application.Commands.PurchaseDeliveryNote;
using GesFer.Application.Common.Interfaces;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.PurchaseDeliveryNote;

/// <summary>
/// Handler para confirmar un albarán de compra
/// </summary>
public class ConfirmPurchaseDeliveryNoteCommandHandler : ICommandHandler<ConfirmPurchaseDeliveryNoteCommand>
{
    private readonly ApplicationDbContext _context;

    public ConfirmPurchaseDeliveryNoteCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(ConfirmPurchaseDeliveryNoteCommand command, CancellationToken cancellationToken = default)
    {
        var deliveryNote = await _context.PurchaseDeliveryNotes
            .Include(dn => dn.Lines)
            .FirstOrDefaultAsync(dn => dn.Id == command.DeliveryNoteId && dn.DeletedAt == null, cancellationToken);

        if (deliveryNote == null)
            throw new InvalidOperationException($"El albarán con ID {command.DeliveryNoteId} no existe");

        // Si el albarán ya está confirmado, no hacer nada
        // (En este caso, el stock ya se actualizó al crear el albarán)

        // Aquí podrías agregar lógica adicional de confirmación si es necesario
        deliveryNote.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

