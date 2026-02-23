using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GesFer.Shared.Back.Domain.ValueObjects;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class UpdateCustomerCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new UpdateCustomerCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateCustomer_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Original Name",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new UpdateCustomerCommand(
            customerId,
            new UpdateCustomerDto
            {
                Name = "Updated Name",
                TaxId = "B87210134",
                Email = "updated@example.com",
                IsActive = false
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.Name.Should().Be("Updated Name");
        result.IsActive.Should().BeFalse();

        var updated = await _context.Customers.FindAsync(customerId);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCustomerNotFound()
    {
        // Arrange
        var command = new UpdateCustomerCommand(
            Guid.NewGuid(),
            new UpdateCustomerDto { Name = "Updated" });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el cliente*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenNameAlreadyExistsForAnotherCustomer()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customer1 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "Customer 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var customer2 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "Customer 2",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        var command = new UpdateCustomerCommand(
            customer1.Id,
            new UpdateCustomerDto
            {
                Name = "Customer 2" // Name of customer 2
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe otro cliente con el nombre*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenReferencedEntitiesNotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new UpdateCustomerCommand(
            customer.Id,
            new UpdateCustomerDto
            {
                Name = "Customer",
                SellTariffId = Guid.NewGuid() // Non-existent
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la tarifa de venta*");
    }
}
