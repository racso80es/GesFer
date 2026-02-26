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

public class CreateArticleFamilyHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly CreateArticleFamilyCommandHandler _handler;

    public CreateArticleFamilyHandlerTests()
    {
        // Mock ApplicationDbContext using a valid DbContextOptions (though it won't be used by the mock)
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockContext = new Mock<ApplicationDbContext>(options);

        _handler = new CreateArticleFamilyCommandHandler(_mockContext.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();
        var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto
        {
            CompanyId = companyId,
            Code = "FAM01",
            Name = "Familia Test",
            Description = "Descripción de prueba",
            TaxTypeId = taxTypeId
        });

        // Mock empty ArticleFamilies (no duplicates)
        var articleFamilies = new List<ArticleFamily>().BuildMockDbSet();
        _mockContext.Setup(c => c.ArticleFamilies).Returns(articleFamilies.Object);

        // Mock TaxTypes (tax type exists)
        var taxType = new TaxType { Id = taxTypeId, CompanyId = companyId, Name = "IVA 21%", Value = 21.0m, DeletedAt = null };
        var taxTypes = new List<TaxType> { taxType }.BuildMockDbSet();
        _mockContext.Setup(c => c.TaxTypes).Returns(taxTypes.Object);
        _mockContext.Setup(c => c.TaxTypes.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxType);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("FAM01");
        result.TaxTypeId.Should().Be(taxTypeId);
        result.TaxTypeName.Should().Be("IVA 21%");

        _mockContext.Verify(c => c.ArticleFamilies.Add(It.IsAny<ArticleFamily>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCompanyIdIsEmpty()
    {
        // Arrange
        var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto
        {
            CompanyId = Guid.Empty, // Invalid
            Code = "FAM01"
        });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CompanyId es obligatorio*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCodeAlreadyExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var existingFamily = new ArticleFamily
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = "FAM01",
            DeletedAt = null
        };

        var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto
        {
            CompanyId = companyId,
            Code = "FAM01" // Duplicate
        });

        // Mock existing family
        var articleFamilies = new List<ArticleFamily> { existingFamily }.BuildMockDbSet();
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
        var companyId = Guid.NewGuid();
        var command = new CreateArticleFamilyCommand(new CreateArticleFamilyDto
        {
            CompanyId = companyId,
            Code = "FAM01",
            TaxTypeId = Guid.NewGuid() // Non-existent
        });

        // Mock empty ArticleFamilies (no duplicates)
        var articleFamilies = new List<ArticleFamily>().BuildMockDbSet();
        _mockContext.Setup(c => c.ArticleFamilies).Returns(articleFamilies.Object);

        // Mock TaxTypes (empty)
        var taxTypes = new List<TaxType>().BuildMockDbSet();
        _mockContext.Setup(c => c.TaxTypes).Returns(taxTypes.Object);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El tipo de tasa no existe*");
    }
}
