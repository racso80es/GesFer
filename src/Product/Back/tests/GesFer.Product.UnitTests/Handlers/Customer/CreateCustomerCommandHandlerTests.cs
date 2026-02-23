using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using GesFer.Shared.Back.Domain.ValueObjects;
using GesFer.Product.Back.Infrastructure.DTOs;

namespace GesFer.Product.UnitTests.Handlers.Customer;

public class CreateCustomerCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAdminApiClient> _mockAdminApiClient;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _mockAdminApiClient = new Mock<IAdminApiClient>();
        _handler = new CreateCustomerCommandHandler(_context, _mockAdminApiClient.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateCustomer_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var companyDto = new AdminCompanyDto { Id = companyId, Name = "Test Company" };
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(companyDto);

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "New Customer",
                TaxId = "B87210134", // Valid CIF
                Email = "customer@example.com",
                Address = "Test Address",
                Phone = "123456789"
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("New Customer");
        result.TaxId.Should().Be("B87210134");
        result.Email.Should().Be("customer@example.com");

        var created = await _context.Customers.FindAsync(result.Id);
        created.Should().NotBeNull();
        created!.CompanyId.Should().Be(companyId);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCompanyNotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync((AdminCompanyDto?)null);

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "Test"
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la empresa con ID {companyId}*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenNameAlreadyExistsInCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var companyDto = new AdminCompanyDto { Id = companyId, Name = "Test Company" };
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(companyDto);

        _context.Customers.Add(new GesFer.Product.Back.Domain.Entities.Customer
        {
            CompanyId = companyId,
            Name = "Existing Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "Existing Customer"
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Ya existe un cliente con el nombre 'Existing Customer' en esta empresa*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenSellTariffNotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var tariffId = Guid.NewGuid();
        var companyDto = new AdminCompanyDto { Id = companyId, Name = "Test Company" };
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(companyDto);

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "Customer",
                SellTariffId = tariffId
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la tarifa de venta con ID {tariffId}*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenPostalCodeNotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var postalCodeId = Guid.NewGuid();
        var companyDto = new AdminCompanyDto { Id = companyId, Name = "Test Company" };
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(companyDto);

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "Customer",
                PostalCodeId = postalCodeId
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró el código postal con ID {postalCodeId}*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCityNotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var companyDto = new AdminCompanyDto { Id = companyId, Name = "Test Company" };
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(companyDto);

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "Customer",
                CityId = cityId
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la ciudad con ID {cityId}*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenStateNotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var stateId = Guid.NewGuid();
        var companyDto = new AdminCompanyDto { Id = companyId, Name = "Test Company" };
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(companyDto);

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "Customer",
                StateId = stateId
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró la provincia con ID {stateId}*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCountryNotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var companyDto = new AdminCompanyDto { Id = companyId, Name = "Test Company" };
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(companyDto);

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "Customer",
                CountryId = countryId
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró el país con ID {countryId}*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenTaxIdIsInvalid()
    {
         // Arrange
        var companyId = Guid.NewGuid();
        var companyDto = new AdminCompanyDto { Id = companyId, Name = "Test Company" };
        _mockAdminApiClient.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(companyDto);

        var command = new CreateCustomerCommand(
            new CreateCustomerDto
            {
                CompanyId = companyId,
                Name = "Customer",
                TaxId = "INVALID"
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*formato del identificador fiscal*"); // Matching part of the exception message from TaxId.Create
    }
}
