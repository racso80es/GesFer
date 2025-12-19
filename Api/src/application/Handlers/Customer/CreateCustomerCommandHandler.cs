using GesFer.Application.Commands.Customer;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Customer;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Customer;

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ApplicationDbContext _context;

    public CreateCustomerCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        // Validar que la empresa existe
        var companyExists = await _context.Companies
            .AnyAsync(c => c.Id == command.Dto.CompanyId && c.DeletedAt == null, cancellationToken);
        if (!companyExists)
            throw new InvalidOperationException($"No se encontró la empresa con ID {command.Dto.CompanyId}");

        // Validar que no exista un cliente con el mismo nombre en la misma empresa
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CompanyId == command.Dto.CompanyId && c.Name == command.Dto.Name && c.DeletedAt == null, cancellationToken);

        if (existingCustomer != null)
            throw new InvalidOperationException($"Ya existe un cliente con el nombre '{command.Dto.Name}' en esta empresa");

        // Validar tarifa de venta si se proporciona
        if (command.Dto.SellTariffId.HasValue)
        {
            var tariffExists = await _context.Tariffs
                .AnyAsync(t => t.Id == command.Dto.SellTariffId.Value && t.CompanyId == command.Dto.CompanyId && t.DeletedAt == null, cancellationToken);
            if (!tariffExists)
                throw new InvalidOperationException($"No se encontró la tarifa de venta con ID {command.Dto.SellTariffId.Value}");
        }

        // Validar IDs de dirección si se proporcionan
        if (command.Dto.PostalCodeId.HasValue)
        {
            var postalCodeExists = await _context.PostalCodes
                .AnyAsync(pc => pc.Id == command.Dto.PostalCodeId.Value && pc.DeletedAt == null, cancellationToken);
            if (!postalCodeExists)
                throw new InvalidOperationException($"No se encontró el código postal con ID {command.Dto.PostalCodeId.Value}");
        }

        if (command.Dto.CityId.HasValue)
        {
            var cityExists = await _context.Cities
                .AnyAsync(c => c.Id == command.Dto.CityId.Value && c.DeletedAt == null, cancellationToken);
            if (!cityExists)
                throw new InvalidOperationException($"No se encontró la ciudad con ID {command.Dto.CityId.Value}");
        }

        if (command.Dto.StateId.HasValue)
        {
            var stateExists = await _context.States
                .AnyAsync(s => s.Id == command.Dto.StateId.Value && s.DeletedAt == null, cancellationToken);
            if (!stateExists)
                throw new InvalidOperationException($"No se encontró la provincia con ID {command.Dto.StateId.Value}");
        }

        if (command.Dto.CountryId.HasValue)
        {
            var countryExists = await _context.Countries
                .AnyAsync(c => c.Id == command.Dto.CountryId.Value && c.DeletedAt == null, cancellationToken);
            if (!countryExists)
                throw new InvalidOperationException($"No se encontró el país con ID {command.Dto.CountryId.Value}");
        }

        var customer = new Domain.Entities.Customer
        {
            CompanyId = command.Dto.CompanyId,
            Name = command.Dto.Name,
            TaxId = command.Dto.TaxId,
            Address = command.Dto.Address,
            Phone = command.Dto.Phone,
            Email = command.Dto.Email,
            SellTariffId = command.Dto.SellTariffId,
            PostalCodeId = command.Dto.PostalCodeId,
            CityId = command.Dto.CityId,
            StateId = command.Dto.StateId,
            CountryId = command.Dto.CountryId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            Name = customer.Name,
            TaxId = customer.TaxId,
            Address = customer.Address,
            Phone = customer.Phone,
            Email = customer.Email,
            SellTariffId = customer.SellTariffId,
            PostalCodeId = customer.PostalCodeId,
            CityId = customer.CityId,
            StateId = customer.StateId,
            CountryId = customer.CountryId,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}

