using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.DTOs.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using MockQueryable.Moq;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.UnitTests.Handlers.ArticleFamilies;

public class UpdateArticleFamilyHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly UpdateArticleFamilyCommandHandler _handler;

    public UpdateArticleFamilyHandlerTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockContext = new Mock<ApplicationDbContext>(options);
        _handler = new UpdateArticleFamilyCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();
        var existingFamily = new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM01",
            Name = "Old Name",
            TaxTypeId = taxTypeId,
            DeletedAt = null
        };

        var command = new UpdateArticleFamilyCommand(id, new UpdateArticleFamilyDto
        {
            Id = id,
            Code = "FAM01-UPD",
            Name = "New Name",
            Description = "Updated description",
            TaxTypeId = taxTypeId
        });

        // Mock ArticleFamilies
        var articleFamilies = new List<ArticleFamily> { existingFamily }.BuildMockDbSet();
        _mockContext.Setup(c => c.ArticleFamilies).Returns(articleFamilies.Object);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("FAM01-UPD");
        result.Name.Should().Be("New Name");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var command = new UpdateArticleFamilyCommand(Guid.NewGuid(), new UpdateArticleFamilyDto());

        // Mock empty ArticleFamilies
        var articleFamilies = new List<ArticleFamily>().BuildMockDbSet();
        _mockContext.Setup(c => c.ArticleFamilies).Returns(articleFamilies.Object);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCodeDuplicatesAnotherFamily()
    {
        // Arrange
        var id = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var existingFamily = new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM01",
            DeletedAt = null
        };

        var otherFamily = new ArticleFamily
        {
            Id = otherId,
            CompanyId = companyId,
            Code = "FAM02",
            DeletedAt = null
        };

        var command = new UpdateArticleFamilyCommand(id, new UpdateArticleFamilyDto
        {
            Id = id,
            Code = "FAM02", // Trying to use code of other family
            TaxTypeId = Guid.NewGuid()
        });

        // Mock ArticleFamilies
        var articleFamilies = new List<ArticleFamily> { existingFamily, otherFamily }.BuildMockDbSet();
        _mockContext.Setup(c => c.ArticleFamilies).Returns(articleFamilies.Object);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe una familia de artículos con este código*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenTaxTypeDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var existingFamily = new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM01",
            TaxTypeId = Guid.NewGuid(),
            DeletedAt = null
        };

        var newTaxTypeId = Guid.NewGuid();
        var command = new UpdateArticleFamilyCommand(id, new UpdateArticleFamilyDto
        {
            Id = id,
            Code = "FAM01",
            TaxTypeId = newTaxTypeId // Changing TaxType
        });

        // Mock ArticleFamilies
        var articleFamilies = new List<ArticleFamily> { existingFamily }.BuildMockDbSet();
        _mockContext.Setup(c => c.ArticleFamilies).Returns(articleFamilies.Object);

        // Mock TaxTypes (empty, so new one doesn't exist)
        var taxTypes = new List<TaxType>().BuildMockDbSet();
        _mockContext.Setup(c => c.TaxTypes).Returns(taxTypes.Object);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El tipo de tasa no existe*");
    }
}
