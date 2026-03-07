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
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Original Customer",
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var handler = new UpdateCustomerCommandHandler(_context);
        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
        {
            Name = "Updated Customer",
            TaxId = "B12345674",
            IsActive = false
        });

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Customer");
        result.TaxId.Should().Be("B12345674");
        result.IsActive.Should().BeFalse();

        var customerInDb = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        customerInDb.Should().NotBeNull();
        customerInDb!.Name.Should().Be("Updated Customer");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var handler = new UpdateCustomerCommandHandler(_context);
        var command = new UpdateCustomerCommand(Guid.NewGuid(), new UpdateCustomerDto
        {
            Name = "Updated Customer"
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el cliente con ID*");
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherCustomerHasSameName_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var existingCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "Existing Customer Name",
            IsActive = true
        };

        var targetCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Target Customer",
            IsActive = true
        };

        _context.Customers.AddRange(existingCustomer, targetCustomer);
        await _context.SaveChangesAsync();

        var handler = new UpdateCustomerCommandHandler(_context);
        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
        {
            Name = "Existing Customer Name"
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe otro cliente con el nombre*");
    }
}
