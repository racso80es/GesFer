using GesFer.Application.Commands.PostalCode;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.PostalCode;

public class GetAllPostalCodesCommandHandler : ICommandHandler<GetAllPostalCodesCommand, List<PostalCodeDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllPostalCodesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostalCodeDto>> HandleAsync(GetAllPostalCodesCommand command, CancellationToken cancellationToken = default)
    {
        var query = _context.PostalCodes
            .Include(pc => pc.City)
                .ThenInclude(c => c.State)
                    .ThenInclude(s => s.Country)
            .Where(pc => pc.DeletedAt == null);

        // Filtrar por CityId si se proporciona
        if (command.CityId.HasValue)
        {
            query = query.Where(pc => pc.CityId == command.CityId.Value);
        }

        // Filtrar por StateId si se proporciona
        if (command.StateId.HasValue)
        {
            query = query.Where(pc => pc.City.StateId == command.StateId.Value);
        }

        // Filtrar por CountryId si se proporciona
        if (command.CountryId.HasValue)
        {
            query = query.Where(pc => pc.City.State.CountryId == command.CountryId.Value);
        }

        var postalCodes = await query
            .OrderBy(pc => pc.City.State.Country.Name)
            .ThenBy(pc => pc.City.State.Name)
            .ThenBy(pc => pc.City.Name)
            .ThenBy(pc => pc.Code)
            .Select(pc => new PostalCodeDto
            {
                Id = pc.Id,
                CityId = pc.CityId,
                CityName = pc.City.Name,
                StateId = pc.City.StateId,
                StateName = pc.City.State.Name,
                CountryId = pc.City.State.CountryId,
                CountryName = pc.City.State.Country.Name,
                Code = pc.Code,
                IsActive = pc.IsActive,
                CreatedAt = pc.CreatedAt,
                UpdatedAt = pc.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return postalCodes;
    }
}

