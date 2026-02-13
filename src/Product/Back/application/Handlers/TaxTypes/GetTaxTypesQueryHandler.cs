using GesFer.Product.Application.Queries.TaxTypes;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Shared.Back.Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;
using GesFer.Shared.Back.Domain.Common;

namespace GesFer.Product.Application.Handlers.TaxTypes;

public class GetTaxTypesQueryHandler : IQueryHandler<GetTaxTypesQuery, Result<IReadOnlyList<TaxTypeDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IUserContext _userContext;

    public GetTaxTypesQueryHandler(ApplicationDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<TaxTypeDto>>> Handle(GetTaxTypesQuery request, CancellationToken cancellationToken)
    {
        var companyId = _userContext.CompanyId;

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

        return Result.Success<IReadOnlyList<TaxTypeDto>>(taxTypes);
    }
}

public class GetTaxTypeByIdQueryHandler : IQueryHandler<GetTaxTypeByIdQuery, Result<TaxTypeDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IUserContext _userContext;

    public GetTaxTypeByIdQueryHandler(ApplicationDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<Result<TaxTypeDto>> Handle(GetTaxTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var companyId = _userContext.CompanyId;

        var taxType = await _context.TaxTypes
            .AsNoTracking()
            .Where(t => t.Id == request.Id && t.CompanyId == companyId && t.IsActive)
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

        if (taxType == null)
        {
            return Result.Failure<TaxTypeDto>(new Error("TaxType.NotFound", "Tax type not found."));
        }

        return Result.Success(taxType);
    }
}
