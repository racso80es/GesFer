using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class GetArticleFamilyByIdTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly GetArticleFamilyByIdCommandHandler _handler;

    public GetArticleFamilyByIdTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>();
        _contextMock = new Mock<ApplicationDbContext>(options);
        _handler = new GetArticleFamilyByIdCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnArticleFamily_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var taxType = new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true };
        var family = new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            TaxTypeId = taxTypeId,
            TaxType = taxType,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamiliesMock = new List<ArticleFamily> { family }.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

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
        var articleFamiliesMock = new List<ArticleFamily>().BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

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

        var family = new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamiliesMock = new List<ArticleFamily> { family }.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

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
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new GetArticleFamilyByIdCommand(id);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }
}
