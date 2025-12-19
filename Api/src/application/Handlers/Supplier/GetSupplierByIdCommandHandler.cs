using GesFer.Application.Commands.Supplier;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Supplier;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Supplier;

public class GetSupplierByIdCommandHandler : ICommandHandler<GetSupplierByIdCommand, SupplierDto?>
{
    private readonly ApplicationDbContext _context;

    public GetSupplierByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto?> HandleAsync(GetSupplierByIdCommand command, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .Where(s => s.Id == command.Id && s.DeletedAt == null)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                Name = s.Name,
                TaxId = s.TaxId,
                Address = s.Address,
                Phone = s.Phone,
                Email = s.Email,
                BuyTariffId = s.BuyTariffId,
                PostalCodeId = s.PostalCodeId,
                CityId = s.CityId,
                StateId = s.StateId,
                CountryId = s.CountryId,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return supplier;
    }
}

