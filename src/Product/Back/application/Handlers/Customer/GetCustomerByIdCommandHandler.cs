using GesFer.Application.Commands.Customer;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Customer;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Customer;

public class GetCustomerByIdCommandHandler : ICommandHandler<GetCustomerByIdCommand, CustomerDto?>
{
    private readonly ApplicationDbContext _context;

    public GetCustomerByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto?> HandleAsync(GetCustomerByIdCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == command.Id && c.DeletedAt == null)
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
            .FirstOrDefaultAsync(cancellationToken);

        return customer;
    }
}

