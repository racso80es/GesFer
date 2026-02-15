using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.User;

public class GetUserByIdCommandHandler : ICommandHandler<GetUserByIdCommand, UserDto?>
{
    private readonly ApplicationDbContext _context;
    private readonly IAdminApiClient _adminApiClient;

    public GetUserByIdCommandHandler(ApplicationDbContext context, IAdminApiClient adminApiClient)
    {
        _context = context;
        _adminApiClient = adminApiClient;
    }

    public async Task<UserDto?> HandleAsync(GetUserByIdCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(u => u.Id == command.Id && u.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
        if (user == null) return null;

        var company = await _adminApiClient.GetCompanyAsync(user.CompanyId);
        return new UserDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            CompanyName = company?.Name ?? string.Empty,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            PostalCodeId = user.PostalCodeId,
            CityId = user.CityId,
            StateId = user.StateId,
            CountryId = user.CountryId,
            LanguageId = user.LanguageId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

