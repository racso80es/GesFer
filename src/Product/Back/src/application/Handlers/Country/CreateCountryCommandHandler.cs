using GesFer.Application.Commands.Country;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Country;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Country;

public class CreateCountryCommandHandler : ICommandHandler<CreateCountryCommand, CountryDto>
{
    private readonly ApplicationDbContext _context;

    public CreateCountryCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CountryDto> HandleAsync(CreateCountryCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que no exista un país con el mismo código
        var existingCountry = await _context.Countries
            .FirstOrDefaultAsync(c => c.Code == command.Dto.Code && c.DeletedAt == null, cancellationToken);

        if (existingCountry != null)
            throw new InvalidOperationException($"Ya existe un país con el código '{command.Dto.Code}'");

        // Validar que no exista un país con el mismo nombre
        var existingCountryByName = await _context.Countries
            .FirstOrDefaultAsync(c => c.Name == command.Dto.Name && c.DeletedAt == null, cancellationToken);

        if (existingCountryByName != null)
            throw new InvalidOperationException($"Ya existe un país con el nombre '{command.Dto.Name}'");

        // Validar idioma
        var languageExists = await _context.Languages
            .AnyAsync(l => l.Id == command.Dto.LanguageId && l.DeletedAt == null, cancellationToken);
        if (!languageExists)
            throw new InvalidOperationException($"No se encontró el idioma con ID {command.Dto.LanguageId}");

        var country = new GesFer.Domain.Entities.Country
        {
            Name = command.Dto.Name,
            Code = command.Dto.Code,
            LanguageId = command.Dto.LanguageId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Countries.Add(country);
        await _context.SaveChangesAsync(cancellationToken);

        return new CountryDto
        {
            Id = country.Id,
            Name = country.Name,
            Code = country.Code,
            LanguageId = country.LanguageId,
            IsActive = country.IsActive,
            CreatedAt = country.CreatedAt,
            UpdatedAt = country.UpdatedAt
        };
    }
}

