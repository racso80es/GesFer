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

public class GetCustomerByIdCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly GetCustomerByIdCommandHandler _handler;

    public GetCustomerByIdCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _mockContext = new Mock<ApplicationDbContext>(options);
        _handler = new GetCustomerByIdCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCustomer_WhenFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var customer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Customer",
            IsActive = true,
            DeletedAt = null
        };

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer> { customer }.BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        var command = new GetCustomerByIdCommand(customerId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.Name.Should().Be("Customer");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer>().BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        var command = new GetCustomerByIdCommand(Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }
}
