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

public class GetAllArticleFamiliesTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();

    private readonly GetAllArticleFamiliesCommandHandler _handler;

    public GetAllArticleFamiliesTests()
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
        _handler = new GetAllArticleFamiliesCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllFamilies_WhenNoFilter()
    {
        // Arrange
        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();
        var taxType1 = Guid.NewGuid();
        var taxType2 = Guid.NewGuid();

        _taxTypes.AddRange(new[] { new TaxType { Id = taxType1, CompanyId = company1, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = taxType2, CompanyId = company2, Code = "T2", Name = "Tax 2", Value = 20, CreatedAt = DateTime.UtcNow, IsActive = true } });

        _articleFamilies.AddRange(new[] { new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company1, Code = "F1", Name = "Family 1", TaxTypeId = taxType1, CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company2, Code = "F2", Name = "Family 2", TaxTypeId = taxType2, CreatedAt = DateTime.UtcNow, IsActive = true } });
        // // // // // //


        var command = new GetAllArticleFamiliesCommand();

        // Act

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(2);
        result.Select(f => f.Name).Should().Contain(new[] { "Family 1", "Family 2" });
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByCompanyId_WhenProvided()
    {
        // Arrange
        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();
        var taxType1 = Guid.NewGuid();
        var taxType2 = Guid.NewGuid();

        _taxTypes.AddRange(new[] { new TaxType { Id = taxType1, CompanyId = company1, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = taxType2, CompanyId = company2, Code = "T2", Name = "Tax 2", Value = 20, CreatedAt = DateTime.UtcNow, IsActive = true } });

        _articleFamilies.AddRange(new[] { new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company1, Code = "F1", Name = "Family 1", TaxTypeId = taxType1, CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company2, Code = "F2", Name = "Family 2", TaxTypeId = taxType2, CreatedAt = DateTime.UtcNow, IsActive = true } });
        // // // // // //


        var command = new GetAllArticleFamiliesCommand(company1);

        // Act

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(1);
        result.First().CompanyId.Should().Be(company1);
    }

    [Fact]
    public async Task HandleAsync_ShouldExcludeDeletedFamilies()
    {
        // Arrange
        var company = Guid.NewGuid();
        var taxType = Guid.NewGuid();

        _taxTypes.Add(new TaxType { Id = taxType, CompanyId = company, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true });

        _articleFamilies.AddRange(new[] { new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company, Code = "F1", Name = "Family 1", TaxTypeId = taxType, CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company, Code = "F2", Name = "Family 2", TaxTypeId = taxType, CreatedAt = DateTime.UtcNow, IsActive = false, DeletedAt = DateTime.UtcNow } });
        // // // // // //


        var command = new GetAllArticleFamiliesCommand();

        // Act

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Family 1");
    }

    [Fact]
    public async Task HandleAsync_ShouldOrderByName()
    {
        // Arrange
        var company = Guid.NewGuid();
        var taxType = Guid.NewGuid();

        _taxTypes.Add(new TaxType { Id = taxType, CompanyId = company, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true });

        _articleFamilies.AddRange(new[] { new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company, Code = "F2", Name = "B Family", TaxTypeId = taxType, CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = company, Code = "F1", Name = "A Family", TaxTypeId = taxType, CreatedAt = DateTime.UtcNow, IsActive = true } });
        // // // // // //


        var command = new GetAllArticleFamiliesCommand();

        // Act

SetupMocks();
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("A Family");
        result[1].Name.Should().Be("B Family");
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