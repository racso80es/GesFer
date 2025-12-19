using GesFer.Application.Commands.PostalCode;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.PostalCode;

public class CreatePostalCodeCommandHandler : ICommandHandler<CreatePostalCodeCommand, PostalCodeDto>
{
    private readonly ApplicationDbContext _context;

    public CreatePostalCodeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PostalCodeDto> HandleAsync(CreatePostalCodeCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que la ciudad existe
        var city = await _context.Cities
            .Include(c => c.State)
                .ThenInclude(s => s.Country)
            .FirstOrDefaultAsync(c => c.Id == command.Dto.CityId && c.DeletedAt == null, cancellationToken);

        if (city == null)
            throw new InvalidOperationException($"No se encontró la ciudad con ID {command.Dto.CityId}");

        // Validar que no exista un código postal con el mismo código en la misma ciudad
        var existingPostalCode = await _context.PostalCodes
            .FirstOrDefaultAsync(pc => pc.Code == command.Dto.Code 
                && pc.CityId == command.Dto.CityId 
                && pc.DeletedAt == null, cancellationToken);

        if (existingPostalCode != null)
            throw new InvalidOperationException($"Ya existe un código postal '{command.Dto.Code}' en esta ciudad");

        var postalCode = new GesFer.Domain.Entities.PostalCode
        {
            CityId = command.Dto.CityId,
            Code = command.Dto.Code,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.PostalCodes.Add(postalCode);
        await _context.SaveChangesAsync(cancellationToken);

        // Cargar la ciudad y sus relaciones para obtener los nombres
        await _context.Entry(postalCode).Reference(pc => pc.City).LoadAsync(cancellationToken);
        await _context.Entry(postalCode.City).Reference(c => c.State).LoadAsync(cancellationToken);
        await _context.Entry(postalCode.City.State).Reference(s => s.Country).LoadAsync(cancellationToken);

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

