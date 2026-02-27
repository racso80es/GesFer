using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class DeleteArticleFamilyTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly DeleteArticleFamilyCommandHandler _handler;

    public DeleteArticleFamilyTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>();
        _contextMock = new Mock<ApplicationDbContext>(options);
        _handler = new DeleteArticleFamilyCommandHandler(_contextMock.Object);
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

        var articleFamiliesMock = new List<ArticleFamily> { family }.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        // Setup FindAsync logic on the mock if needed, but the handler uses First/Single usually.
        // Let's check the handler logic. It likely uses EF Core methods.
        // MockQueryable handles IQueryable methods. If FindAsync is used, we might need specific setup or rely on FirstOrDefault.
        // Wait, standard FindAsync is on DbSet, MockQueryable might not cover it directly unless we use FirstOrDefault.
        // Let's assume the handler uses standard EF LINQ which MockQueryable supports.
        // If the handler uses FindAsync specifically, we need to mock it on the DbSet.

        // Mock FindAsync for DbSet
        articleFamiliesMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken _) =>
            {
                var searchId = (Guid)ids[0];
                return searchId == id ? family : null;
            });

        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteArticleFamilyCommand(id);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        family.DeletedAt.Should().NotBeNull();
        family.IsActive.Should().BeFalse();

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var articleFamiliesMock = new List<ArticleFamily>().BuildMockDbSet();
        articleFamiliesMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleFamily?)null);

        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

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

        var articleFamiliesMock = new List<ArticleFamily> { family }.BuildMockDbSet();
        articleFamiliesMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new DeleteArticleFamilyCommand(id);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }
}
