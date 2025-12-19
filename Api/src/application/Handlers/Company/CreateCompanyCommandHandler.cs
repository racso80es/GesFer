using GesFer.Application.Commands.Company;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Company;

public class CreateCompanyCommandHandler : ICommandHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly ApplicationDbContext _context;

    public CreateCompanyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDto> HandleAsync(CreateCompanyCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que no exista una empresa con el mismo nombre
        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == command.Dto.Name && c.DeletedAt == null, cancellationToken);

        if (existingCompany != null)
            throw new InvalidOperationException($"Ya existe una empresa con el nombre '{command.Dto.Name}'");

        var company = new GesFer.Domain.Entities.Company
        {
            Name = command.Dto.Name,
            TaxId = command.Dto.TaxId,
            Address = command.Dto.Address,
            Phone = command.Dto.Phone,
            Email = command.Dto.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Companies.Add(company);
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

