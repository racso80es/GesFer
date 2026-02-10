using FluentAssertions;
using GesFer.Admin.Application.Handlers.Company;
using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

using CompanyEntity = GesFer.Shared.Back.Domain.Entities.Company;

namespace GesFer.Admin.UnitTests.Handlers.Company;

public class GetCompanyByIdHandlerTests
{
    private static AdminDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AdminDbContext(options);
    }

    [Fact]
    public async Task Handle_WithExistingId_ReturnsCompanyDto()
    {
        await using var context = CreateContext();
        var companyId = Guid.NewGuid();
        context.Companies.Add(new CompanyEntity
        {
            Id = companyId,
            Name = "Empresa Consulta",
            Address = "Calle Get 1",
            Phone = "911222333",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new GetCompanyByIdHandler(context);
        var result = await handler.Handle(new GetCompanyByIdCommand(companyId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(companyId);
        result.Name.Should().Be("Empresa Consulta");
        result.Address.Should().Be("Calle Get 1");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNull()
    {
        await using var context = CreateContext();
        var handler = new GetCompanyByIdHandler(context);

        var result = await handler.Handle(new GetCompanyByIdCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithSoftDeletedCompany_ReturnsNull()
    {
        await using var context = CreateContext();
        var companyId = Guid.NewGuid();
        context.Companies.Add(new CompanyEntity
        {
            Id = companyId,
            Name = "Empresa Borrada",
            Address = "Calle 1",
            DeletedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new GetCompanyByIdHandler(context);
        var result = await handler.Handle(new GetCompanyByIdCommand(companyId), CancellationToken.None);

        result.Should().BeNull();
    }
}
