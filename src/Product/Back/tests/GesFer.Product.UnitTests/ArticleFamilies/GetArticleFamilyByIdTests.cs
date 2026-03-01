using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class GetArticleFamilyByIdTests
{
    private readonly ProductDbContext _context;
    private readonly GetArticleFamilyByIdCommandHandler _handler;

    public GetArticleFamilyByIdTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ProductDbContext(options);
        _handler = new GetArticleFamilyByIdCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnArticleFamily_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        _context.TaxTypes.Add(new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true });
        _context.ArticleFamilies.Add(new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            TaxTypeId = taxTypeId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new GetArticleFamilyByIdCommand(id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.TaxTypeName.Should().Be("Tax 1");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var command = new GetArticleFamilyByIdCommand(Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenCompanyIdMismatch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.ArticleFamilies.Add(new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new GetArticleFamilyByIdCommand(id, Guid.NewGuid()); // Different CompanyId

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();

        _context.ArticleFamilies.Add(new ArticleFamily
        {
            Id = id,
            CompanyId = Guid.NewGuid(),
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            DeletedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var command = new GetArticleFamilyByIdCommand(id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }
}
