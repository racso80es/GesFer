using GesFer.Application.Commands.State;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.State;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.State;

public class UpdateStateCommandHandler : ICommandHandler<UpdateStateCommand, StateDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateStateCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StateDto> HandleAsync(UpdateStateCommand command, CancellationToken cancellationToken = default)
    {
        var state = await _context.States
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.DeletedAt == null, cancellationToken);

        if (state == null)
            throw new InvalidOperationException($"No se encontró la provincia con ID {command.Id}");

        // Validar que no exista otra provincia con el mismo nombre en el mismo país (excepto la actual)
        var existingState = await _context.States
            .FirstOrDefaultAsync(s => s.Name == command.Dto.Name 
                && s.CountryId == state.CountryId 
                && s.Id != command.Id 
                && s.DeletedAt == null, cancellationToken);

        if (existingState != null)
            throw new InvalidOperationException($"Ya existe otra provincia con el nombre '{command.Dto.Name}' en este país");

        state.Name = command.Dto.Name;
        state.Code = command.Dto.Code;
        state.IsActive = command.Dto.IsActive;
        state.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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

