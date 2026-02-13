using FluentValidation;
using GesFer.Product.Application.Commands.TaxTypes;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.Application.Handlers.TaxTypes;

public class CreateTaxTypeCommandHandler : ICommandHandler<CreateTaxTypeCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _context;
    private readonly IUserContext _userContext;

    public CreateTaxTypeCommandHandler(ApplicationDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<Result<Guid>> Handle(CreateTaxTypeCommand request, CancellationToken cancellationToken)
    {
        var companyId = _userContext.CompanyId;
        if (companyId == Guid.Empty)
        {
            return Result.Failure<Guid>(new Error("TaxType.Validation", "CompanyId is required."));
        }

        // Check uniqueness
        var existingCode = await _context.TaxTypes
            .AnyAsync(t => t.CompanyId == companyId && t.Code == request.TaxType.Code, cancellationToken);
        if (existingCode)
        {
            return Result.Failure<Guid>(new Error("TaxType.Validation", "A tax type with this code already exists."));
        }

        var existingName = await _context.TaxTypes
            .AnyAsync(t => t.CompanyId == companyId && t.Name == request.TaxType.Name, cancellationToken);
        if (existingName)
        {
            return Result.Failure<Guid>(new Error("TaxType.Validation", "A tax type with this name already exists."));
        }

        var taxType = new TaxType
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = request.TaxType.Code,
            Name = request.TaxType.Name,
            Description = request.TaxType.Description,
            Value = request.TaxType.Value,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.TaxTypes.Add(taxType);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(taxType.Id);
    }
}

public class CreateTaxTypeValidator : AbstractValidator<CreateTaxTypeCommand>
{
    public CreateTaxTypeValidator()
    {
        RuleFor(x => x.TaxType.Code).NotEmpty().MaximumLength(10);
        RuleFor(x => x.TaxType.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TaxType.Value).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxType.Description).MaximumLength(255);
    }
}
