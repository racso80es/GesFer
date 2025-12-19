using GesFer.Application.Commands.City;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.City;

public class GetAllCitiesCommandHandler : ICommandHandler<GetAllCitiesCommand, List<CityDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllCitiesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CityDto>> HandleAsync(GetAllCitiesCommand command, CancellationToken cancellationToken = default)
    {
        var query = _context.Cities
            .Include(c => c.State)
                .ThenInclude(s => s.Country)
            .Where(c => c.DeletedAt == null);

        // Filtrar por StateId si se proporciona
        if (command.StateId.HasValue)
        {
            query = query.Where(c => c.StateId == command.StateId.Value);
        }

        // Filtrar por CountryId si se proporciona
        if (command.CountryId.HasValue)
        {
            query = query.Where(c => c.State.CountryId == command.CountryId.Value);
        }

        var cities = await query
            .Include(c => c.PostalCodes)
            .OrderBy(c => c.State.Country.Name)
            .ThenBy(c => c.State.Name)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return cities.Select(c => new CityDto
        {
            Id = c.Id,
            StateId = c.StateId,
            StateName = c.State.Name,
            CountryId = c.State.CountryId,
            CountryName = c.State.Country.Name,
            Name = c.Name,
            PostalCodes = c.PostalCodes.Where(pc => pc.DeletedAt == null).Select(pc => pc.Code).ToList(),
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }
}

