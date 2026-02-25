using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;
using SharedEntities = GesFer.Shared.Back.Domain.Entities;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IAdminApiClient> _mockAdminApiClient;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // We use Mock behavior Loose by default
        _mockContext = new Mock<ApplicationDbContext>(options);
        _mockAdminApiClient = new Mock<IAdminApiClient>();
        _handler = new CreateCustomerCommandHandler(_mockContext.Object, _mockAdminApiClient.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateCustomer_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "New Customer",
            TaxId = "B87210134", // Valid CIF
            Email = "customer@example.com"
        });

        // Mock Admin Api
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company", CreatedAt = DateTime.UtcNow });

        // Mock DbSets
        // Customers (empty initially)
        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer>().BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        // Tariffs (empty)
        var tariffs = new List<Tariff>().BuildMockDbSet();
        _mockContext.Setup(x => x.Tariffs).Returns(tariffs.Object);

        // PostalCodes (empty)
        var postalCodes = new List<SharedEntities.PostalCode>().BuildMockDbSet();
        _mockContext.Setup(x => x.PostalCodes).Returns(postalCodes.Object);

        // Cities (empty)
        var cities = new List<SharedEntities.City>().BuildMockDbSet();
        _mockContext.Setup(x => x.Cities).Returns(cities.Object);

        // States (empty)
        var states = new List<SharedEntities.State>().BuildMockDbSet();
        _mockContext.Setup(x => x.States).Returns(states.Object);

        // Countries (empty)
        var countries = new List<SharedEntities.Country>().BuildMockDbSet();
        _mockContext.Setup(x => x.Countries).Returns(countries.Object);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Customer");
        result.TaxId.Should().Be("B87210134");

        _mockContext.Verify(x => x.Customers.Add(It.IsAny<GesFer.Product.Back.Domain.Entities.Customer>()), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCompanyDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new CreateCustomerCommand(new CreateCustomerDto { CompanyId = companyId, Name = "Test" });

        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync((AdminCompanyDto?)null);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la empresa con ID {companyId}*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCustomerNameExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new CreateCustomerCommand(new CreateCustomerDto { CompanyId = companyId, Name = "Existing Customer" });

        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId });

        // Setup existing customer in Mock DbSet
        var existingCustomers = new List<GesFer.Product.Back.Domain.Entities.Customer>
        {
            new GesFer.Product.Back.Domain.Entities.Customer
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Name = "Existing Customer",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        }.BuildMockDbSet();

        _mockContext.Setup(x => x.Customers).Returns(existingCustomers.Object);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Ya existe un cliente con el nombre 'Existing Customer'*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenTariffDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var tariffId = Guid.NewGuid();
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer",
            SellTariffId = tariffId
        });

        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId });

        var customers = new List<GesFer.Product.Back.Domain.Entities.Customer>().BuildMockDbSet();
        _mockContext.Setup(x => x.Customers).Returns(customers.Object);

        var tariffs = new List<Tariff>().BuildMockDbSet(); // Empty
        _mockContext.Setup(x => x.Tariffs).Returns(tariffs.Object);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la tarifa de venta con ID {tariffId}*");
    }
}
