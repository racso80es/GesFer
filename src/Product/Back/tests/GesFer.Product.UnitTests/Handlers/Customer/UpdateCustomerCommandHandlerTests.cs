using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

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
    public async Task HandleAsync_WithValidData_ShouldUpdateCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var originalName = "Original Name";

        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = originalName,
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
        {
            Name = "Updated Name",
            IsActive = true
        });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.Name.Should().Be("Updated Name");

        var updatedCustomer = await _context.Customers.FindAsync(customerId);
        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
        {
            Name = "Ghost Customer"
        });

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró el cliente con ID {customerId}*");
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsDuplicateInSameCompany_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();

        _context.Customers.AddRange(
            new GesFer.Product.Back.Domain.Entities.Customer { Id = customer1Id, CompanyId = companyId, Name = "Customer One", IsActive = true },
            new GesFer.Product.Back.Domain.Entities.Customer { Id = customer2Id, CompanyId = companyId, Name = "Customer Two", IsActive = true }
        );
        await _context.SaveChangesAsync();

        var command = new UpdateCustomerCommand(customer1Id, new UpdateCustomerDto
        {
            Name = "Customer Two", // Trying to rename 1 to 2's name
            IsActive = true
        });

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Ya existe otro cliente con el nombre 'Customer Two' en esta empresa*");
    }

    [Fact]
    public async Task HandleAsync_WhenSellTariffIdInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var invalidTariffId = Guid.NewGuid();

        _context.Customers.Add(new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Test Customer",
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
        {
            Name = "Test Customer",
            SellTariffId = invalidTariffId,
            IsActive = true
        });

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la tarifa de venta con ID {invalidTariffId}*");
    }
}
