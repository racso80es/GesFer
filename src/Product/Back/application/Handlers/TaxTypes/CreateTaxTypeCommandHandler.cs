using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Common.Interfaces;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.TaxTypes;

public class CreateTaxTypeCommandHandler : ICommandHandler<CreateTaxTypeCommand, Guid>
{
    private readonly ApplicationDbContext _context;

    public CreateTaxTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> HandleAsync(CreateTaxTypeCommand command, CancellationToken cancellationToken = default)
    {
        var companyId = command.CompanyId ?? Guid.Empty;
        if (companyId == Guid.Empty)
            throw new InvalidOperationException("CompanyId es obligatorio.");

        var existingCode = await _context.TaxTypes
            .AnyAsync(t => t.CompanyId == companyId && t.Code == command.TaxType.Code, cancellationToken);
        if (existingCode)
            throw new InvalidOperationException("Ya existe un tipo de impuesto con este código.");

        var existingName = await _context.TaxTypes
            .AnyAsync(t => t.CompanyId == companyId && t.Name == command.TaxType.Name, cancellationToken);
        if (existingName)
            throw new InvalidOperationException("Ya existe un tipo de impuesto con este nombre.");

        if (string.IsNullOrWhiteSpace(command.TaxType.Code) || command.TaxType.Code.Length > 10)
            throw new InvalidOperationException("El código es obligatorio y máximo 10 caracteres.");
        if (string.IsNullOrWhiteSpace(command.TaxType.Name) || command.TaxType.Name.Length > 50)
            throw new InvalidOperationException("El nombre es obligatorio y máximo 50 caracteres.");
        if (command.TaxType.Value < 0)
            throw new InvalidOperationException("El valor debe ser mayor o igual a 0.");

        var taxType = new TaxType
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = command.TaxType.Code.Trim(),
            Name = command.TaxType.Name.Trim(),
            Description = command.TaxType.Description?.Trim(),
            Value = command.TaxType.Value,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.TaxTypes.Add(taxType);
        await _context.SaveChangesAsync(cancellationToken);

        return taxType.Id;
    }
}
