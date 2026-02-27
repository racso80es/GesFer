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

public class CreateArticleFamilyTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly CreateArticleFamilyCommandHandler _handler;

    public CreateArticleFamilyTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>();
        _contextMock = new Mock<ApplicationDbContext>(options);
        _handler = new CreateArticleFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();
        var taxType = new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Code = "IVA21",
            Name = "IVA General 21%",
            Value = 21.0m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var taxTypesMock = new List<TaxType> { taxType }.BuildMockDbSet();
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypesMock.Object);

        // Mock empty ArticleFamilies to ensure no duplicates
        var articleFamiliesMock = new List<ArticleFamily>().BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        // Setup SaveChangesAsync
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateArticleFamilyCommand(
            new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM01",
                Name = "Familia Test",
                Description = "Descripción de prueba",
                TaxTypeId = taxTypeId
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Code.Should().Be("FAM01");
        result.TaxTypeId.Should().Be(taxTypeId);

        // Verify Add was called
        _contextMock.Verify(c => c.ArticleFamilies.Add(It.Is<ArticleFamily>(a =>
            a.CompanyId == companyId &&
            a.Code == "FAM01" &&
            a.TaxTypeId == taxTypeId
        )), Times.Once);

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCompanyIdIsEmpty()
    {
        // Arrange
        var command = new CreateArticleFamilyCommand(
            new CreateArticleFamilyDto
            {
                CompanyId = Guid.Empty,
                Code = "FAM01",
                Name = "Familia Test",
                TaxTypeId = Guid.NewGuid()
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
        var taxTypeId = Guid.NewGuid();
        var existingFamily = new ArticleFamily
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = "FAM01",
            Name = "Familia Existente",
            TaxTypeId = taxTypeId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var taxTypesMock = new List<TaxType>().BuildMockDbSet(); // Not needed for this check but good practice
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypesMock.Object);

        var articleFamiliesMock = new List<ArticleFamily> { existingFamily }.BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new CreateArticleFamilyCommand(
            new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM01",
                Name = "Nueva Familia",
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

        // Mock empty TaxTypes
        var taxTypesMock = new List<TaxType>().BuildMockDbSet();
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypesMock.Object);

        // Mock empty ArticleFamilies (so code check passes)
        var articleFamiliesMock = new List<ArticleFamily>().BuildMockDbSet();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesMock.Object);

        var command = new CreateArticleFamilyCommand(
            new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM01",
                Name = "Familia Test",
                TaxTypeId = Guid.NewGuid() // Non-existent TaxType
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El tipo de tasa no existe*");
    }
}
