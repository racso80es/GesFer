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

public class CreateArticleFamilyTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();

    private readonly CreateArticleFamilyCommandHandler _handler;

    public CreateArticleFamilyTests()
    {
        var mockCompanies = _companies.BuildMockDbSet();

        var mockTaxTypes = _taxTypes.BuildMockDbSet();

        mockTaxTypes.Setup(m => m.Add(It.IsAny<TaxType>())).Callback<TaxType>((s) => _taxTypes.Add(s));


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
        mockArticleFamilies.Setup(m => m.Add(It.IsAny<ArticleFamily>())).Callback<ArticleFamily>((s) => _articleFamilies.Add(s));

        _context = _contextMock.Object;
        _handler = new CreateArticleFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        // Seed TaxType
        _taxTypes.Add(new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Code = "IVA21",
            Name = "IVA General 21%",
            Value = 21.0m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        // // // // // //


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

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Code.Should().Be("FAM01");
        result.TaxTypeId.Should().Be(taxTypeId);

        var created = _articleFamilies.SingleOrDefault(x => x.Id == result.Id);
        created.Should().NotBeNull();
        created!.CompanyId.Should().Be(companyId);
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

SetupMocks();
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

        // Seed existing family
        _articleFamilies.Add(new ArticleFamily
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = "FAM01",
            Name = "Familia Existente",
            TaxTypeId = taxTypeId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        // Seed TaxType (needed for foreign key check if handler checks it, but handler checks it separately)
        _taxTypes.Add(new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Code = "IVA21",
            Name = "IVA General 21%",
            Value = 21.0m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        // // // // // //


        var command = new CreateArticleFamilyCommand(
            new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM01",
                Name = "Nueva Familia",
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
        var command = new CreateArticleFamilyCommand(
            new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM01",
                Name = "Familia Test",
                TaxTypeId = Guid.NewGuid() // Non-existent TaxType
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

        mockTaxTypes.Setup(m => m.Add(It.IsAny<TaxType>())).Callback<TaxType>((s) => _taxTypes.Add(s));


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
        mockArticleFamilies.Setup(m => m.Add(It.IsAny<ArticleFamily>())).Callback<ArticleFamily>((s) => _articleFamilies.Add(s));
    }

}