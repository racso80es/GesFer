using GesFer.Product.UnitTests.Infrastructure;
using MockQueryable.Moq;
using Moq;
using FluentAssertions;
using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Handlers.TaxTypes;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.TaxTypes;

/// <summary>
/// Tests del handler legacy CreateTaxTypeCommandHandler (ICommandHandler + CompanyId en comando).
/// </summary>
public class CreateTaxTypeTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();

    private readonly CreateTaxTypeCommandHandler _handler;

    public CreateTaxTypeTests()
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
        _handler = new CreateTaxTypeCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateTaxType_WhenRequestIsValid()
    {
        var companyId = Guid.NewGuid();
        var command = new CreateTaxTypeCommand(
            new CreateTaxTypeDto
            {
                Code = "IVA21",
                Name = "IVA General 21%",
                Value = 21.0m
            },
            companyId);

        var id = await _handler.HandleAsync(command);

        id.Should().NotBe(Guid.Empty);
        var created = _taxTypes.SingleOrDefault(x => x.Id == id);
        created.Should().NotBeNull();
        created!.Code.Should().Be("IVA21");
        created.CompanyId.Should().Be(companyId);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCompanyIdIsEmpty()
    {
        var command = new CreateTaxTypeCommand(
            new CreateTaxTypeDto
            {
                Code = "X",
                Name = "Test",
                Value = 0
            },
            null);


SetupMocks();
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CompanyId*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCodeAlreadyExists()
    {
        var companyId = Guid.NewGuid();
        _taxTypes.Add(new TaxType
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = "IVA21",
            Name = "Existente",
            Value = 21,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        // // // // // //


        var command = new CreateTaxTypeCommand(
            new CreateTaxTypeDto
            {
                Code = "IVA21",
                Name = "Otro",
                Value = 21
            },
            companyId);


SetupMocks();
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*código*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenValueIsNegative()
    {
        var companyId = Guid.NewGuid();
        var command = new CreateTaxTypeCommand(
            new CreateTaxTypeDto
            {
                Code = "X",
                Name = "Test",
                Value = -1
            },
            companyId);


SetupMocks();
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*valor*");
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