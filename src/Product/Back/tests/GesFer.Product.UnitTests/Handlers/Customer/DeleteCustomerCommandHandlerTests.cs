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
    public async Task HandleAsync_WithValidId_ShouldSoftDeleteCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Customer to Delete",
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var handler = new DeleteCustomerCommandHandler(_context);
        var command = new DeleteCustomerCommand(customerId);

        // Act
        await handler.HandleAsync(command);

        // Assert
        // In EF Core, if a soft delete is implemented via global query filters, it might be excluded by default
        var customerInDb = await _context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == customerId);
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
            .WithMessage("*No se encontró el cliente con ID*");
    }
}
