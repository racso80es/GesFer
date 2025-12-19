using GesFer.Application.Commands.Group;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Group;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Group;

public class GetAllGroupsCommandHandler : ICommandHandler<GetAllGroupsCommand, List<GroupDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllGroupsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GroupDto>> HandleAsync(GetAllGroupsCommand command, CancellationToken cancellationToken = default)
    {
        var groups = await _context.Groups
            .Where(g => g.DeletedAt == null)
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                IsActive = g.IsActive,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return groups;
    }
}

