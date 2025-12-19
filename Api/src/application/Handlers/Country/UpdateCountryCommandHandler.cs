using GesFer.Application.Commands.Country;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Country;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Country;

public class UpdateCountryCommandHandler : ICommandHandler<UpdateCountryCommand, CountryDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateCountryCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CountryDto> HandleAsync(UpdateCountryCommand command, CancellationToken cancellationToken = default)
    {
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.DeletedAt == null, cancellationToken);

        if (country == null)
            throw new InvalidOperationException($"No se encontró el país con ID {command.Id}");

        // Validar que no exista otro país con el mismo código (excepto el actual)
        var existingCountry = await _context.Countries
            .FirstOrDefaultAsync(c => c.Code == command.Dto.Code && c.Id != command.Id && c.DeletedAt == null, cancellationToken);

        if (existingCountry != null)
            throw new InvalidOperationException($"Ya existe otro país con el código '{command.Dto.Code}'");

        // Validar que no exista otro país con el mismo nombre (excepto el actual)
        var existingCountryByName = await _context.Countries
            .FirstOrDefaultAsync(c => c.Name == command.Dto.Name && c.Id != command.Id && c.DeletedAt == null, cancellationToken);

        if (existingCountryByName != null)
            throw new InvalidOperationException($"Ya existe otro país con el nombre '{command.Dto.Name}'");

        country.Name = command.Dto.Name;
        country.Code = command.Dto.Code;
        country.IsActive = command.Dto.IsActive;
        country.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CountryDto
        {
            Id = country.Id,
            Name = country.Name,
            Code = country.Code,
            IsActive = country.IsActive,
            CreatedAt = country.CreatedAt,
            UpdatedAt = country.UpdatedAt
        };
    }
}

