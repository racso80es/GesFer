using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class GetAllCustomersCommandHandlerTests
{
    private readonly ApplicationDbContext _context;

    public GetAllCustomersCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllActiveCustomers()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        var customer1 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "A Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var customer2 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "B Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var deletedCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "Deleted Customer",
            DeletedAt = DateTime.UtcNow
        };

        _context.Customers.AddRange(customer1, customer2, deletedCustomer);
        await _context.SaveChangesAsync();

        var handler = new GetAllCustomersCommandHandler(_context);
        var command = new GetAllCustomersCommand();

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().Contain(new[] { "A Customer", "B Customer" });
        result.Select(c => c.Name).Should().NotContain("Deleted Customer");
    }

    [Fact]
    public async Task HandleAsync_WithCompanyId_ShouldFilterByCompany()
    {
        // Arrange
        var company1Id = Guid.NewGuid();
        var company2Id = Guid.NewGuid();

        var customer1 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = company1Id,
            Name = "Company 1 Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var customer2 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = company2Id,
            Name = "Company 2 Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        var handler = new GetAllCustomersCommandHandler(_context);
        var command = new GetAllCustomersCommand { CompanyId = company1Id };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Company 1 Customer");
    }
}
