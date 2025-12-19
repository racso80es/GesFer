using GesFer.Application.Commands.Group;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Group;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Group;

public class CreateGroupCommandHandler : ICommandHandler<CreateGroupCommand, GroupDto>
{
    private readonly ApplicationDbContext _context;

    public CreateGroupCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GroupDto> HandleAsync(CreateGroupCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que no exista un grupo con el mismo nombre
        var existingGroup = await _context.Groups
            .FirstOrDefaultAsync(g => g.Name == command.Dto.Name && g.DeletedAt == null, cancellationToken);

        if (existingGroup != null)
            throw new InvalidOperationException($"Ya existe un grupo con el nombre '{command.Dto.Name}'");

        var group = new GesFer.Domain.Entities.Group
        {
            Name = command.Dto.Name,
            Description = command.Dto.Description,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Groups.Add(group);
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

