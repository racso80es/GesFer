using FluentAssertions;
using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Application.Handlers.Company;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

using CompanyEntity = GesFer.Shared.Back.Domain.Entities.Company;

namespace GesFer.Admin.UnitTests.Handlers.Company;

public class DeleteCompanyHandlerTests
{
    private static AdminDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AdminDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidId_SoftDeletesCompany()
    {
        // Arrange
        await using var context = CreateContext();
        var companyId = Guid.NewGuid();
        context.Companies.Add(new CompanyEntity
        {
            Id = companyId,
            Name = "Empresa a Borrar",
            Address = "Calle 1",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new DeleteCompanyHandler(context);
        var command = new DeleteCompanyCommand(companyId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedCompany = await context.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == companyId);
        deletedCompany.Should().NotBeNull();
        deletedCompany!.DeletedAt.Should().NotBeNull();
        deletedCompany.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithInvalidId_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var context = CreateContext();
        var handler = new DeleteCompanyHandler(context);
        var command = new DeleteCompanyCommand(Guid.NewGuid());

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa*");
    }

    [Fact]
    public async Task Handle_WithAlreadyDeletedCompany_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var context = CreateContext();
        var companyId = Guid.NewGuid();
        context.Companies.Add(new CompanyEntity
        {
            Id = companyId,
            Name = "Empresa Borrada",
            Address = "Calle 1",
            DeletedAt = DateTime.UtcNow,
            IsActive = false
        });
        await context.SaveChangesAsync();

        var handler = new DeleteCompanyHandler(context);
        var command = new DeleteCompanyCommand(companyId);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa*");
    }
}
