using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class UpdateCustomerCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public UpdateCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldUpdateCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var existingCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Old Name",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.Add(existingCustomer);
        await _context.SaveChangesAsync();

        var handler = new UpdateCustomerCommandHandler(_context);

        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
        {
            Name = "Updated Name",
            IsActive = false
        });

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.Name.Should().Be("Updated Name");
        result.IsActive.Should().BeFalse();

        var customerInDb = await _context.Customers.FindAsync(customerId);
        customerInDb.Should().NotBeNull();
        customerInDb!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var handler = new UpdateCustomerCommandHandler(_context);
        var command = new UpdateCustomerCommand(Guid.NewGuid(), new UpdateCustomerDto
        {
            Name = "New Name"
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el cliente*");
    }

    [Fact]
    public async Task HandleAsync_WhenNewNameExistsForAnotherCustomer_ShouldThrowException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();

        var customer1 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customer1Id,
            CompanyId = companyId,
            Name = "Customer 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var customer2 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customer2Id,
            CompanyId = companyId,
            Name = "Customer 2",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        var handler = new UpdateCustomerCommandHandler(_context);

        var command = new UpdateCustomerCommand(customer1Id, new UpdateCustomerDto
        {
            Name = "Customer 2" // Trying to rename to an existing name
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe otro cliente con el nombre*");
    }
}
