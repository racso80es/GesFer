using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Admin.Application.Handlers.Company;

public class DeleteCompanyHandler : IRequestHandler<DeleteCompanyCommand>
{
    private readonly AdminDbContext _context;

    public DeleteCompanyHandler(AdminDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletedAt == null, cancellationToken);

        if (company == null)
            throw new InvalidOperationException($"No se encontró la empresa con ID {request.Id}");

        // Soft delete
        company.DeletedAt = DateTime.UtcNow;
        company.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
