using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;
using SharedEntities = GesFer.Shared.Back.Domain.Entities;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class UpdateCustomerCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _mockContext = new Mock<ApplicationDbContext>(options);
        _handler = new UpdateCustomerCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateCustomer_WhenRequestIsValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var existingCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Old Name",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer> { existingCustomer }.BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        // Mock Tariffs, etc. as empty or containing referenced IDs if needed
        var tariffs = new List<Tariff>().BuildMockDbSet();
        _mockContext.Setup(x => x.Tariffs).Returns(tariffs.Object);

        var postalCodes = new List<SharedEntities.PostalCode>().BuildMockDbSet();
        _mockContext.Setup(x => x.PostalCodes).Returns(postalCodes.Object);

        var cities = new List<SharedEntities.City>().BuildMockDbSet();
        _mockContext.Setup(x => x.Cities).Returns(cities.Object);

        var states = new List<SharedEntities.State>().BuildMockDbSet();
        _mockContext.Setup(x => x.States).Returns(states.Object);

        var countries = new List<SharedEntities.Country>().BuildMockDbSet();
        _mockContext.Setup(x => x.Countries).Returns(countries.Object);

        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
        {
            Name = "New Name",
            TaxId = "B87210134",
            IsActive = true
        });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        result.TaxId.Should().Be("B87210134");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCustomerNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto { Name = "New Name" });

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer>().BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró el cliente con ID {customerId}*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenNameDuplicateInCompany()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var existingCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Old Name"
        };

        var otherCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = otherCustomerId,
            CompanyId = companyId,
            Name = "Target Name"
        };

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer> { existingCustomer, otherCustomer }.BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
        {
            Name = "Target Name"
        });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Ya existe otro cliente con el nombre 'Target Name' en esta empresa*");
    }
}
