using GesFer.Application.Commands.Group;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Group;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Group;

public class UpdateGroupCommandHandler : ICommandHandler<UpdateGroupCommand, GroupDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateGroupCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GroupDto> HandleAsync(UpdateGroupCommand command, CancellationToken cancellationToken = default)
    {
        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == command.Id && g.DeletedAt == null, cancellationToken);

        if (group == null)
            throw new InvalidOperationException($"No se encontró el grupo con ID {command.Id}");

        // Validar que no exista otro grupo con el mismo nombre (excepto el actual)
        var existingGroup = await _context.Groups
            .FirstOrDefaultAsync(g => g.Name == command.Dto.Name && g.Id != command.Id && g.DeletedAt == null, cancellationToken);

        if (existingGroup != null)
            throw new InvalidOperationException($"Ya existe otro grupo con el nombre '{command.Dto.Name}'");

        group.Name = command.Dto.Name;
        group.Description = command.Dto.Description;
        group.IsActive = command.Dto.IsActive;
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            IsActive = group.IsActive,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt
        };
    }
}

