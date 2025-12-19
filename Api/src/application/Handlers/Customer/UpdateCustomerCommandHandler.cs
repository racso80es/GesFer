using GesFer.Application.Commands.Customer;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Customer;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Customer;

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateCustomerCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> HandleAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.DeletedAt == null, cancellationToken);

        if (customer == null)
            throw new InvalidOperationException($"No se encontró el cliente con ID {command.Id}");

        // Validar que no exista otro cliente con el mismo nombre en la misma empresa (excepto el actual)
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CompanyId == customer.CompanyId && c.Name == command.Dto.Name && c.Id != command.Id && c.DeletedAt == null, cancellationToken);

        if (existingCustomer != null)
            throw new InvalidOperationException($"Ya existe otro cliente con el nombre '{command.Dto.Name}' en esta empresa");

        // Validar tarifa de venta si se proporciona
        if (command.Dto.SellTariffId.HasValue)
        {
            var tariffExists = await _context.Tariffs
                .AnyAsync(t => t.Id == command.Dto.SellTariffId.Value && t.CompanyId == customer.CompanyId && t.DeletedAt == null, cancellationToken);
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

        customer.Name = command.Dto.Name;
        customer.TaxId = command.Dto.TaxId;
        customer.Address = command.Dto.Address;
        customer.Phone = command.Dto.Phone;
        customer.Email = command.Dto.Email;
        customer.SellTariffId = command.Dto.SellTariffId;
        customer.PostalCodeId = command.Dto.PostalCodeId;
        customer.CityId = command.Dto.CityId;
        customer.StateId = command.Dto.StateId;
        customer.CountryId = command.Dto.CountryId;
        customer.IsActive = command.Dto.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

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

