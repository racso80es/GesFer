using GesFer.Application.Commands.Supplier;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Supplier;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Supplier;

public class GetAllSuppliersCommandHandler : ICommandHandler<GetAllSuppliersCommand, List<SupplierDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllSuppliersCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierDto>> HandleAsync(GetAllSuppliersCommand command, CancellationToken cancellationToken = default)
    {
        var query = _context.Suppliers
            .Where(s => s.DeletedAt == null);

        if (command.CompanyId.HasValue)
        {
            query = query.Where(s => s.CompanyId == command.CompanyId.Value);
        }

        var suppliers = await query
            .OrderBy(s => s.Name)
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
            .ToListAsync(cancellationToken);

        return suppliers;
    }
}

