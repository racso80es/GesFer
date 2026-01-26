using GesFer.Application.Commands.City;
using GesFer.Application.Common.Interfaces;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.City;

public class DeleteCityCommandHandler : ICommandHandler<DeleteCityCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteCityCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DeleteCityCommand command, CancellationToken cancellationToken = default)
    {
        var city = await _context.Cities
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.DeletedAt == null, cancellationToken);

        if (city == null)
            throw new InvalidOperationException($"No se encontró la ciudad con ID {command.Id}");

        // Soft delete
        city.DeletedAt = DateTime.UtcNow;
        city.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

