using FluentAssertions;
using GesFer.Application.Commands.Company;
using GesFer.Application.DTOs.Company;
using GesFer.Application.Handlers.Company;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.Company;

/// <summary>
/// Tests unitarios para CreateCompanyCommandHandler usando In-Memory Database
/// </summary>
public class CreateCompanyCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateCompany()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var handler = new CreateCompanyCommandHandler(context);

        var createDto = new CreateCompanyDto
        {
            Name = "Test Company",
            TaxId = "B12345678",
            Address = "Test Address 123",
            Phone = "+34 123 456 789",
            Email = "test@example.com"
        };

        var command = new CreateCompanyCommand(createDto);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Company");
        result.TaxId.Should().Be("B12345678");
        result.Address.Should().Be("Test Address 123");
        result.Phone.Should().Be("+34 123 456 789");
        result.Email.Should().Be("test@example.com");
        result.Id.Should().NotBeEmpty();
        result.IsActive.Should().BeTrue();

        // Verificar que se guardó en la base de datos
        var savedCompany = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == result.Id && c.DeletedAt == null);
        savedCompany.Should().NotBeNull();
        savedCompany!.Name.Should().Be("Test Company");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        // Crear una empresa existente
        var existingCompany = new Company
        {
            Name = "Existing Company",
            Address = "Existing Address",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Companies.Add(existingCompany);
        await context.SaveChangesAsync();

        var handler = new CreateCompanyCommandHandler(context);

        var createDto = new CreateCompanyDto
        {
            Name = "Existing Company", // Nombre duplicado
            Address = "New Address"
        };

        var command = new CreateCompanyCommand(createDto);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var handler = new CreateCompanyCommandHandler(context);

        var createDto = new CreateCompanyDto
        {
            Name = "Test Company",
            Address = "Test Address",
            Email = "invalid-email" // Email inválido
        };

        var command = new CreateCompanyCommand(createDto);

        // Act & Assert
        // Email.Create() debería lanzar una excepción para un email inválido
        await Assert.ThrowsAsync<Exception>(
            async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithValidEmail_ShouldCreateCompanyWithEmail()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var handler = new CreateCompanyCommandHandler(context);

        var createDto = new CreateCompanyDto
        {
            Name = "Test Company",
            Address = "Test Address",
            Email = "valid@example.com"
        };

        var command = new CreateCompanyCommand(createDto);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("valid@example.com");
    }
}
