using GesFer.Application.Commands.Company;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Company;

public class GetCompanyByIdCommandHandler : ICommandHandler<GetCompanyByIdCommand, CompanyDto?>
{
    private readonly ApplicationDbContext _context;

    public GetCompanyByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto?> HandleAsync(GetCompanyByIdCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .Where(c => c.Id == command.Id && c.DeletedAt == null)
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
            .FirstOrDefaultAsync(cancellationToken);

        return company;
    }
}

