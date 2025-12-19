using GesFer.Application.Commands.Country;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Country;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Country;

public class GetAllCountriesCommandHandler : ICommandHandler<GetAllCountriesCommand, List<CountryDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllCountriesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CountryDto>> HandleAsync(GetAllCountriesCommand command, CancellationToken cancellationToken = default)
    {
        var countries = await _context.Countries
            .Where(c => c.DeletedAt == null)
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return countries;
    }
}

