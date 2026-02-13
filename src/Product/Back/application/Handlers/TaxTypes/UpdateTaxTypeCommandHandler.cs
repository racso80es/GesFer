using FluentValidation;
using GesFer.Product.Application.Commands.TaxTypes;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.Application.Handlers.TaxTypes;

public class UpdateTaxTypeCommandHandler : ICommandHandler<UpdateTaxTypeCommand, Result>
{
    private readonly ApplicationDbContext _context;
    private readonly IUserContext _userContext;

    public UpdateTaxTypeCommandHandler(ApplicationDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<Result> Handle(UpdateTaxTypeCommand request, CancellationToken cancellationToken)
    {
        var companyId = _userContext.CompanyId;
        var taxType = await _context.TaxTypes
            .FirstOrDefaultAsync(t => t.Id == request.TaxType.Id && t.CompanyId == companyId, cancellationToken);

        if (taxType == null)
        {
            return Result.Failure(new Error("TaxType.NotFound", "Tax type not found."));
        }

        // Check uniqueness if changed
        if (taxType.Code != request.TaxType.Code)
        {
            var existingCode = await _context.TaxTypes
                .AnyAsync(t => t.CompanyId == companyId && t.Code == request.TaxType.Code && t.Id != taxType.Id, cancellationToken);
            if (existingCode)
            {
                return Result.Failure(new Error("TaxType.Validation", "A tax type with this code already exists."));
            }
        }

        if (taxType.Name != request.TaxType.Name)
        {
            var existingName = await _context.TaxTypes
                .AnyAsync(t => t.CompanyId == companyId && t.Name == request.TaxType.Name && t.Id != taxType.Id, cancellationToken);
            if (existingName)
            {
                return Result.Failure(new Error("TaxType.Validation", "A tax type with this name already exists."));
            }
        }

        taxType.Code = request.TaxType.Code;
        taxType.Name = request.TaxType.Name;
        taxType.Description = request.TaxType.Description;
        taxType.Value = request.TaxType.Value;
        taxType.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateTaxTypeValidator : AbstractValidator<UpdateTaxTypeCommand>
{
    public UpdateTaxTypeValidator()
    {
        RuleFor(x => x.TaxType.Id).NotEmpty();
        RuleFor(x => x.TaxType.Code).NotEmpty().MaximumLength(10);
        RuleFor(x => x.TaxType.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TaxType.Value).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxType.Description).MaximumLength(255);
    }
}
