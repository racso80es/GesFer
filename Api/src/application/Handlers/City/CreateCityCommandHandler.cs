using GesFer.Application.Commands.City;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.City;

public class CreateCityCommandHandler : ICommandHandler<CreateCityCommand, CityDto>
{
    private readonly ApplicationDbContext _context;

    public CreateCityCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CityDto> HandleAsync(CreateCityCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que la provincia existe
        var state = await _context.States
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.Id == command.Dto.StateId && s.DeletedAt == null, cancellationToken);

        if (state == null)
            throw new InvalidOperationException($"No se encontró la provincia con ID {command.Dto.StateId}");

        // Validar que no exista una ciudad con el mismo nombre en la misma provincia
        var existingCity = await _context.Cities
            .FirstOrDefaultAsync(c => c.Name == command.Dto.Name 
                && c.StateId == command.Dto.StateId 
                && c.DeletedAt == null, cancellationToken);

        if (existingCity != null)
            throw new InvalidOperationException($"Ya existe una ciudad con el nombre '{command.Dto.Name}' en esta provincia");

        var city = new GesFer.Domain.Entities.City
        {
            StateId = command.Dto.StateId,
            Name = command.Dto.Name,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Cities.Add(city);
        await _context.SaveChangesAsync(cancellationToken);

        // Cargar la provincia, el país y los códigos postales para obtener los nombres
        await _context.Entry(city).Reference(c => c.State).LoadAsync(cancellationToken);
        await _context.Entry(city.State).Reference(s => s.Country).LoadAsync(cancellationToken);
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

