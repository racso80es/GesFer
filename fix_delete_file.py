import sys

content = """using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new DeleteArticleFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            TaxTypeId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamilies = new List<ArticleFamily> { family };
        var articleFamiliesDbSetMock = articleFamilies.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesDbSetMock.Object);

        var command = new DeleteArticleFamilyCommand(familyId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        articleFamilies.First().IsActive.Should().BeFalse();
        articleFamilies.First().DeletedAt.Should().NotBeNull();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var articleFamilies = new List<ArticleFamily>();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var command = new DeleteArticleFamilyCommand(familyId);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }
}
"""

with open('src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/DeleteArticleFamilyTests.cs', 'w') as f:
    f.write(content)
