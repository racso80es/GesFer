using GesFer.Product.Application.Commands.TaxTypes;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.Application.Handlers.TaxTypes;

public class DeleteTaxTypeCommandHandler : ICommandHandler<DeleteTaxTypeCommand, Result>
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
            return Result.Failure(new Error("TaxType.NotFound", "Tax type not found."));
        }

        // Soft delete
        taxType.DeletedAt = DateTime.UtcNow;
        taxType.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
