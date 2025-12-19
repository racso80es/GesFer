using GesFer.Application.Commands.Company;
using GesFer.Application.Common.Interfaces;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Company;

public class DeleteCompanyCommandHandler : ICommandHandler<DeleteCompanyCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteCompanyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(DeleteCompanyCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.DeletedAt == null, cancellationToken);

        if (company == null)
            throw new InvalidOperationException($"No se encontró la empresa con ID {command.Id}");

        // Soft delete
        company.DeletedAt = DateTime.UtcNow;
        company.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

