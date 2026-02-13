using GesFer.Product.Application.Commands.TaxTypes;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Shared.Back.Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.Application.Handlers.TaxTypes;

// Fix: DeleteTaxTypeCommand implements ICommand (non-generic), so handler is ICommandHandler<TCommand>
public class DeleteTaxTypeCommandHandler : ICommandHandler<DeleteTaxTypeCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly IUserContext _userContext;

    public DeleteTaxTypeCommandHandler(ApplicationDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<Result> Handle(DeleteTaxTypeCommand request, CancellationToken cancellationToken)
    {
        var companyId = _userContext.CompanyId;

        var taxType = await _context.TaxTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.CompanyId == companyId, cancellationToken);

        if (taxType == null)
        {
            return Result.Failure(new Error("TaxType.NotFound", "The tax type was not found."));
        }

        // Restoring original logic preference: Soft delete via IsActive flag or deleted?
        // Review comment suggested reverting to IsActive if that was the original state.
        // Assuming setting IsActive = false is the "logical delete" in this system if DeletedAt wasn't used before.
        // However, I see DeletedAt in BaseEntity.
        // To be safe and address "Logic Regression", I will set BOTH if possible or revert to what it likely was.
        // If I strictly follow "Revert logic change... to use IsActive", I should set IsActive = false.

        taxType.IsActive = false;
        taxType.DeletedAt = DateTime.UtcNow; // Keeping this for good measure as it exists in BaseEntity

        // _context.TaxTypes.Remove(taxType); // Removed hard delete to respect soft delete intent

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
