using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.ArticleFamilies;

public class CreateArticleFamilyCommandHandler : ICommandHandler<CreateArticleFamilyCommand, ArticleFamilyDto>
{
    private readonly ApplicationDbContext _context;

    public CreateArticleFamilyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ArticleFamilyDto> HandleAsync(CreateArticleFamilyCommand command, CancellationToken cancellationToken = default)
    {
        var companyId = command.Dto.CompanyId;
        if (companyId == Guid.Empty)
            throw new InvalidOperationException("CompanyId es obligatorio.");

        var existsCode = await _context.ArticleFamilies
            .AnyAsync(af => af.CompanyId == companyId && af.Code == command.Dto.Code && af.DeletedAt == null, cancellationToken);
        if (existsCode)
            throw new InvalidOperationException("Ya existe una familia de artículos con este código en la empresa.");

        var taxTypeExists = await _context.TaxTypes
            .AnyAsync(t => t.Id == command.Dto.TaxTypeId && t.CompanyId == companyId && t.DeletedAt == null, cancellationToken);
        if (!taxTypeExists)
            throw new InvalidOperationException("El tipo de tasa no existe o no pertenece a la empresa.");

        var entity = new ArticleFamily
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = command.Dto.Code,
            Name = command.Dto.Name,
            Description = command.Dto.Description,
            TaxTypeId = command.Dto.TaxTypeId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ArticleFamilies.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var taxType = await _context.TaxTypes.FindAsync(new object[] { entity.TaxTypeId }, cancellationToken);
        return new ArticleFamilyDto
        {
            Id = entity.Id,
            CompanyId = entity.CompanyId,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            TaxTypeId = entity.TaxTypeId,
            TaxTypeName = taxType?.Name,
            TaxTypeValue = taxType?.Value,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
