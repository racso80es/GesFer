using Microsoft.EntityFrameworkCore;
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
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new UpdateArticleFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxType1Id = Guid.NewGuid();
        var taxType2Id = Guid.NewGuid();

        var taxTypes = new List<TaxType>
        {
            new TaxType { Id = taxType1Id, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = taxType2Id, CompanyId = companyId, Code = "T2", Name = "Tax 2", Value = 20, CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        var familyId = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            Code = "OLD",
            Name = "Old Name",
            Description = "Old Desc",
            TaxTypeId = taxType1Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamilies = new List<ArticleFamily> { family };

        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        // We also need to setup FindAsync because the handler probably uses it
        _contextMock.Setup(c => c.ArticleFamilies.FindAsync(familyId)).ReturnsAsync(family);

        var command = new UpdateArticleFamilyCommand(familyId,
            new UpdateArticleFamilyDto
            {
                Id = familyId,
                Code = "NEW",
                Name = "New Name",
                Description = "New Desc",
                TaxTypeId = taxType2Id
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
        result.Code.Should().Be("NEW");
        result.Name.Should().Be("New Name");
        result.TaxTypeId.Should().Be(taxType2Id);

        family.Code.Should().Be("NEW");
        family.TaxTypeId.Should().Be(taxType2Id);
        family.UpdatedAt.Should().NotBeNull();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var articleFamilies = new List<ArticleFamily>();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ArticleFamilies.FindAsync(familyId)).ReturnsAsync((ArticleFamily?)null);

        var command = new UpdateArticleFamilyCommand(familyId,
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

        var articleFamilies = new List<ArticleFamily> { family1, family2 };
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ArticleFamilies.FindAsync(family2.Id)).ReturnsAsync(family2);

        var taxTypes = new List<TaxType> { new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true } };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

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
        var articleFamilies = new List<ArticleFamily> { family };

        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ArticleFamilies.FindAsync(family.Id)).ReturnsAsync(family);

        var taxTypes = new List<TaxType>(); // Empty, tax type does not exist
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

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
