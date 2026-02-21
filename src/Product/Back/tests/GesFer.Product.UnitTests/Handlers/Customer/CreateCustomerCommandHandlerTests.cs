using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class CreateCustomerCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAdminApiClient> _adminApiMock;

    public CreateCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _adminApiMock = new Mock<IAdminApiClient>();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var tariffId = Guid.NewGuid();
        var postalCodeId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var stateId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        _adminApiMock
            .Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        // Seed references
        _context.Tariffs.Add(new Tariff { Id = tariffId, CompanyId = companyId, Name = "Tariff 1", IsActive = true });
        _context.Countries.Add(new Country { Id = countryId, Name = "Test Country", Code = "TC" });
        _context.States.Add(new State { Id = stateId, Name = "Test State", CountryId = countryId });
        _context.Cities.Add(new City { Id = cityId, Name = "Test City", StateId = stateId });
        _context.PostalCodes.Add(new PostalCode { Id = postalCodeId, Code = "12345", CityId = cityId });

        await _context.SaveChangesAsync();

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);

        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "New Customer",
            TaxId = "A08001851", // Valid ES TaxId (using memory recommendation)
            Address = "Test Address",
            Phone = "123456789",
            Email = "customer@example.com",
            SellTariffId = tariffId,
            PostalCodeId = postalCodeId,
            CityId = cityId,
            StateId = stateId,
            CountryId = countryId
        });

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Customer");
        result.TaxId.Should().Be("A08001851");
        result.Email.Should().Be("customer@example.com");

        var customerInDb = await _context.Customers.FirstOrDefaultAsync(c => c.Name == "New Customer");
        customerInDb.Should().NotBeNull();
        customerInDb!.CompanyId.Should().Be(companyId);
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyDoesNotExist_ShouldThrow()
    {
        // Arrange
        _adminApiMock.Setup(x => x.GetCompanyAsync(It.IsAny<Guid>())).ReturnsAsync((AdminCompanyDto?)null);

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto { CompanyId = Guid.NewGuid(), Name = "Customer" });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa*");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNameExistsInCompany_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId)).ReturnsAsync(new AdminCompanyDto { Id = companyId });

        _context.Customers.Add(new GesFer.Product.Back.Domain.Entities.Customer
        {
            CompanyId = companyId,
            Name = "Existing Customer",
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Existing Customer"
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe un cliente con el nombre*");
    }

    [Fact]
    public async Task HandleAsync_WhenTariffDoesNotExist_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId)).ReturnsAsync(new AdminCompanyDto { Id = companyId });

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer",
            SellTariffId = Guid.NewGuid() // Non-existent
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la tarifa de venta*");
    }

     [Fact]
    public async Task HandleAsync_WhenPostalCodeDoesNotExist_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId)).ReturnsAsync(new AdminCompanyDto { Id = companyId });

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer",
            PostalCodeId = Guid.NewGuid() // Non-existent
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el código postal*");
    }

    [Fact]
    public async Task HandleAsync_WhenCityDoesNotExist_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId)).ReturnsAsync(new AdminCompanyDto { Id = companyId });

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer",
            CityId = Guid.NewGuid() // Non-existent
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la ciudad*");
    }

    [Fact]
    public async Task HandleAsync_WhenStateDoesNotExist_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId)).ReturnsAsync(new AdminCompanyDto { Id = companyId });

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer",
            StateId = Guid.NewGuid() // Non-existent
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la provincia*");
    }

    [Fact]
    public async Task HandleAsync_WhenCountryDoesNotExist_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId)).ReturnsAsync(new AdminCompanyDto { Id = companyId });

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer",
            CountryId = Guid.NewGuid() // Non-existent
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el país*");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidTaxId_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId)).ReturnsAsync(new AdminCompanyDto { Id = companyId });

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer",
            TaxId = "INVALID"
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidEmail_ShouldThrow()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId)).ReturnsAsync(new AdminCompanyDto { Id = companyId });

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer",
            Email = "invalid-email"
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
