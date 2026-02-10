using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Application.DTOs.Company;
using GesFer.Admin.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Admin.Application.Handlers.Company;

public class GetCompanyByIdHandler : IRequestHandler<GetCompanyByIdCommand, CompanyDto?>
{
    private readonly AdminDbContext _context;

    public GetCompanyByIdHandler(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto?> Handle(GetCompanyByIdCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .Where(c => c.Id == request.Id && c.DeletedAt == null)
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
            .FirstOrDefaultAsync(cancellationToken);

        return company;
    }
}
