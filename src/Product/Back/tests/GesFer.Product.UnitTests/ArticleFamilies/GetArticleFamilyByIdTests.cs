using GesFer.Product.UnitTests.Infrastructure;
using MockQueryable.Moq;
using Moq;
using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class GetArticleFamilyByIdTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();

    private readonly GetArticleFamilyByIdCommandHandler _handler;

    public GetArticleFamilyByIdTests()
    {
        var mockCompanies = _companies.BuildMockDbSet();

        var mockTaxTypes = _taxTypes.BuildMockDbSet();
        mockTaxTypes.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                if (ids != null && ids.Length > 0 && ids[0] is Guid id)
                    return _taxTypes.SingleOrDefault(t => t.Id == id);
                return null;
            });
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
        _handler = new GetArticleFamilyByIdCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnArticleFamily_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        _taxTypes.Add(new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true });
        _articleFamilies.Add(new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            TaxTypeId = taxTypeId,
            TaxType = _taxTypes[0],
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        // // // // // //


        var command = new GetArticleFamilyByIdCommand(id);

        // Act

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.TaxTypeName.Should().Be("Tax 1");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var command = new GetArticleFamilyByIdCommand(Guid.NewGuid());

        // Act

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenCompanyIdMismatch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _articleFamilies.Add(new ArticleFamily
        {
            Id = id,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        // // // // // //


        var command = new GetArticleFamilyByIdCommand(id, Guid.NewGuid()); // Different CompanyId

        // Act

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();

        _articleFamilies.Add(new ArticleFamily
        {
            Id = id,
            CompanyId = Guid.NewGuid(),
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            DeletedAt = DateTime.UtcNow
        });
        // // // // // //


        var command = new GetArticleFamilyByIdCommand(id);

        // Act

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }

    private void SetupMocks()
    {

        var mockCompanies = _companies.BuildMockDbSet();

        var mockTaxTypes = _taxTypes.BuildMockDbSet();
        mockTaxTypes.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                if (ids != null && ids.Length > 0 && ids[0] is Guid id)
                    return _taxTypes.SingleOrDefault(t => t.Id == id);
                return null;
            });

        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        mockArticleFamilies.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                if (ids != null && ids.Length > 0 && ids[0] is Guid id)
                    return _articleFamilies.SingleOrDefault(t => t.Id == id);
                return null;
            });

        _contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);
    }
}