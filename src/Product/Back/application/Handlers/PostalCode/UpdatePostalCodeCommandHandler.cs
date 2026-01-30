using GesFer.Application.Commands.PostalCode;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.PostalCode;

public class UpdatePostalCodeCommandHandler : ICommandHandler<UpdatePostalCodeCommand, PostalCodeDto>
{
    private readonly ApplicationDbContext _context;

    public UpdatePostalCodeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PostalCodeDto> HandleAsync(UpdatePostalCodeCommand command, CancellationToken cancellationToken = default)
    {
        var postalCode = await _context.PostalCodes
            .Include(pc => pc.City)
                .ThenInclude(c => c.State)
                    .ThenInclude(s => s.Country)
            .FirstOrDefaultAsync(pc => pc.Id == command.Id && pc.DeletedAt == null, cancellationToken);

        if (postalCode == null)
            throw new InvalidOperationException($"No se encontró el código postal con ID {command.Id}");

        // Validar que no exista otro código postal con el mismo código en la misma ciudad (excepto el actual)
        var existingPostalCode = await _context.PostalCodes
            .FirstOrDefaultAsync(pc => pc.Code == command.Dto.Code 
                && pc.CityId == postalCode.CityId 
                && pc.Id != command.Id 
                && pc.DeletedAt == null, cancellationToken);

        if (existingPostalCode != null)
            throw new InvalidOperationException($"Ya existe otro código postal '{command.Dto.Code}' en esta ciudad");

        postalCode.Code = command.Dto.Code;
        postalCode.IsActive = command.Dto.IsActive;
        postalCode.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new PostalCodeDto
        {
            Id = postalCode.Id,
            CityId = postalCode.CityId,
            CityName = postalCode.City.Name,
            StateId = postalCode.City.StateId,
            StateName = postalCode.City.State.Name,
            CountryId = postalCode.City.State.CountryId,
            CountryName = postalCode.City.State.Country.Name,
            Code = postalCode.Code,
            IsActive = postalCode.IsActive,
            CreatedAt = postalCode.CreatedAt,
            UpdatedAt = postalCode.UpdatedAt
        };
    }
}

