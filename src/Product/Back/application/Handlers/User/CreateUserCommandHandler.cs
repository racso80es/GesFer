using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.User;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IAdminApiClient _adminApiClient;

    public CreateUserCommandHandler(ApplicationDbContext context, IAdminApiClient adminApiClient)
    {
        _context = context;
        _adminApiClient = adminApiClient;
    }

    public async Task<UserDto> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _adminApiClient.GetCompanyAsync(command.Dto.CompanyId);
        if (company == null)
            throw new InvalidOperationException($"No se encontró la empresa con ID {command.Dto.CompanyId}");

        // Validar que no exista un usuario con el mismo username en la misma empresa
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == command.Dto.Username
                && u.CompanyId == command.Dto.CompanyId
                && u.DeletedAt == null, cancellationToken);

        if (existingUser != null)
            throw new InvalidOperationException($"Ya existe un usuario con el nombre '{command.Dto.Username}' en esta empresa");

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

        // Validar LanguageId si se proporciona
        if (command.Dto.LanguageId.HasValue)
        {
            var languageExists = await _context.Languages
                .AnyAsync(l => l.Id == command.Dto.LanguageId.Value && l.DeletedAt == null, cancellationToken);
            if (!languageExists)
                throw new InvalidOperationException($"No se encontró el idioma con ID {command.Dto.LanguageId.Value}");
        }

        // Hash de la contraseña
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Dto.Password, BCrypt.Net.BCrypt.GenerateSalt(11));

        // Validar y convertir Email si se proporciona
        Email? email = null;
        if (!string.IsNullOrWhiteSpace(command.Dto.Email))
        {
            email = Email.Create(command.Dto.Email);
        }

        var user = new Product.Back.Domain.Entities.User
        {
            CompanyId = command.Dto.CompanyId,
            Username = command.Dto.Username,
            PasswordHash = passwordHash,
            FirstName = command.Dto.FirstName,
            LastName = command.Dto.LastName,
            Email = email,
            Phone = command.Dto.Phone,
            Address = command.Dto.Address,
            PostalCodeId = command.Dto.PostalCodeId,
            CityId = command.Dto.CityId,
            StateId = command.Dto.StateId,
            CountryId = command.Dto.CountryId,
            LanguageId = command.Dto.LanguageId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            CompanyName = company.Name,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email.HasValue ? user.Email.Value.Value : null,
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

