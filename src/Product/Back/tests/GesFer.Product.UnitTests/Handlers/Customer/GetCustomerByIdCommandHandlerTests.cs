using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class GetCustomerByIdCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public GetCustomerByIdCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerExists_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Existing Customer",
            TaxId = GesFer.Shared.Back.Domain.ValueObjects.TaxId.Create("B87210134"),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var handler = new GetCustomerByIdCommandHandler(_context);
        var command = new GetCustomerByIdCommand(customerId);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.Name.Should().Be("Existing Customer");
        result.TaxId.Should().Be("B87210134");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var handler = new GetCustomerByIdCommandHandler(_context);
        var command = new GetCustomerByIdCommand(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerIsDeleted_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = Guid.NewGuid(),
            Name = "Deleted Customer",
            DeletedAt = DateTime.UtcNow
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var handler = new GetCustomerByIdCommandHandler(_context);
        var command = new GetCustomerByIdCommand(customerId);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }
}
