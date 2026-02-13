using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Common.Interfaces;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.TaxTypes;

public class GetTaxTypeByIdCommandHandler : ICommandHandler<GetTaxTypeByIdCommand, TaxTypeDto?>
{
    private readonly ApplicationDbContext _context;

    public GetTaxTypeByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaxTypeDto?> HandleAsync(GetTaxTypeByIdCommand command, CancellationToken cancellationToken = default)
    {
        var companyId = command.CompanyId;

        var taxType = await _context.TaxTypes
            .AsNoTracking()
            .Where(t => t.Id == command.Id && t.IsActive && (companyId == null || t.CompanyId == companyId))
            .Select(t => new TaxTypeDto
            {
                Id = t.Id,
                CompanyId = t.CompanyId,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                Value = t.Value,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                IsActive = t.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        return taxType;
    }
}
