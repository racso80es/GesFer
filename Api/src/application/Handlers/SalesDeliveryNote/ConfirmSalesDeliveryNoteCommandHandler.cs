using GesFer.Application.Commands.SalesDeliveryNote;
using GesFer.Application.Common.Interfaces;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.SalesDeliveryNote;

/// <summary>
/// Handler para confirmar un albarán de venta
/// </summary>
public class ConfirmSalesDeliveryNoteCommandHandler : ICommandHandler<ConfirmSalesDeliveryNoteCommand>
{
    private readonly ApplicationDbContext _context;

    public ConfirmSalesDeliveryNoteCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(ConfirmSalesDeliveryNoteCommand command, CancellationToken cancellationToken = default)
    {
        var deliveryNote = await _context.SalesDeliveryNotes
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

