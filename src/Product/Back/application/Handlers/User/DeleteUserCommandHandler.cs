using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Product.Back.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.User;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly ProductDbContext _context;

    public DeleteUserCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.Id && u.DeletedAt == null, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"No se encontró el usuario con ID {command.Id}");

        // Soft delete
        user.DeletedAt = DateTime.UtcNow;
        user.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

