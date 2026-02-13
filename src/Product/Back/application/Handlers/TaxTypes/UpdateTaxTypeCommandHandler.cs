using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Common.Interfaces;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.TaxTypes;

public class UpdateTaxTypeCommandHandler : ICommandHandler<UpdateTaxTypeCommand>
{
    private readonly ApplicationDbContext _context;

    public UpdateTaxTypeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(UpdateTaxTypeCommand command, CancellationToken cancellationToken = default)
    {
        var companyId = command.CompanyId ?? Guid.Empty;
        if (companyId == Guid.Empty)
            throw new InvalidOperationException("CompanyId es obligatorio.");

        var taxType = await _context.TaxTypes
            .FirstOrDefaultAsync(t => t.Id == command.TaxType.Id && t.CompanyId == companyId, cancellationToken);

        if (taxType == null)
            throw new InvalidOperationException("Tipo de impuesto no encontrado.");

        if (taxType.Code != command.TaxType.Code)
        {
            var existingCode = await _context.TaxTypes
                .AnyAsync(t => t.CompanyId == companyId && t.Code == command.TaxType.Code && t.Id != taxType.Id, cancellationToken);
            if (existingCode)
                throw new InvalidOperationException("Ya existe un tipo de impuesto con este código.");
        }

        if (taxType.Name != command.TaxType.Name)
        {
            var existingName = await _context.TaxTypes
                .AnyAsync(t => t.CompanyId == companyId && t.Name == command.TaxType.Name && t.Id != taxType.Id, cancellationToken);
            if (existingName)
                throw new InvalidOperationException("Ya existe un tipo de impuesto con este nombre.");
        }

        if (string.IsNullOrWhiteSpace(command.TaxType.Code) || command.TaxType.Code.Length > 10)
            throw new InvalidOperationException("El código es obligatorio y máximo 10 caracteres.");
        if (string.IsNullOrWhiteSpace(command.TaxType.Name) || command.TaxType.Name.Length > 50)
            throw new InvalidOperationException("El nombre es obligatorio y máximo 50 caracteres.");
        if (command.TaxType.Value < 0)
            throw new InvalidOperationException("El valor debe ser mayor o igual a 0.");

        taxType.Code = command.TaxType.Code.Trim();
        taxType.Name = command.TaxType.Name.Trim();
        taxType.Description = command.TaxType.Description?.Trim();
        taxType.Value = command.TaxType.Value;
        taxType.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
