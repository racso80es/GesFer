using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class CreateCustomerCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAdminApiClient> _adminApiMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _adminApiMock = new Mock<IAdminApiClient>();
        _handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "New Customer",
            Email = "customer@example.com",
            Phone = "123456789",
            Address = "Test Address"
        });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Customer");
        result.CompanyId.Should().Be(companyId);
        result.Email.Should().Be("customer@example.com");

        var customerInDb = await _context.Customers.FirstOrDefaultAsync(c => c.Id == result.Id);
        customerInDb.Should().NotBeNull();
        customerInDb!.Name.Should().Be("New Customer");
    }

    [Fact]
    public async Task HandleAsync_WithAllOptionalFields_ShouldCreateCustomer()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var tariffId = Guid.NewGuid();
        var postalCodeId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var stateId = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        // Seed dependencies
        _context.Tariffs.Add(new Tariff { Id = tariffId, CompanyId = companyId, Name = "Standard", IsActive = true });
        _context.PostalCodes.Add(new GesFer.Shared.Back.Domain.Entities.PostalCode { Id = postalCodeId, Code = "12345", CityId = cityId });
        _context.Cities.Add(new GesFer.Shared.Back.Domain.Entities.City { Id = cityId, Name = "Test City", StateId = stateId });
        _context.States.Add(new GesFer.Shared.Back.Domain.Entities.State { Id = stateId, Name = "Test State", CountryId = countryId });
        _context.Countries.Add(new GesFer.Shared.Back.Domain.Entities.Country { Id = countryId, Name = "Test Country", Code = "TC" });
        await _context.SaveChangesAsync();

        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Full Customer",
            SellTariffId = tariffId,
            PostalCodeId = postalCodeId,
            CityId = cityId,
            StateId = stateId,
            CountryId = countryId,
            TaxId = "A08001851" // Valid TaxId based on memory
        });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.SellTariffId.Should().Be(tariffId);
        result.PostalCodeId.Should().Be(postalCodeId);
        result.CityId.Should().Be(cityId);
        result.StateId.Should().Be(stateId);
        result.CountryId.Should().Be(countryId);
        result.TaxId.Should().Be("A08001851");
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync((AdminCompanyDto?)null);

        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Orphan Customer"
        });

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la empresa con ID {companyId}*");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNameDuplicateInCompany_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        _context.Customers.Add(new GesFer.Product.Back.Domain.Entities.Customer
        {
            CompanyId = companyId,
            Name = "Existing Customer",
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Existing Customer"
        });

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Ya existe un cliente con el nombre 'Existing Customer' en esta empresa*");
    }

    [Fact]
    public async Task HandleAsync_WhenSellTariffIdInvalid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var invalidTariffId = Guid.NewGuid();

        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Customer with Invalid Tariff",
            SellTariffId = invalidTariffId
        });

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la tarifa de venta con ID {invalidTariffId}*");
    }
}
