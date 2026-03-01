using FluentAssertions;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new GetArticleFamilyByIdCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnArticleFamily_WhenItExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var taxType = new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true };
        var taxTypes = new List<TaxType> { taxType };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            TaxTypeId = taxTypeId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamilies = new List<ArticleFamily> { family };
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var query = new GetArticleFamilyByIdCommand(familyId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
        result.Code.Should().Be("FAM1");
        result.Name.Should().Be("Family 1");



    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var articleFamilies = new List<ArticleFamily>();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var query = new GetArticleFamilyByIdCommand(Guid.NewGuid());

        // Act & Assert
        var result = await _handler.HandleAsync(query);
        result.Should().BeNull();
    }
}
