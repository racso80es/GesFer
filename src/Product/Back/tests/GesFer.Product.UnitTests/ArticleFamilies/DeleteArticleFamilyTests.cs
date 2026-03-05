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

public class DeleteArticleFamilyTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly List<GesFer.Product.Back.Domain.Entities.Company> _companies = new();
    private readonly List<ArticleFamily> _articleFamilies = new();
    private readonly List<TaxType> _taxTypes = new();

    private readonly DeleteArticleFamilyCommandHandler _handler;

    public DeleteArticleFamilyTests()
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
        _handler = new DeleteArticleFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = id,
            CompanyId = Guid.NewGuid(),
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _articleFamilies.Add(family);
        // // // // // //


        var command = new DeleteArticleFamilyCommand(id);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var deleted = _articleFamilies.SingleOrDefault(x => x.Id == id);
        deleted.Should().NotBeNull();
        deleted!.DeletedAt.Should().NotBeNull();
        deleted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var command = new DeleteArticleFamilyCommand(Guid.NewGuid());

        // Act & Assert

SetupMocks();
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenAlreadyDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = id,
            CompanyId = Guid.NewGuid(),
            Code = "FAM1",
            Name = "Family 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            DeletedAt = DateTime.UtcNow
        };
        _articleFamilies.Add(family);
        // // // // // //


        var command = new DeleteArticleFamilyCommand(id);

        // Act & Assert

SetupMocks();
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
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