using FluentAssertions;
using GesFer.Application.Commands.Company;
using GesFer.Application.Handlers.Company;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.Company;

public class DeleteCompanyCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidId_ShouldSoftDeleteCompany()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var company = new GesFer.Product.Back.Domain.Entities.Company
        {
            Id = Guid.NewGuid(),
            Name = "Company To Delete",
            IsActive = true
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var handler = new DeleteCompanyCommandHandler(context);
        var command = new DeleteCompanyCommand(company.Id);

        // Act
        await handler.HandleAsync(command);

        // Assert
        var deletedCompany = await context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == company.Id);

        deletedCompany.Should().NotBeNull();
        deletedCompany!.DeletedAt.Should().NotBeNull();
        deletedCompany.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var handler = new DeleteCompanyCommandHandler(context);
        var command = new DeleteCompanyCommand(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));
    }
}
