using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class GetAllCustomersCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly GetAllCustomersCommandHandler _handler;

    public GetAllCustomersCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _mockContext = new Mock<ApplicationDbContext>(options);
        _handler = new GetAllCustomersCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllCustomers_WhenCompanyIdNotProvided()
    {
        // Arrange
        var customer1 = new GesFer.Product.Back.Domain.Entities.Customer { Id = Guid.NewGuid(), CompanyId = Guid.NewGuid(), Name = "A Customer" };
        var customer2 = new GesFer.Product.Back.Domain.Entities.Customer { Id = Guid.NewGuid(), CompanyId = Guid.NewGuid(), Name = "B Customer" };

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer> { customer1, customer2 }.BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        var command = new GetAllCustomersCommand(null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCompanyCustomers_WhenCompanyIdProvided()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customer1 = new GesFer.Product.Back.Domain.Entities.Customer { Id = Guid.NewGuid(), CompanyId = companyId, Name = "A Customer" };
        var customer2 = new GesFer.Product.Back.Domain.Entities.Customer { Id = Guid.NewGuid(), CompanyId = Guid.NewGuid(), Name = "B Customer" };

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer> { customer1, customer2 }.BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        var command = new GetAllCustomersCommand(companyId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("A Customer");
    }
}
