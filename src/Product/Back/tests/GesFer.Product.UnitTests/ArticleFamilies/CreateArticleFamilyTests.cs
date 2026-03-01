using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.DTOs.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class CreateArticleFamilyTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly CreateArticleFamilyCommandHandler _handler;

    public CreateArticleFamilyTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new CreateArticleFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var taxTypes = new List<TaxType>
        {
            new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var articleFamilies = new List<ArticleFamily>();
        var articleFamiliesDbSetMock = articleFamilies.BuildMockDbSet();
        articleFamiliesDbSetMock.Setup(d => d.Add(It.IsAny<ArticleFamily>())).Callback<ArticleFamily>(articleFamilies.Add);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesDbSetMock.Object);

        var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM1",
                Name = "Family 1",
                Description = "Description 1",
                TaxTypeId = taxTypeId
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("FAM1");
        result.Name.Should().Be("Family 1");
        result.TaxTypeId.Should().Be(taxTypeId);

        articleFamilies.Should().ContainSingle();
        articleFamilies.First().Code.Should().Be("FAM1");
        articleFamilies.First().CompanyId.Should().Be(companyId);

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCodeAlreadyExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var existingFamily = new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId, Code = "FAM1", Name = "F1", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true };
        var articleFamilies = new List<ArticleFamily> { existingFamily };
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var taxTypes = new List<TaxType>
        {
            new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM1",
                Name = "Another Family",
                TaxTypeId = taxTypeId
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe una familia de artículos con este código*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenTaxTypeDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        var articleFamilies = new List<ArticleFamily>();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var taxTypes = new List<TaxType>(); // Empty
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM1",
                Name = "Another Family",
                TaxTypeId = Guid.NewGuid() // Non-existent
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El tipo de tasa no existe*");
    }
}
