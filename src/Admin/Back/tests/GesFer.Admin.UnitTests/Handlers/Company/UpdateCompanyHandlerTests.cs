using FluentAssertions;
using GesFer.Admin.Application.DTOs.Company;
using GesFer.Admin.Application.Handlers.Company;
using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

using CompanyEntity = GesFer.Shared.Back.Domain.Entities.Company;

namespace GesFer.Admin.UnitTests.Handlers.Company;

public class UpdateCompanyHandlerTests
{
    private static AdminDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AdminDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesCompanyAndReturnsDto()
    {
        await using var context = CreateContext();
        var companyId = Guid.NewGuid();
        context.Companies.Add(new CompanyEntity
        {
            Id = companyId,
            Name = "Nombre Antiguo",
            Address = "Calle Vieja 1"
        });
        await context.SaveChangesAsync();

        var handler = new UpdateCompanyHandler(context);
        var dto = new UpdateCompanyDto
        {
            Name = "Nombre Actualizado",
            TaxId = "B87654323",
            Address = "Calle Nueva 2",
            Phone = "987654321",
            Email = "nuevo@empresa.com",
            IsActive = true
        };

        var result = await handler.Handle(new UpdateCompanyCommand(companyId, dto), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(companyId);
        result.Name.Should().Be("Nombre Actualizado");
        result.Address.Should().Be("Calle Nueva 2");
        var saved = await context.Companies.FirstAsync(c => c.Id == companyId);
        saved.Name.Should().Be("Nombre Actualizado");
        saved.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsInvalidOperationException()
    {
        await using var context = CreateContext();
        var handler = new UpdateCompanyHandler(context);
        var dto = new UpdateCompanyDto
        {
            Name = "Cualquiera",
            Address = "Calle 1",
            IsActive = true
        };

        var act = () => handler.Handle(new UpdateCompanyCommand(Guid.NewGuid(), dto), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa*");
    }

    [Fact]
    public async Task Handle_WithDuplicateNameFromOtherCompany_ThrowsInvalidOperationException()
    {
        await using var context = CreateContext();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        context.Companies.AddRange(
            new CompanyEntity { Id = id1, Name = "Empresa A", Address = "Calle 1" },
            new CompanyEntity { Id = id2, Name = "Empresa B", Address = "Calle 2" }
        );
        await context.SaveChangesAsync();

        var handler = new UpdateCompanyHandler(context);
        var dto = new UpdateCompanyDto
        {
            Name = "Empresa B",
            Address = "Calle 1",
            IsActive = true
        };

        var act = () => handler.Handle(new UpdateCompanyCommand(id1, dto), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe otra empresa con el nombre*");
    }
}
