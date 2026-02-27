using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.DTOs.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class UpdateArticleFamilyTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly UpdateArticleFamilyCommandHandler _handler;

    public UpdateArticleFamilyTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>();
        _contextMock = new Mock<ApplicationDbContext>(options);
        _handler = new UpdateArticleFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxType1 = Guid.NewGuid();
        var taxType2 = Guid.NewGuid();

        var taxTypes = new List<TaxType>
        {
            new TaxType { Id = taxType1, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = taxType2, CompanyId = companyId, Code = "T2", Name = "Tax 2", Value = 20, CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        var familyId = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            Code = "OLD",
            Name = "Old Name",
            Description = "Old Desc",
            TaxTypeId = taxType1,
            TaxType = taxTypes[0],
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamiliesMock = new List<ArticleFamily> { family }.BuildMockDbSet();
        var taxTypesMock = taxTypes.BuildMockDbSet();

        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypesMock.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateArticleFamilyCommand(familyId,
            new UpdateArticleFamilyDto
            {
                Id = familyId,
                Code = "NEW",
                Name = "New Name",
                Description = "New Desc",
                TaxTypeId = taxType2
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
        result.Code.Should().Be("NEW");
        result.Name.Should().Be("New Name");
        result.TaxTypeId.Should().Be(taxType2);

        // Check if entity was updated in memory (mock object)
        family.Code.Should().Be("NEW");
        family.TaxTypeId.Should().Be(taxType2);

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var articleFamiliesMock = new List<ArticleFamily>().BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new UpdateArticleFamilyCommand(Guid.NewGuid(),
            new UpdateArticleFamilyDto
            {
                Code = "TEST",
                Name = "Test"
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCodeAlreadyExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var family1 = new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId, Code = "FAM1", Name = "F1", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true };
        var family2 = new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId, Code = "FAM2", Name = "F2", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true };

        var families = new List<ArticleFamily> { family1, family2 };
        var articleFamiliesMock = families.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        // Try to update family2 with family1's code
        var command = new UpdateArticleFamilyCommand(family2.Id,
            new UpdateArticleFamilyDto
            {
                Id = family2.Id,
                Code = "FAM1", // Duplicate!
                Name = "Updated Name",
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
        var taxTypeId = Guid.NewGuid();

        var family = new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId, Code = "FAM1", Name = "F1", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true };

        var articleFamiliesMock = new List<ArticleFamily> { family }.BuildMockDbSet();
        var taxTypesMock = new List<TaxType>().BuildMockDbSet(); // Empty tax types

        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypesMock.Object);

        var command = new UpdateArticleFamilyCommand(family.Id,
            new UpdateArticleFamilyDto
            {
                Id = family.Id,
                Code = "FAM1",
                Name = "Updated Name",
                TaxTypeId = Guid.NewGuid() // Non-existent
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El tipo de tasa no existe*");
    }
}
