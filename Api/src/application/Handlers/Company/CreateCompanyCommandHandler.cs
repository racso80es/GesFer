using GesFer.Application.Commands.Company;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Company;

public class CreateCompanyCommandHandler : ICommandHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly ApplicationDbContext _context;

    public CreateCompanyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> HandleAsync(CreateCompanyCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que no exista una empresa con el mismo nombre
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == command.Dto.Name && c.DeletedAt == null, cancellationToken);

        if (existingCompany != null)
            throw new InvalidOperationException($"Ya existe una empresa con el nombre '{command.Dto.Name}'");

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

        var company = new GesFer.Domain.Entities.Company
        {
            Name = command.Dto.Name,
            TaxId = command.Dto.TaxId,
            Address = command.Dto.Address,
            Phone = command.Dto.Phone,
            Email = command.Dto.Email,
            PostalCodeId = command.Dto.PostalCodeId,
            CityId = command.Dto.CityId,
            StateId = command.Dto.StateId,
            CountryId = command.Dto.CountryId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Companies.Add(company);
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
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt
        };
    }
}

