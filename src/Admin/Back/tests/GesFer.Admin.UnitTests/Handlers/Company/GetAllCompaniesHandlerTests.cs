using FluentAssertions;
using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Application.Handlers.Company;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

using CompanyEntity = GesFer.Shared.Back.Domain.Entities.Company;

namespace GesFer.Admin.UnitTests.Handlers.Company;

public class GetAllCompaniesHandlerTests
{
    private static AdminDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AdminDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyActiveCompanies()
    {
        // Arrange
        await using var context = CreateContext();
        context.Companies.AddRange(
            new CompanyEntity { Name = "Empresa A", Address = "Calle A", IsActive = true },
            new CompanyEntity { Name = "Empresa B", Address = "Calle B", IsActive = false, DeletedAt = DateTime.UtcNow },
            new CompanyEntity { Name = "Empresa C", Address = "Calle C", IsActive = true }
        );
        await context.SaveChangesAsync();

        var handler = new GetAllCompaniesHandler(context);

        // Act
        var result = await handler.Handle(new GetAllCompaniesCommand(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().Contain(new[] { "Empresa A", "Empresa C" });
        result.Select(c => c.Name).Should().NotContain("Empresa B");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoActiveCompanies()
    {
        // Arrange
        await using var context = CreateContext();
        context.Companies.AddRange(
            new CompanyEntity { Name = "Empresa Borrada 1", Address = "Calle 1", DeletedAt = DateTime.UtcNow },
            new CompanyEntity { Name = "Empresa Borrada 2", Address = "Calle 2", DeletedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var handler = new GetAllCompaniesHandler(context);

        // Act
        var result = await handler.Handle(new GetAllCompaniesCommand(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
