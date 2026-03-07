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
    public async Task HandleAsync_WhenCustomerExists_ShouldReturnCustomerDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Existing Customer",
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
    public async Task HandleAsync_WhenCustomerIsSoftDeleted_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Deleted Customer",
            IsActive = false,
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
