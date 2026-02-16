using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Application.DTOs.Company;
using GesFer.Admin.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Admin.Application.Handlers.Company;

public class GetAllCompaniesHandler : IRequestHandler<GetAllCompaniesCommand, List<CompanyDto>>
{
    private readonly AdminDbContext _context;

    public GetAllCompaniesHandler(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyDto>> Handle(GetAllCompaniesCommand request, CancellationToken cancellationToken)
    {
        var companies = await _context.Companies
            .Where(c => c.DeletedAt == null)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                TaxId = c.TaxId.HasValue ? c.TaxId.Value.Value : null,
                Address = c.Address,
                Phone = c.Phone,
                Email = c.Email.HasValue ? c.Email.Value.Value : null,
                PostalCodeId = c.PostalCodeId,
                CityId = c.CityId,
                StateId = c.StateId,
                CountryId = c.CountryId,
                LanguageId = c.LanguageId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return companies;
    }
}
