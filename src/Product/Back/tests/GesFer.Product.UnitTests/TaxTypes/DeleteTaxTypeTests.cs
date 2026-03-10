using FluentAssertions;
using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Handlers.TaxTypes;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.TaxTypes;

public class DeleteTaxTypeTests
{
    private readonly ApplicationDbContext _context;
    private readonly DeleteTaxTypeCommandHandler _handler;

    public DeleteTaxTypeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new DeleteTaxTypeCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldSoftDeleteTaxType_WhenRequestIsValid()
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
            Value = 21.0m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new DeleteTaxTypeCommand(taxTypeId, companyId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var deleted = await _context.TaxTypes.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == taxTypeId);
        deleted.Should().NotBeNull();
        deleted!.DeletedAt.Should().NotBeNull();
        deleted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCompanyIdIsEmpty()
    {
        // Arrange
        var command = new DeleteTaxTypeCommand(Guid.NewGuid(), null);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CompanyId*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenTaxTypeNotFound()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new DeleteTaxTypeCommand(Guid.NewGuid(), companyId);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tipo de impuesto no encontrado*");
    }
}
