using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Common.Interfaces;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.TaxTypes;

public class DeleteTaxTypeCommandHandler : ICommandHandler<DeleteTaxTypeCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteTaxTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DeleteTaxTypeCommand command, CancellationToken cancellationToken = default)
    {
        var companyId = command.CompanyId ?? Guid.Empty;
        if (companyId == Guid.Empty)
            throw new InvalidOperationException("CompanyId es obligatorio.");

        var taxType = await _context.TaxTypes
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.CompanyId == companyId, cancellationToken);

        if (taxType == null)
            throw new InvalidOperationException("Tipo de impuesto no encontrado.");

        taxType.DeletedAt = DateTime.UtcNow;
        taxType.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
