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

public class CreateCompanyHandlerTests
{
    private static AdminDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AdminDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidDto_CreatesCompanyAndReturnsDto()
    {
        await using var context = CreateContext();
        var handler = new CreateCompanyHandler(context);
        var dto = new CreateCompanyDto
        {
            Name = "Empresa Nueva",
            TaxId = "B12345674",
            Address = "Calle Principal 1",
            Phone = "912345678",
            Email = "contacto@empresa.com"
        };

        var result = await handler.Handle(new CreateCompanyCommand(dto), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(dto.Name);
        result.TaxId.Should().Be(dto.TaxId);
        result.Address.Should().Be(dto.Address);
        result.IsActive.Should().BeTrue();
        var saved = await context.Companies.FirstOrDefaultAsync(c => c.Id == result.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be(dto.Name);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ThrowsInvalidOperationException()
    {
        await using var context = CreateContext();
        context.Companies.Add(new CompanyEntity
        {
            Name = "Empresa Existente",
            Address = "Calle 1"
        });
        await context.SaveChangesAsync();

        var handler = new CreateCompanyHandler(context);
        var dto = new CreateCompanyDto
        {
            Name = "Empresa Existente",
            Address = "Calle 2"
        };

        var act = () => handler.Handle(new CreateCompanyCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe una empresa con el nombre*");
    }

    [Fact]
    public async Task Handle_WithMinimalDto_CreatesCompany()
    {
        await using var context = CreateContext();
        var handler = new CreateCompanyHandler(context);
        var dto = new CreateCompanyDto
        {
            Name = "Solo Nombre",
            Address = "Dirección obligatoria"
        };

        var result = await handler.Handle(new CreateCompanyCommand(dto), CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Solo Nombre");
        result.TaxId.Should().BeNull();
        result.Email.Should().BeNull();
    }
}
