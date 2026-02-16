using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Application.DTOs.Company;
using GesFer.Admin.Infrastructure.Data;
using GesFer.Shared.Back.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Admin.Application.Handlers.Company;

public class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly AdminDbContext _context;

    public UpdateCompanyHandler(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletedAt == null, cancellationToken);

        if (company == null)
            throw new InvalidOperationException($"No se encontró la empresa con ID {request.Id}");

        // Validar que no exista otra empresa con el mismo nombre (excepto la actual)
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == request.Dto.Name && c.Id != request.Id && c.DeletedAt == null, cancellationToken);

        if (existingCompany != null)
            throw new InvalidOperationException($"Ya existe otra empresa con el nombre '{request.Dto.Name}'");

        // Validar y convertir TaxId si se proporciona
        TaxId? taxId = null;
        if (!string.IsNullOrWhiteSpace(request.Dto.TaxId))
        {
            taxId = TaxId.Create(request.Dto.TaxId);
        }

        // Validar y convertir Email si se proporciona
        Email? email = null;
        if (!string.IsNullOrWhiteSpace(request.Dto.Email))
        {
            email = Email.Create(request.Dto.Email);
        }

        company.Name = request.Dto.Name;
        company.TaxId = taxId;
        company.Address = request.Dto.Address;
        company.Phone = request.Dto.Phone;
        company.Email = email;
        company.PostalCodeId = request.Dto.PostalCodeId;
        company.CityId = request.Dto.CityId;
        company.StateId = request.Dto.StateId;
        company.CountryId = request.Dto.CountryId;
        company.LanguageId = request.Dto.LanguageId;
        company.IsActive = request.Dto.IsActive;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            TaxId = company.TaxId.HasValue ? company.TaxId.Value.Value : null,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email.HasValue ? company.Email.Value.Value : null,
            PostalCodeId = company.PostalCodeId,
            CityId = company.CityId,
            StateId = company.StateId,
            CountryId = company.CountryId,
            LanguageId = company.LanguageId,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt
        };
    }
}
