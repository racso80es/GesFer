using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class GetAllArticleFamiliesTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly GetAllArticleFamiliesCommandHandler _handler;

    public GetAllArticleFamiliesTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>();
        _contextMock = new Mock<ApplicationDbContext>(options);
        _handler = new GetAllArticleFamiliesCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllFamilies_WhenNoFilter()
    {
        // Arrange
        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();
        var taxType1 = Guid.NewGuid();
        var taxType2 = Guid.NewGuid();

        var taxTypes = new List<TaxType>
        {
            new TaxType { Id = taxType1, CompanyId = company1, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = taxType2, CompanyId = company2, Code = "T2", Name = "Tax 2", Value = 20, CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        var families = new List<ArticleFamily>
        {
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company1, Code = "F1", Name = "Family 1", TaxTypeId = taxType1, TaxType = taxTypes[0], CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company2, Code = "F2", Name = "Family 2", TaxTypeId = taxType2, TaxType = taxTypes[1], CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        var taxTypesMock = taxTypes.BuildMockDbSet();
        var articleFamiliesMock = families.BuildMockDbSet();

        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypesMock.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new GetAllArticleFamiliesCommand();

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(2);
        result.Select(f => f.Name).Should().Contain(new[] { "Family 1", "Family 2" });
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByCompanyId_WhenProvided()
    {
        // Arrange
        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();

        var families = new List<ArticleFamily>
        {
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company1, Code = "F1", Name = "Family 1", CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company2, Code = "F2", Name = "Family 2", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        var articleFamiliesMock = families.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new GetAllArticleFamiliesCommand(company1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(1);
        result.First().CompanyId.Should().Be(company1);
    }

    [Fact]
    public async Task HandleAsync_ShouldExcludeDeletedFamilies()
    {
        // Arrange
        var company = Guid.NewGuid();

        var families = new List<ArticleFamily>
        {
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company, Code = "F1", Name = "Family 1", CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company, Code = "F2", Name = "Family 2", CreatedAt = DateTime.UtcNow, IsActive = false, DeletedAt = DateTime.UtcNow }
        };

        var articleFamiliesMock = families.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new GetAllArticleFamiliesCommand();

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Family 1");
    }

    [Fact]
    public async Task HandleAsync_ShouldOrderByName()
    {
        // Arrange
        var company = Guid.NewGuid();

        var families = new List<ArticleFamily>
        {
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company, Code = "F2", Name = "B Family", CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company, Code = "F1", Name = "A Family", CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        var articleFamiliesMock = families.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new GetAllArticleFamiliesCommand();

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("A Family");
        result[1].Name.Should().Be("B Family");
    }
}
