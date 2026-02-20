using GesFer.Application.Commands.State;
using GesFer.Application.Common.Interfaces;
using GesFer.Product.Back.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.State;

public class DeleteStateCommandHandler : ICommandHandler<DeleteStateCommand>
{
    private readonly ProductDbContext _context;

    public DeleteStateCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DeleteStateCommand command, CancellationToken cancellationToken = default)
    {
        var state = await _context.States
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.DeletedAt == null, cancellationToken);

        if (state == null)
            throw new InvalidOperationException($"No se encontró la provincia con ID {command.Id}");

        // Verificar que no tenga ciudades asociadas
        var hasCities = await _context.Cities
            .AnyAsync(c => c.StateId == command.Id && c.DeletedAt == null, cancellationToken);

        if (hasCities)
            throw new InvalidOperationException($"No se puede eliminar la provincia porque tiene ciudades asociadas");

        // Soft delete
        state.DeletedAt = DateTime.UtcNow;
        state.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

