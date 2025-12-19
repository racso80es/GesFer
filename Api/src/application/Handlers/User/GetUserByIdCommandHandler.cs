using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.User;

public class GetUserByIdCommandHandler : ICommandHandler<GetUserByIdCommand, UserDto?>
{
    private readonly ApplicationDbContext _context;

    public GetUserByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> HandleAsync(GetUserByIdCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .Where(u => u.Id == command.Id && u.DeletedAt == null)
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
                Address = u.Address,
                PostalCodeId = u.PostalCodeId,
                CityId = u.CityId,
                StateId = u.StateId,
                CountryId = u.CountryId,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}

