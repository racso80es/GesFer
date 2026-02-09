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

public class UpdateCompanyCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidData_ShouldUpdateCompany()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var company = new GesFer.Product.Back.Domain.Entities.Company
        {
            Id = companyId,
            Name = "Original Name",
            Address = "Original Address",
            IsActive = true
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var handler = new UpdateCompanyCommandHandler(context);

        var updateDto = new UpdateCompanyDto
        {
            Name = "Updated Name",
            Address = "Updated Address",
            TaxId = "B12345674", // Valid TaxId
            Email = "updated@example.com",
            IsActive = true
        };

        var command = new UpdateCompanyCommand(companyId, updateDto);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Address.Should().Be("Updated Address");
        result.Email.Should().Be("updated@example.com");

        var updatedCompany = await context.Companies.FindAsync(companyId);
        updatedCompany.Should().NotBeNull();
        updatedCompany!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentId_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var handler = new UpdateCompanyCommandHandler(context);

        var updateDto = new UpdateCompanyDto
        {
            Name = "New Name"
        };

        var command = new UpdateCompanyCommand(Guid.NewGuid(), updateDto);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var company1 = new GesFer.Product.Back.Domain.Entities.Company { Id = Guid.NewGuid(), Name = "Company One" };
        var company2 = new GesFer.Product.Back.Domain.Entities.Company { Id = Guid.NewGuid(), Name = "Company Two" };

        context.Companies.AddRange(company1, company2);
        await context.SaveChangesAsync();

        var handler = new UpdateCompanyCommandHandler(context);

        // Try to rename Company Two to "Company One"
        var updateDto = new UpdateCompanyDto
        {
            Name = "Company One"
        };

        var command = new UpdateCompanyCommand(company2.Id, updateDto);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));
    }
}
