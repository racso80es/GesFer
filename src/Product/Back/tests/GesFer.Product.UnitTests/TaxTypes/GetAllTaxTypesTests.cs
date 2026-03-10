using FluentAssertions;
using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Handlers.TaxTypes;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.TaxTypes;

public class GetAllTaxTypesTests
{
    private readonly ApplicationDbContext _context;
    private readonly GetAllTaxTypesCommandHandler _handler;

    public GetAllTaxTypesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new GetAllTaxTypesCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnActiveTaxTypesForCompany_OrderedByCode()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();

        _context.TaxTypes.AddRange(
            new TaxType { Id = Guid.NewGuid(), CompanyId = companyId, Code = "IVA10", Name = "IVA 10%", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = Guid.NewGuid(), CompanyId = companyId, Code = "IVA21", Name = "IVA 21%", Value = 21, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = Guid.NewGuid(), CompanyId = companyId, Code = "IVA04", Name = "IVA 4%", Value = 4, CreatedAt = DateTime.UtcNow, DeletedAt = DateTime.UtcNow, IsActive = false }, // Inactive
            new TaxType { Id = Guid.NewGuid(), CompanyId = otherCompanyId, Code = "IVA21", Name = "Other", Value = 21, CreatedAt = DateTime.UtcNow, IsActive = true } // Other company
        );
        await _context.SaveChangesAsync();

        var command = new GetAllTaxTypesCommand(companyId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Code.Should().Be("IVA10");
        result[1].Code.Should().Be("IVA21");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCompanyIdIsEmpty()
    {
        // Arrange
        var command = new GetAllTaxTypesCommand(null);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CompanyId*");
    }
}
