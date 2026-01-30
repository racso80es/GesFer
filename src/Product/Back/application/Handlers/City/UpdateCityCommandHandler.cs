using GesFer.Application.Commands.City;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.City;

public class UpdateCityCommandHandler : ICommandHandler<UpdateCityCommand, CityDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateCityCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CityDto> HandleAsync(UpdateCityCommand command, CancellationToken cancellationToken = default)
    {
        var city = await _context.Cities
            .Include(c => c.State)
                .ThenInclude(s => s.Country)
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.DeletedAt == null, cancellationToken);

        if (city == null)
            throw new InvalidOperationException($"No se encontró la ciudad con ID {command.Id}");

        // Validar que no exista otra ciudad con el mismo nombre en la misma provincia (excepto la actual)
        var existingCity = await _context.Cities
            .FirstOrDefaultAsync(c => c.Name == command.Dto.Name 
                && c.StateId == city.StateId 
                && c.Id != command.Id 
                && c.DeletedAt == null, cancellationToken);

        if (existingCity != null)
            throw new InvalidOperationException($"Ya existe otra ciudad con el nombre '{command.Dto.Name}' en esta provincia");

        city.Name = command.Dto.Name;
        city.IsActive = command.Dto.IsActive;
        city.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Cargar los códigos postales
        await _context.Entry(city).Collection(c => c.PostalCodes).LoadAsync(cancellationToken);

        return new CityDto
        {
            Id = city.Id,
            StateId = city.StateId,
            StateName = city.State.Name,
            CountryId = city.State.CountryId,
            CountryName = city.State.Country.Name,
            Name = city.Name,
            PostalCodes = city.PostalCodes.Where(pc => pc.DeletedAt == null).Select(pc => pc.Code).ToList(),
            IsActive = city.IsActive,
            CreatedAt = city.CreatedAt,
            UpdatedAt = city.UpdatedAt
        };
    }
}

