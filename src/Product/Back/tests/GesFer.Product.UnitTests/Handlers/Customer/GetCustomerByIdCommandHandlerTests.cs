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

public class GetCustomerByIdCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly GetCustomerByIdCommandHandler _handler;

    public GetCustomerByIdCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new GetCustomerByIdCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCustomerDto_WhenFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Found Customer",
            TaxId = TaxId.Create("B87210134"),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new GetCustomerByIdCommand(customerId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.Name.Should().Be("Found Customer");
        result.TaxId.Should().Be("B87210134");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var command = new GetCustomerByIdCommand(Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenCustomerIsDeleted()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = Guid.NewGuid(),
            Name = "Deleted Customer",
            DeletedAt = DateTime.UtcNow,
            IsActive = false
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new GetCustomerByIdCommand(customerId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }
}
