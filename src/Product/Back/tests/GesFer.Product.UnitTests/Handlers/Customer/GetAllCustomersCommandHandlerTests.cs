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
    public async Task HandleAsync_WhenNoCompanyIdProvided_ShouldReturnAllCustomers()
    {
        // Arrange
        var company1Id = Guid.NewGuid();
        var company2Id = Guid.NewGuid();

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer>
        {
            new() { Id = Guid.NewGuid(), CompanyId = company1Id, Name = "Customer A", IsActive = true },
            new() { Id = Guid.NewGuid(), CompanyId = company1Id, Name = "Customer B", IsActive = true },
            new() { Id = Guid.NewGuid(), CompanyId = company2Id, Name = "Customer C", IsActive = true },
            new() { Id = Guid.NewGuid(), CompanyId = company1Id, Name = "Deleted Customer", IsActive = false, DeletedAt = DateTime.UtcNow }
        };

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        var handler = new GetAllCustomersCommandHandler(_context);
        var command = new GetAllCustomersCommand(null);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3); // Should not include deleted
        result.Should().Contain(c => c.Name == "Customer A");
        result.Should().Contain(c => c.Name == "Customer B");
        result.Should().Contain(c => c.Name == "Customer C");
        result.Should().NotContain(c => c.Name == "Deleted Customer");
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyIdProvided_ShouldReturnCustomersFilteredByCompany()
    {
        // Arrange
        var targetCompanyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer>
        {
            new() { Id = Guid.NewGuid(), CompanyId = targetCompanyId, Name = "Customer A", IsActive = true },
            new() { Id = Guid.NewGuid(), CompanyId = targetCompanyId, Name = "Customer B", IsActive = true },
            new() { Id = Guid.NewGuid(), CompanyId = otherCompanyId, Name = "Customer C", IsActive = true }
        };

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        var handler = new GetAllCustomersCommandHandler(_context);
        var command = new GetAllCustomersCommand(targetCompanyId);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(c => c.CompanyId == targetCompanyId).Should().BeTrue();
        result.Should().Contain(c => c.Name == "Customer A");
        result.Should().Contain(c => c.Name == "Customer B");
    }
}
