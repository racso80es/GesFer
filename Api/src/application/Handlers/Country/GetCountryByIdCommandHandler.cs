using GesFer.Application.Commands.Country;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Country;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Country;

public class GetCountryByIdCommandHandler : ICommandHandler<GetCountryByIdCommand, CountryDto?>
{
    private readonly ApplicationDbContext _context;

    public GetCountryByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CountryDto?> HandleAsync(GetCountryByIdCommand command, CancellationToken cancellationToken = default)
    {
        var country = await _context.Countries
            .Where(c => c.Id == command.Id && c.DeletedAt == null)
            .Select(c => new CountryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return country;
    }
}

