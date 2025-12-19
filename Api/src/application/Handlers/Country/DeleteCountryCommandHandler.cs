using GesFer.Application.Commands.Country;
using GesFer.Application.Common.Interfaces;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Country;

public class DeleteCountryCommandHandler : ICommandHandler<DeleteCountryCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteCountryCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DeleteCountryCommand command, CancellationToken cancellationToken = default)
    {
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.DeletedAt == null, cancellationToken);

        if (country == null)
            throw new InvalidOperationException($"No se encontró el país con ID {command.Id}");

        // Verificar que no tenga provincias asociadas
        var hasStates = await _context.States
            .AnyAsync(s => s.CountryId == command.Id && s.DeletedAt == null, cancellationToken);

        if (hasStates)
            throw new InvalidOperationException($"No se puede eliminar el país porque tiene provincias asociadas");

        // Soft delete
        country.DeletedAt = DateTime.UtcNow;
        country.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

