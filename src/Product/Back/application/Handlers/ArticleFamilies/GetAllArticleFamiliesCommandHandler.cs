using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.ArticleFamilies;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.ArticleFamilies;

public class GetAllArticleFamiliesCommandHandler : ICommandHandler<GetAllArticleFamiliesCommand, List<ArticleFamilyDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllArticleFamiliesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArticleFamilyDto>> HandleAsync(GetAllArticleFamiliesCommand command, CancellationToken cancellationToken = default)
    {
        var query = _context.ArticleFamilies
            .Include(af => af.TaxType)
            .Where(af => af.DeletedAt == null);

        if (command.CompanyId.HasValue)
            query = query.Where(af => af.CompanyId == command.CompanyId.Value);

        return await query
            .OrderBy(af => af.Name)
            .Select(af => new ArticleFamilyDto
            {
                Id = af.Id,
                CompanyId = af.CompanyId,
                Code = af.Code,
                Name = af.Name,
                Description = af.Description,
                TaxTypeId = af.TaxTypeId,
                TaxTypeName = af.TaxType != null ? af.TaxType.Name : null,
                TaxTypeValue = af.TaxType != null ? af.TaxType.Value : null,
                IsActive = af.IsActive,
                CreatedAt = af.CreatedAt,
                UpdatedAt = af.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
