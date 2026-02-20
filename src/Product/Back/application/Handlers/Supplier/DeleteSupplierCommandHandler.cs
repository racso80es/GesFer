using GesFer.Application.Commands.Supplier;
using GesFer.Application.Common.Interfaces;
using GesFer.Product.Back.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Supplier;

public class DeleteSupplierCommandHandler : ICommandHandler<DeleteSupplierCommand>
{
    private readonly ProductDbContext _context;

    public DeleteSupplierCommandHandler(ProductDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DeleteSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.DeletedAt == null, cancellationToken);

        if (supplier == null)
            throw new InvalidOperationException($"No se encontró el proveedor con ID {command.Id}");

        // Soft delete
        supplier.DeletedAt = DateTime.UtcNow;
        supplier.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

