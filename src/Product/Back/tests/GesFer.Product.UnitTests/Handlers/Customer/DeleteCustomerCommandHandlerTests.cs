using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class DeleteCustomerCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly DeleteCustomerCommandHandler _handler;

    public DeleteCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new DeleteCustomerCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldSoftDeleteCustomer_WhenFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = Guid.NewGuid(),
            Name = "Customer to Delete",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new DeleteCustomerCommand(customerId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var deleted = await _context.Customers.FindAsync(customerId);
        deleted.Should().NotBeNull();
        deleted!.DeletedAt.Should().NotBeNull();
        deleted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCustomerNotFound()
    {
        // Arrange
        var command = new DeleteCustomerCommand(Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el cliente*");
    }
}
