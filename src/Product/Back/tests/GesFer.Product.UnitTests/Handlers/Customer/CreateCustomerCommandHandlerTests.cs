using FluentAssertions;
using GesFer.Application.Commands.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Application.Handlers.Customer;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

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
        _adminApiMock
            .Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);

        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = "Test Customer",
            TaxId = "B12345674",
            Address = "Test Address",
            Phone = "123456789",
            Email = "customer@example.com"
        });

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Customer");
        result.CompanyId.Should().Be(companyId);
        result.TaxId.Should().Be("B12345674");
        result.Email.Should().Be("customer@example.com");

        var customerInDb = await _context.Customers.FirstOrDefaultAsync(c => c.Name == "Test Customer");
        customerInDb.Should().NotBeNull();
        customerInDb!.CompanyId.Should().Be(companyId);
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyDoesNotExist_ShouldThrowException()
    {
        // Arrange
        _adminApiMock.Setup(x => x.GetCompanyAsync(It.IsAny<Guid>())).ReturnsAsync((AdminCompanyDto?)null);

        var handler = new CreateCustomerCommandHandler(_context, _adminApiMock.Object);

        var command = new CreateCustomerCommand(new CreateCustomerDto
        {
            CompanyId = Guid.NewGuid(),
            Name = "Test Customer"
        });

        // Act
        Func<Task> act = async () => await handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa con ID*");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNameAlreadyExists_ShouldThrowException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _adminApiMock
            .Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var existingCustomer = new GesFer.Product.Back.Domain.Entities.Customer
        {
            CompanyId = companyId,
            Name = "Existing Customer",
            IsActive = true
        };
        _context.Customers.Add(existingCustomer);
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
}
