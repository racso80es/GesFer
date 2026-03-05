using GesFer.Product.UnitTests.Infrastructure;
using MockQueryable.Moq;
using Moq;
using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.DTOs.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class UpdateArticleFamilyTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();

    private readonly UpdateArticleFamilyCommandHandler _handler;

    public UpdateArticleFamilyTests()
    {
        var mockCompanies = _companies.BuildMockDbSet();

        var mockTaxTypes = _taxTypes.BuildMockDbSet();
        mockTaxTypes.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                var id = (Guid)ids[0];
                return _taxTypes.SingleOrDefault(t => t.Id == id);
            });

        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        mockArticleFamilies.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                var id = (Guid)ids[0];
                return _articleFamilies.SingleOrDefault(t => t.Id == id);
            });


        _contextMock = new Mock<ApplicationDbContext>();
        _contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);

        _context = _contextMock.Object;
        _handler = new UpdateArticleFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxType1 = Guid.NewGuid();
        var taxType2 = Guid.NewGuid();

        // Seed TaxTypes
        _taxTypes.AddRange(new[] { new TaxType { Id = taxType1, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = taxType2, CompanyId = companyId, Code = "T2", Name = "Tax 2", Value = 20, CreatedAt = DateTime.UtcNow, IsActive = true } });

        // Seed ArticleFamily
        var familyId = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            Code = "OLD",
            Name = "Old Name",
            Description = "Old Desc",
            TaxTypeId = taxType1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _articleFamilies.Add(family);
        // // // // // //


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

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
        result.Code.Should().Be("NEW");
        result.Name.Should().Be("New Name");
        result.TaxTypeId.Should().Be(taxType2);

        var updated = _articleFamilies.SingleOrDefault(x => x.Id == familyId);
        updated.Should().NotBeNull();
        updated!.Code.Should().Be("NEW");
        updated.TaxTypeId.Should().Be(taxType2);
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var command = new UpdateArticleFamilyCommand(Guid.NewGuid(),
            new UpdateArticleFamilyDto
            {
                Code = "TEST",
                Name = "Test"
            });

        // Act & Assert

SetupMocks();
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

        // Seed TaxType
        _taxTypes.Add(new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true });

        // Seed two families
        var family1 = new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId, Code = "FAM1", Name = "F1", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true };
        var family2 = new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId, Code = "FAM2", Name = "F2", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true };

        _articleFamilies.AddRange(new[] { family1, family2 });
        // // // // // //


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

SetupMocks();
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

        // Seed TaxType
        _taxTypes.Add(new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true });

        // Seed ArticleFamily
        var family = new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId, Code = "FAM1", Name = "F1", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true };
        _articleFamilies.Add(family);
        // // // // // //


        var command = new UpdateArticleFamilyCommand(family.Id,
            new UpdateArticleFamilyDto
            {
                Id = family.Id,
                Code = "FAM1",
                Name = "Updated Name",
                TaxTypeId = Guid.NewGuid() // Non-existent
            });

        // Act & Assert

SetupMocks();
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El tipo de tasa no existe*");
    }

    private void SetupMocks()
    {

        var mockCompanies = _companies.BuildMockDbSet();

        var mockTaxTypes = _taxTypes.BuildMockDbSet();
        mockTaxTypes.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                var id = (Guid)ids[0];
                return _taxTypes.SingleOrDefault(t => t.Id == id);
            });

        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        mockArticleFamilies.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                var id = (Guid)ids[0];
                return _articleFamilies.SingleOrDefault(t => t.Id == id);
            });


        _contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);
    }

}