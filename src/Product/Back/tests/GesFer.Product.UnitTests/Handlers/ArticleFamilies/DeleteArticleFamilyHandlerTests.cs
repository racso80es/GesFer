using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.UnitTests.Handlers.ArticleFamilies;

public class DeleteArticleFamilyHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly DeleteArticleFamilyCommandHandler _handler;

    public DeleteArticleFamilyHandlerTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockContext = new Mock<ApplicationDbContext>(options);
        _handler = new DeleteArticleFamilyCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldSoftDeleteArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingFamily = new ArticleFamily
        {
            Id = id,
            IsActive = true,
            DeletedAt = null
        };

        var command = new DeleteArticleFamilyCommand(id);

        // Mock ArticleFamilies
        var articleFamilies = new List<ArticleFamily> { existingFamily }.BuildMockDbSet();
        _mockContext.Setup(c => c.ArticleFamilies).Returns(articleFamilies.Object);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        existingFamily.IsActive.Should().BeFalse();
        existingFamily.DeletedAt.Should().NotBeNull();

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var command = new DeleteArticleFamilyCommand(Guid.NewGuid());

        // Mock empty ArticleFamilies
        var articleFamilies = new List<ArticleFamily>().BuildMockDbSet();
        _mockContext.Setup(c => c.ArticleFamilies).Returns(articleFamilies.Object);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }
}
