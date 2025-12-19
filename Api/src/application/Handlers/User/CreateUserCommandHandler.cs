using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.User;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly ApplicationDbContext _context;

    public CreateUserCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que la empresa existe
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == command.Dto.CompanyId && c.DeletedAt == null, cancellationToken);

        if (company == null)
            throw new InvalidOperationException($"No se encontró la empresa con ID {command.Dto.CompanyId}");

        // Validar que no exista un usuario con el mismo username en la misma empresa
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == command.Dto.Username 
                && u.CompanyId == command.Dto.CompanyId 
                && u.DeletedAt == null, cancellationToken);

        if (existingUser != null)
            throw new InvalidOperationException($"Ya existe un usuario con el nombre '{command.Dto.Username}' en esta empresa");

        // Hash de la contraseña
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Dto.Password, BCrypt.Net.BCrypt.GenerateSalt(11));

        var user = new GesFer.Domain.Entities.User
        {
            CompanyId = command.Dto.CompanyId,
            Username = command.Dto.Username,
            PasswordHash = passwordHash,
            FirstName = command.Dto.FirstName,
            LastName = command.Dto.LastName,
            Email = command.Dto.Email,
            Phone = command.Dto.Phone,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Cargar la empresa para obtener el nombre
        await _context.Entry(user).Reference(u => u.Company).LoadAsync(cancellationToken);

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
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

