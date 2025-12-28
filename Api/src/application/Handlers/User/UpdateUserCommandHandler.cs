using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.User;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateUserCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == command.Id && u.DeletedAt == null, cancellationToken);

        if (user == null)
            throw new InvalidOperationException($"No se encontró el usuario con ID {command.Id}");

        // Validar que no exista otro usuario con el mismo username en la misma empresa (excepto el actual)
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == command.Dto.Username 
                && u.CompanyId == user.CompanyId 
                && u.Id != command.Id 
                && u.DeletedAt == null, cancellationToken);

        if (existingUser != null)
            throw new InvalidOperationException($"Ya existe otro usuario con el nombre '{command.Dto.Username}' en esta empresa");

        // Validar IDs de dirección si se proporcionan
        if (command.Dto.PostalCodeId.HasValue)
        {
            var postalCodeExists = await _context.PostalCodes
                .AnyAsync(pc => pc.Id == command.Dto.PostalCodeId.Value && pc.DeletedAt == null, cancellationToken);
            if (!postalCodeExists)
                throw new InvalidOperationException($"No se encontró el código postal con ID {command.Dto.PostalCodeId.Value}");
        }

        if (command.Dto.CityId.HasValue)
        {
            var cityExists = await _context.Cities
                .AnyAsync(c => c.Id == command.Dto.CityId.Value && c.DeletedAt == null, cancellationToken);
            if (!cityExists)
                throw new InvalidOperationException($"No se encontró la ciudad con ID {command.Dto.CityId.Value}");
        }

        if (command.Dto.StateId.HasValue)
        {
            var stateExists = await _context.States
                .AnyAsync(s => s.Id == command.Dto.StateId.Value && s.DeletedAt == null, cancellationToken);
            if (!stateExists)
                throw new InvalidOperationException($"No se encontró la provincia con ID {command.Dto.StateId.Value}");
        }

        if (command.Dto.CountryId.HasValue)
        {
            var countryExists = await _context.Countries
                .AnyAsync(c => c.Id == command.Dto.CountryId.Value && c.DeletedAt == null, cancellationToken);
            if (!countryExists)
                throw new InvalidOperationException($"No se encontró el país con ID {command.Dto.CountryId.Value}");
        }

        if (command.Dto.LanguageId.HasValue)
        {
            var languageExists = await _context.Languages.AnyAsync(l => l.Id == command.Dto.LanguageId && l.DeletedAt == null, cancellationToken);
            if (!languageExists)
                throw new InvalidOperationException($"No se encontró el idioma con ID {command.Dto.LanguageId}");
        }

        user.Username = command.Dto.Username;
        user.FirstName = command.Dto.FirstName;
        user.LastName = command.Dto.LastName;
        user.Email = command.Dto.Email;
        user.Phone = command.Dto.Phone;
        user.Address = command.Dto.Address;
        user.PostalCodeId = command.Dto.PostalCodeId;
        user.CityId = command.Dto.CityId;
        user.StateId = command.Dto.StateId;
        user.CountryId = command.Dto.CountryId;
        // LanguageId: si viene en el DTO, usarlo (puede ser null explícitamente). Si no viene, mantener el actual.
        // Como el DTO siempre incluye LanguageId (puede ser null), lo asignamos directamente.
        user.LanguageId = command.Dto.LanguageId;
        user.IsActive = command.Dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        // Actualizar contraseña solo si se proporciona
        if (!string.IsNullOrWhiteSpace(command.Dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Dto.Password, BCrypt.Net.BCrypt.GenerateSalt(11));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            CompanyName = user.Company.Name,
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

