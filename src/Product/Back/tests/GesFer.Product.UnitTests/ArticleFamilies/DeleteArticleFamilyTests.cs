using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class DeleteArticleFamilyTests
{
    private readonly ApplicationDbContext _context;
    private readonly DeleteArticleFamilyCommandHandler _handler;

    public DeleteArticleFamilyTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new DeleteArticleFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = id,
            CompanyId = Guid.NewGuid(),
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.ArticleFamilies.Add(family);
        await _context.SaveChangesAsync();

        var command = new DeleteArticleFamilyCommand(id);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var deleted = await _context.ArticleFamilies.FindAsync(id);
        deleted.Should().NotBeNull();
        deleted!.DeletedAt.Should().NotBeNull();
        deleted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var command = new DeleteArticleFamilyCommand(Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenAlreadyDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = id,
            CompanyId = Guid.NewGuid(),
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            DeletedAt = DateTime.UtcNow
        };
        _context.ArticleFamilies.Add(family);
        await _context.SaveChangesAsync();

        var command = new DeleteArticleFamilyCommand(id);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }
}
