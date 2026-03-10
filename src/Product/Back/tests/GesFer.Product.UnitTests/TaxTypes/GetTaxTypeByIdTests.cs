using FluentAssertions;
using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Handlers.TaxTypes;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.TaxTypes;

public class GetTaxTypeByIdTests
{
    private readonly ApplicationDbContext _context;
    private readonly GetTaxTypeByIdCommandHandler _handler;

    public GetTaxTypeByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new GetTaxTypeByIdCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTaxType_WhenFoundAndActive()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        _context.TaxTypes.Add(new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Code = "IVA21",
            Name = "IVA 21%",
            Value = 21,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new GetTaxTypeByIdCommand(taxTypeId, companyId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taxTypeId);
        result.Code.Should().Be("IVA21");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var command = new GetTaxTypeByIdCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }
}
