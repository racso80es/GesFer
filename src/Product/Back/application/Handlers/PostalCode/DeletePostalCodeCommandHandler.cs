using GesFer.Application.Commands.PostalCode;
using GesFer.Application.Common.Interfaces;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.PostalCode;

public class DeletePostalCodeCommandHandler : ICommandHandler<DeletePostalCodeCommand>
{
    private readonly ProductDbContext _context;

    public DeletePostalCodeCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DeletePostalCodeCommand command, CancellationToken cancellationToken = default)
    {
        var postalCode = await _context.PostalCodes
            .FirstOrDefaultAsync(pc => pc.Id == command.Id && pc.DeletedAt == null, cancellationToken);

        if (postalCode == null)
            throw new InvalidOperationException($"No se encontró el código postal con ID {command.Id}");

        // Soft delete
        postalCode.DeletedAt = DateTime.UtcNow;
        postalCode.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

