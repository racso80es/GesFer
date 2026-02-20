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
    private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private void AddCommonDependencies(ApplicationDbContext context, Guid companyId, Guid? tariffId = null, Guid? postalCodeId = null)
    {
        if (tariffId.HasValue)
        {
            context.Tariffs.Add(new Tariff
            {
                Id = tariffId.Value,
                CompanyId = companyId,
                Name = "Standard Tariff",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (postalCodeId.HasValue)
        {
            var cityId = Guid.NewGuid();
            context.Cities.Add(new City { Id = cityId, Name = "Test City", StateId = Guid.NewGuid() });

            context.PostalCodes.Add(new PostalCode
            {
                Id = postalCodeId.Value,
                Code = "28001",
                CityId = cityId
            });
        }

        context.SaveChanges();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var tariffId = Guid.NewGuid();
        var postalCodeId = Guid.NewGuid();

        var adminMock = new Mock<IAdminApiClient>();
        adminMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        AddCommonDependencies(context, companyId, tariffId, postalCodeId);

        var handler = new CreateCustomerCommandHandler(context, adminMock.Object);

        var dto = new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "New Customer",
            TaxId = "A08001851",
            Email = "customer@example.com",
            Phone = "123456789",
            SellTariffId = tariffId,
            PostalCodeId = postalCodeId,
            Address = "Test Address"
        };
        var command = new CreateCustomerCommand(dto);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Customer");
        result.CompanyId.Should().Be(companyId);
        result.TaxId.Should().Be("A08001851");
        result.Email.Should().Be("customer@example.com");
        result.SellTariffId.Should().Be(tariffId);
        result.PostalCodeId.Should().Be(postalCodeId);

        var dbCustomer = await context.Customers.FirstOrDefaultAsync(c => c.Id == result.Id);
        dbCustomer.Should().NotBeNull();
        dbCustomer!.Name.Should().Be("New Customer");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCompany_ShouldThrowException()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var adminMock = new Mock<IAdminApiClient>();
        adminMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync((AdminCompanyDto?)null);

        var handler = new CreateCustomerCommandHandler(context, adminMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto { CompanyId = companyId, Name = "Test" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        exception.Message.Should().Contain("No se encontró la empresa");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var adminMock = new Mock<IAdminApiClient>();
        adminMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        context.Customers.Add(new GesFer.Product.Back.Domain.Entities.Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "Existing Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new CreateCustomerCommandHandler(context, adminMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto { CompanyId = companyId, Name = "Existing Customer" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        exception.Message.Should().Contain("Ya existe un cliente con el nombre");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidTariff_ShouldThrowException()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var adminMock = new Mock<IAdminApiClient>();
        adminMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var handler = new CreateCustomerCommandHandler(context, adminMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "New Customer",
            SellTariffId = Guid.NewGuid()
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        exception.Message.Should().Contain("No se encontró la tarifa de venta");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPostalCode_ShouldThrowException()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var adminMock = new Mock<IAdminApiClient>();
        adminMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var handler = new CreateCustomerCommandHandler(context, adminMock.Object);
        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "New Customer",
            PostalCodeId = Guid.NewGuid()
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        exception.Message.Should().Contain("No se encontró el código postal");
    }
}
