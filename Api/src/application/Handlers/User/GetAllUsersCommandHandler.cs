using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.User;

public class GetAllUsersCommandHandler : ICommandHandler<GetAllUsersCommand, List<UserDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllUsersCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> HandleAsync(GetAllUsersCommand command, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.Company)
            .Where(u => u.DeletedAt == null);

        // Filtrar por CompanyId si se proporciona
        if (command.CompanyId.HasValue)
        {
            query = query.Where(u => u.CompanyId == command.CompanyId.Value);
        }

        var users = await query
            .OrderBy(u => u.Company.Name)
            .ThenBy(u => u.Username)
            .Select(u => new UserDto
            {
                Id = u.Id,
                CompanyId = u.CompanyId,
                CompanyName = u.Company.Name,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}

