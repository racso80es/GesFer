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

        user.Username = command.Dto.Username;
        user.FirstName = command.Dto.FirstName;
        user.LastName = command.Dto.LastName;
        user.Email = command.Dto.Email;
        user.Phone = command.Dto.Phone;
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
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

