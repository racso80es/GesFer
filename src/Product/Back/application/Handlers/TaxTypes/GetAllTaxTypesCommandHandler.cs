using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Common.Interfaces;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.TaxTypes;

public class GetAllTaxTypesCommandHandler : ICommandHandler<GetAllTaxTypesCommand, List<TaxTypeDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllTaxTypesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaxTypeDto>> HandleAsync(GetAllTaxTypesCommand command, CancellationToken cancellationToken = default)
    {
        var companyId = command.CompanyId ?? Guid.Empty;
        if (companyId == Guid.Empty)
            throw new InvalidOperationException("CompanyId es obligatorio.");

        var taxTypes = await _context.TaxTypes
            .AsNoTracking()
            .Where(t => t.CompanyId == companyId && t.IsActive)
            .OrderBy(t => t.Code)
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
            .ToListAsync(cancellationToken);

        return taxTypes;
    }
}
