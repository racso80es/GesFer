using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class DeleteCustomerCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public DeleteCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerExists_ShouldSoftDeleteCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "To Delete",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var handler = new DeleteCustomerCommandHandler(_context);
        var command = new DeleteCustomerCommand(customerId);

        // Act
        await handler.HandleAsync(command);

        // Assert
        var customerInDb = await _context.Customers.FindAsync(customerId);
        customerInDb.Should().NotBeNull();
        customerInDb!.DeletedAt.Should().NotBeNull();
        customerInDb.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var handler = new DeleteCustomerCommandHandler(_context);
        var command = new DeleteCustomerCommand(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el cliente*");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerIsAlreadyDeleted_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = Guid.NewGuid(),
            Name = "Already Deleted",
            DeletedAt = DateTime.UtcNow
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var handler = new DeleteCustomerCommandHandler(_context);
        var command = new DeleteCustomerCommand(customerId);

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el cliente*");
    }
}
