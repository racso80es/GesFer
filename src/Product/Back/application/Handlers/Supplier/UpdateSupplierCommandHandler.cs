using GesFer.Application.Commands.Supplier;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Supplier;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.Supplier;

public class UpdateSupplierCommandHandler : ICommandHandler<UpdateSupplierCommand, SupplierDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateSupplierCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto> HandleAsync(UpdateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.DeletedAt == null, cancellationToken);

        if (supplier == null)
            throw new InvalidOperationException($"No se encontró el proveedor con ID {command.Id}");

        // Validar que no exista otro proveedor con el mismo nombre en la misma empresa (excepto el actual)
        var existingSupplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.CompanyId == supplier.CompanyId && s.Name == command.Dto.Name && s.Id != command.Id && s.DeletedAt == null, cancellationToken);

        if (existingSupplier != null)
            throw new InvalidOperationException($"Ya existe otro proveedor con el nombre '{command.Dto.Name}' en esta empresa");

        // Validar tarifa de compra si se proporciona
        if (command.Dto.BuyTariffId.HasValue)
        {
            var tariffExists = await _context.Tariffs
                .AnyAsync(t => t.Id == command.Dto.BuyTariffId.Value && t.CompanyId == supplier.CompanyId && t.DeletedAt == null, cancellationToken);
            if (!tariffExists)
                throw new InvalidOperationException($"No se encontró la tarifa de compra con ID {command.Dto.BuyTariffId.Value}");
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

        supplier.Name = command.Dto.Name;
        supplier.TaxId = command.Dto.TaxId;
        supplier.Address = command.Dto.Address;
        supplier.Phone = command.Dto.Phone;
        supplier.Email = command.Dto.Email;
        supplier.BuyTariffId = command.Dto.BuyTariffId;
        supplier.PostalCodeId = command.Dto.PostalCodeId;
        supplier.CityId = command.Dto.CityId;
        supplier.StateId = command.Dto.StateId;
        supplier.CountryId = command.Dto.CountryId;
        supplier.IsActive = command.Dto.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new SupplierDto
        {
            Id = supplier.Id,
            CompanyId = supplier.CompanyId,
            Name = supplier.Name,
            TaxId = supplier.TaxId,
            Address = supplier.Address,
            Phone = supplier.Phone,
            Email = supplier.Email,
            BuyTariffId = supplier.BuyTariffId,
            PostalCodeId = supplier.PostalCodeId,
            CityId = supplier.CityId,
            StateId = supplier.StateId,
            CountryId = supplier.CountryId,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt
        };
    }
}

