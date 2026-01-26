using GesFer.Application.Commands.State;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.State;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.State;

public class CreateStateCommandHandler : ICommandHandler<CreateStateCommand, StateDto>
{
    private readonly ApplicationDbContext _context;

    public CreateStateCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StateDto> HandleAsync(CreateStateCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que el país existe
        var country = await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == command.Dto.CountryId && c.DeletedAt == null, cancellationToken);

        if (country == null)
            throw new InvalidOperationException($"No se encontró el país con ID {command.Dto.CountryId}");

        // Validar que no exista una provincia con el mismo nombre en el mismo país
        var existingState = await _context.States
            .FirstOrDefaultAsync(s => s.Name == command.Dto.Name 
                && s.CountryId == command.Dto.CountryId 
                && s.DeletedAt == null, cancellationToken);

        if (existingState != null)
            throw new InvalidOperationException($"Ya existe una provincia con el nombre '{command.Dto.Name}' en este país");

        var state = new GesFer.Domain.Entities.State
        {
            CountryId = command.Dto.CountryId,
            Name = command.Dto.Name,
            Code = command.Dto.Code,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.States.Add(state);
        await _context.SaveChangesAsync(cancellationToken);

        // Cargar el país para obtener el nombre
        await _context.Entry(state).Reference(s => s.Country).LoadAsync(cancellationToken);

        return new StateDto
        {
            Id = state.Id,
            CountryId = state.CountryId,
            CountryName = state.Country.Name,
            CountryCode = state.Country.Code,
            Name = state.Name,
            Code = state.Code,
            IsActive = state.IsActive,
            CreatedAt = state.CreatedAt,
            UpdatedAt = state.UpdatedAt
        };
    }
}

