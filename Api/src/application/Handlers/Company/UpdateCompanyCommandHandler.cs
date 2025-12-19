using GesFer.Application.Commands.Company;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Company;

public class UpdateCompanyCommandHandler : ICommandHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateCompanyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> HandleAsync(UpdateCompanyCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.DeletedAt == null, cancellationToken);

        if (company == null)
            throw new InvalidOperationException($"No se encontró la empresa con ID {command.Id}");

        // Validar que no exista otra empresa con el mismo nombre (excepto la actual)
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == command.Dto.Name && c.Id != command.Id && c.DeletedAt == null, cancellationToken);

        if (existingCompany != null)
            throw new InvalidOperationException($"Ya existe otra empresa con el nombre '{command.Dto.Name}'");

        company.Name = command.Dto.Name;
        company.TaxId = command.Dto.TaxId;
        company.Address = command.Dto.Address;
        company.Phone = command.Dto.Phone;
        company.Email = command.Dto.Email;
        company.IsActive = command.Dto.IsActive;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            TaxId = company.TaxId,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt
        };
    }
}

