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

public class DeleteCustomerCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly DeleteCustomerCommandHandler _handler;

    public DeleteCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _mockContext = new Mock<ApplicationDbContext>(options);
        _handler = new DeleteCustomerCommandHandler(_mockContext.Object);
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
            Name = "To Delete",
            IsActive = true,
            DeletedAt = null
        };

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer> { customer }.BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        var command = new DeleteCustomerCommand(customerId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        customer.DeletedAt.Should().NotBeNull();
        customer.IsActive.Should().BeFalse();

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCustomerNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer>().BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        var command = new DeleteCustomerCommand(customerId);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró el cliente con ID {customerId}*");
    }
}
