using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.User;

using Microsoft.Extensions.Logging;

public class GetAllUsersCommandHandler : ICommandHandler<GetAllUsersCommand, List<UserDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetAllUsersCommandHandler> _logger;

    public GetAllUsersCommandHandler(ApplicationDbContext context, ILogger<GetAllUsersCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserDto>> HandleAsync(GetAllUsersCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Company)
                .Where(u => u.DeletedAt == null);

            // Filtrar por CompanyId si se proporciona
            if (command.CompanyId.HasValue)
            {
                query = query.Where(u => u.CompanyId == command.CompanyId.Value);
            }

            // Safe projection and ordering to handle potentially null navigation properties
            // even if EF Core should handle them, InMemory sometimes behaves differently with nulls
            var users = await query
                .OrderBy(u => u.Company != null ? u.Company.Name : string.Empty)
                .ThenBy(u => u.Username)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    CompanyId = u.CompanyId,
                    CompanyName = u.Company != null ? u.Company.Name : string.Empty,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email.HasValue ? u.Email.Value.Value : null,
                    Phone = u.Phone,
                    Address = u.Address,
                    PostalCodeId = u.PostalCodeId,
                    CityId = u.CityId,
                    StateId = u.StateId,
                    CountryId = u.CountryId,
                    LanguageId = u.LanguageId,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }
}

