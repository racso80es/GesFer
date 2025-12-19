using GesFer.Application.Commands.Customer;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Customer;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Customer;

public class GetAllCustomersCommandHandler : ICommandHandler<GetAllCustomersCommand, List<CustomerDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllCustomersCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> HandleAsync(GetAllCustomersCommand command, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers
            .Where(c => c.DeletedAt == null);

        if (command.CompanyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == command.CompanyId.Value);
        }

        var customers = await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                CompanyId = c.CompanyId,
                Name = c.Name,
                TaxId = c.TaxId,
                Address = c.Address,
                Phone = c.Phone,
                Email = c.Email,
                SellTariffId = c.SellTariffId,
                PostalCodeId = c.PostalCodeId,
                CityId = c.CityId,
                StateId = c.StateId,
                CountryId = c.CountryId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return customers;
    }
}

