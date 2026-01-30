using GesFer.Application.Commands.Company;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Company;

public class GetAllCompaniesCommandHandler : ICommandHandler<GetAllCompaniesCommand, List<CompanyDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllCompaniesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyDto>> HandleAsync(GetAllCompaniesCommand command, CancellationToken cancellationToken = default)
    {
        var companies = await _context.Companies
            .Where(c => c.DeletedAt == null)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                TaxId = c.TaxId,
                Address = c.Address,
                Phone = c.Phone,
                Email = c.Email,
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

