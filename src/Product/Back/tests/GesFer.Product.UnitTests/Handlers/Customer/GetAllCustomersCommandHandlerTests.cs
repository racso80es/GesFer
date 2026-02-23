using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class GetAllCustomersCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly GetAllCustomersCommandHandler _handler;

    public GetAllCustomersCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new GetAllCustomersCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnActiveCustomersOnly_ForCompany()
    {
        // Arrange
        var companyId1 = Guid.NewGuid();
        var companyId2 = Guid.NewGuid();

        var customer1 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId1,
            Name = "Customer A (Company 1)",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var customer2 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId1,
            Name = "Customer B (Company 1)",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var customer3 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId2,
            Name = "Customer C (Company 2)",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var deletedCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId1,
            Name = "Deleted Customer",
            DeletedAt = DateTime.UtcNow,
            IsActive = false
        };

        _context.Customers.AddRange(customer1, customer2, customer3, deletedCustomer);
        await _context.SaveChangesAsync();

        var command = new GetAllCustomersCommand(companyId1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Customer A (Company 1)");
        result.Should().Contain(c => c.Name == "Customer B (Company 1)");
        result.Should().NotContain(c => c.Name == "Customer C (Company 2)");
        result.Should().NotContain(c => c.Name == "Deleted Customer");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoCustomersExist()
    {
        // Arrange
        var command = new GetAllCustomersCommand(Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllCustomers_WhenCompanyIdIsNull()
    {
        // Arrange
         var companyId1 = Guid.NewGuid();
        var companyId2 = Guid.NewGuid();

        var customer1 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId1,
            Name = "Customer A",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var customer2 = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId2,
            Name = "Customer B",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Customers.AddRange(customer1, customer2);
        await _context.SaveChangesAsync();

        var command = new GetAllCustomersCommand(null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Customer A");
        result.Should().Contain(c => c.Name == "Customer B");
    }
}
