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

public class GetAllArticleFamiliesTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly GetAllArticleFamiliesCommandHandler _handler;

    public GetAllArticleFamiliesTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new GetAllArticleFamiliesCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllArticleFamilies_ForCompany()
    {
        // Arrange
        var companyId1 = Guid.NewGuid();
        var companyId2 = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var taxType = new TaxType { Id = taxTypeId, CompanyId = companyId1, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true };
        var taxTypes = new List<TaxType> { taxType };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var families = new List<ArticleFamily>
        {
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId1, Code = "F1", Name = "Family 1", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId1, Code = "F2", Name = "Family 2", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId2, Code = "F3", Name = "Family 3", TaxTypeId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        _contextMock.Setup(c => c.ArticleFamilies).Returns(families.BuildMockDbSet().Object);

        var query = new GetAllArticleFamiliesCommand(companyId1);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Code == "F1");
        result.Should().Contain(f => f.Code == "F2");
        result.Should().NotContain(f => f.Code == "F3"); // Different company
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoFamiliesExistForCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var families = new List<ArticleFamily>();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(families.BuildMockDbSet().Object);

        var query = new GetAllArticleFamiliesCommand(companyId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
