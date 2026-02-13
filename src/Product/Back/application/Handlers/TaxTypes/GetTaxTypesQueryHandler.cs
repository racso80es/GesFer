using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Product.Application.Queries.TaxTypes;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Shared.Back.Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.Application.Handlers.TaxTypes;

// Fix: IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
public class GetTaxTypesQueryHandler :
    IQueryHandler<GetTaxTypesQuery, IReadOnlyList<TaxTypeDto>>,
    IQueryHandler<GetTaxTypeByIdQuery, TaxTypeDto>
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

        // Reverted to match original intent of filtering by Active status as requested in review
        // Assuming IsDeleted check is implicit in BaseEntity or handled elsewhere if soft delete is enabled globally
        // But for safety and restoring original logic:
        var taxTypes = await _context.TaxTypes
            .AsNoTracking()
            .Where(t => t.CompanyId == companyId && t.IsActive)
            .OrderBy(t => t.Name)
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
            return Result.Failure<TaxTypeDto>(new Error("TaxType.NotFound", "The tax type was not found."));
        }

        return Result.Success(taxType);
    }
}
