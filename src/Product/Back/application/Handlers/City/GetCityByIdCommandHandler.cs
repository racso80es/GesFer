using GesFer.Application.Commands.City;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.City;

public class GetCityByIdCommandHandler : ICommandHandler<GetCityByIdCommand, CityDto?>
{
    private readonly ApplicationDbContext _context;

    public GetCityByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CityDto?> HandleAsync(GetCityByIdCommand command, CancellationToken cancellationToken = default)
    {
        var city = await _context.Cities
            .Include(c => c.State)
                .ThenInclude(s => s.Country)
            .Include(c => c.PostalCodes)
            .Where(c => c.Id == command.Id && c.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (city == null)
            return null;

        return new CityDto
        {
            Id = city.Id,
            StateId = city.StateId,
            StateName = city.State.Name,
            CountryId = city.State.CountryId,
            CountryName = city.State.Country.Name,
            Name = city.Name,
            PostalCodes = city.PostalCodes.Where(pc => pc.DeletedAt == null).Select(pc => pc.Code).ToList(),
            IsActive = city.IsActive,
            CreatedAt = city.CreatedAt,
            UpdatedAt = city.UpdatedAt
        };
    }
}

