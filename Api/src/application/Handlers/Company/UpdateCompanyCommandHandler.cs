using GesFer.Application.Commands.Company;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Company;

public class UpdateCompanyCommandHandler : ICommandHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateCompanyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> HandleAsync(UpdateCompanyCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.DeletedAt == null, cancellationToken);

        if (company == null)
            throw new InvalidOperationException($"No se encontró la empresa con ID {command.Id}");

        // Validar que no exista otra empresa con el mismo nombre (excepto la actual)
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == command.Dto.Name && c.Id != command.Id && c.DeletedAt == null, cancellationToken);

        if (existingCompany != null)
            throw new InvalidOperationException($"Ya existe otra empresa con el nombre '{command.Dto.Name}'");

        // Validar IDs de dirección si se proporcionan
        if (command.Dto.PostalCodeId.HasValue)
        {
            var postalCodeExists = await _context.PostalCodes
                .AnyAsync(pc => pc.Id == command.Dto.PostalCodeId.Value && pc.DeletedAt == null, cancellationToken);
            if (!postalCodeExists)
                throw new InvalidOperationException($"No se encontró el código postal con ID {command.Dto.PostalCodeId.Value}");
        }

        if (command.Dto.CityId.HasValue)
        {
            var cityExists = await _context.Cities
                .AnyAsync(c => c.Id == command.Dto.CityId.Value && c.DeletedAt == null, cancellationToken);
            if (!cityExists)
                throw new InvalidOperationException($"No se encontró la ciudad con ID {command.Dto.CityId.Value}");
        }

        if (command.Dto.StateId.HasValue)
        {
            var stateExists = await _context.States
                .AnyAsync(s => s.Id == command.Dto.StateId.Value && s.DeletedAt == null, cancellationToken);
            if (!stateExists)
                throw new InvalidOperationException($"No se encontró la provincia con ID {command.Dto.StateId.Value}");
        }

        if (command.Dto.CountryId.HasValue)
        {
            var countryExists = await _context.Countries
                .AnyAsync(c => c.Id == command.Dto.CountryId.Value && c.DeletedAt == null, cancellationToken);
            if (!countryExists)
                throw new InvalidOperationException($"No se encontró el país con ID {command.Dto.CountryId.Value}");
        }

        if (command.Dto.LanguageId.HasValue)
        {
            var languageExists = await _context.Languages
                .AnyAsync(l => l.Id == command.Dto.LanguageId.Value && l.DeletedAt == null, cancellationToken);
            if (!languageExists)
                throw new InvalidOperationException($"No se encontró el idioma con ID {command.Dto.LanguageId.Value}");
        }

        company.Name = command.Dto.Name;
        company.TaxId = command.Dto.TaxId;
        company.Address = command.Dto.Address;
        company.Phone = command.Dto.Phone;
        company.Email = command.Dto.Email;
        company.PostalCodeId = command.Dto.PostalCodeId;
        company.CityId = command.Dto.CityId;
        company.StateId = command.Dto.StateId;
        company.CountryId = command.Dto.CountryId;
        company.LanguageId = command.Dto.LanguageId;
        company.IsActive = command.Dto.IsActive;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            TaxId = company.TaxId,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email,
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

