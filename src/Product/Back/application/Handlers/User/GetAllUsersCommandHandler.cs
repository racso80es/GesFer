using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GesFer.Application.Handlers.User;

public class GetAllUsersCommandHandler : ICommandHandler<GetAllUsersCommand, List<UserDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IAdminApiClient _adminApiClient;
    private readonly ILogger<GetAllUsersCommandHandler> _logger;

    public GetAllUsersCommandHandler(ApplicationDbContext context, IAdminApiClient adminApiClient, ILogger<GetAllUsersCommandHandler> logger)
    {
        _context = context;
        _adminApiClient = adminApiClient;
        _logger = logger;
    }

    public async Task<List<UserDto>> HandleAsync(GetAllUsersCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Users.Where(u => u.DeletedAt == null);
            if (command.CompanyId.HasValue)
                query = query.Where(u => u.CompanyId == command.CompanyId.Value);

            var list = await query.OrderBy(u => u.CompanyId).ThenBy(u => u.Username)
                .Select(u => new { u.Id, u.CompanyId, u.Username, u.FirstName, u.LastName, u.Email, u.Phone, u.Address, u.PostalCodeId, u.CityId, u.StateId, u.CountryId, u.LanguageId, u.IsActive, u.CreatedAt, u.UpdatedAt })
                .ToListAsync(cancellationToken);

            var companyIds = list.Select(u => u.CompanyId).Distinct().ToList();
            var companyNames = new Dictionary<Guid, string>();
            foreach (var id in companyIds)
            {
                var c = await _adminApiClient.GetCompanyAsync(id);
                if (c != null) companyNames[id] = c.Name;
            }

            return list.Select(u => new UserDto
            {
                Id = u.Id,
                CompanyId = u.CompanyId,
                CompanyName = companyNames.GetValueOrDefault(u.CompanyId, string.Empty),
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
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }
}

