using GesFer.Application.Commands.PostalCode;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.PostalCode;

public class GetPostalCodeByIdCommandHandler : ICommandHandler<GetPostalCodeByIdCommand, PostalCodeDto?>
{
    private readonly ApplicationDbContext _context;

    public GetPostalCodeByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PostalCodeDto?> HandleAsync(GetPostalCodeByIdCommand command, CancellationToken cancellationToken = default)
    {
        var postalCode = await _context.PostalCodes
            .Include(pc => pc.City)
                .ThenInclude(c => c.State)
                    .ThenInclude(s => s.Country)
            .Where(pc => pc.Id == command.Id && pc.DeletedAt == null)
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
            .FirstOrDefaultAsync(cancellationToken);

        return postalCode;
    }
}

