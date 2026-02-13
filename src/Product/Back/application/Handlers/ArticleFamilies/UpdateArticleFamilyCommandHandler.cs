using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.ArticleFamilies;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.ArticleFamilies;

public class UpdateArticleFamilyCommandHandler : ICommandHandler<UpdateArticleFamilyCommand, ArticleFamilyDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateArticleFamilyCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ArticleFamilyDto> HandleAsync(UpdateArticleFamilyCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ArticleFamilies
            .Include(af => af.TaxType)
            .FirstOrDefaultAsync(af => af.Id == command.Id && af.DeletedAt == null, cancellationToken);

        if (entity == null)
            throw new InvalidOperationException($"No se encontró la familia de artículos con ID {command.Id}.");

        if (entity.Code != command.Dto.Code)
        {
            var existsCode = await _context.ArticleFamilies
                .AnyAsync(af => af.CompanyId == entity.CompanyId && af.Code == command.Dto.Code && af.Id != command.Id && af.DeletedAt == null, cancellationToken);
            if (existsCode)
                throw new InvalidOperationException("Ya existe una familia de artículos con este código en la empresa.");
        }

        if (command.Dto.TaxTypeId != entity.TaxTypeId)
        {
            var taxTypeExists = await _context.TaxTypes
                .AnyAsync(t => t.Id == command.Dto.TaxTypeId && t.CompanyId == entity.CompanyId && t.DeletedAt == null, cancellationToken);
            if (!taxTypeExists)
                throw new InvalidOperationException("El tipo de tasa no existe o no pertenece a la empresa.");
        }

        entity.Code = command.Dto.Code;
        entity.Name = command.Dto.Name;
        entity.Description = command.Dto.Description;
        entity.TaxTypeId = command.Dto.TaxTypeId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ArticleFamilyDto
        {
            Id = entity.Id,
            CompanyId = entity.CompanyId,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            TaxTypeId = entity.TaxTypeId,
            TaxTypeName = entity.TaxType?.Name,
            TaxTypeValue = entity.TaxType?.Value,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
