import os

def write_create():
    content = """using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.DTOs.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new CreateArticleFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var taxTypes = new List<TaxType>
        {
            new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var articleFamilies = new List<ArticleFamily>();
        var articleFamiliesDbSetMock = articleFamilies.BuildMockDbSet();
        articleFamiliesDbSetMock.Setup(d => d.Add(It.IsAny<ArticleFamily>())).Callback<ArticleFamily>(articleFamilies.Add);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesDbSetMock.Object);

        var command = new CreateArticleFamilyCommand(companyId,
            new ArticleFamilyDto
            {
                Code = "FAM1",
                Name = "Family 1",
                Description = "Description 1",
                TaxTypeId = taxTypeId
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("FAM1");
        result.Name.Should().Be("Family 1");
        result.TaxTypeId.Should().Be(taxTypeId);

        articleFamilies.Should().ContainSingle();
        articleFamilies.First().Code.Should().Be("FAM1");
        articleFamilies.First().CompanyId.Should().Be(companyId);

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCodeAlreadyExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var existingFamily = new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId, Code = "FAM1", Name = "F1", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true };
        var articleFamilies = new List<ArticleFamily> { existingFamily };
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var taxTypes = new List<TaxType>
        {
            new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var command = new CreateArticleFamilyCommand(companyId,
            new ArticleFamilyDto
            {
                Code = "FAM1",
                Name = "Another Family",
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

        var articleFamilies = new List<ArticleFamily>();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var taxTypes = new List<TaxType>(); // Empty
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var command = new CreateArticleFamilyCommand(companyId,
            new ArticleFamilyDto
            {
                Code = "FAM1",
                Name = "Another Family",
                TaxTypeId = Guid.NewGuid() // Non-existent
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El tipo de tasa no existe*");
    }
}
"""
    with open('src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/CreateArticleFamilyTests.cs', 'w') as f:
        f.write(content)

def write_delete():
    content = """using FluentAssertions;
using GesFer.Application.Commands.ArticleFamilies;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class DeleteArticleFamilyTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly DeleteArticleFamilyCommandHandler _handler;

    public DeleteArticleFamilyTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new DeleteArticleFamilyCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldDeleteArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            TaxTypeId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamilies = new List<ArticleFamily> { family };
        var articleFamiliesDbSetMock = articleFamilies.BuildMockDbSet();
        articleFamiliesDbSetMock.Setup(d => d.Remove(It.IsAny<ArticleFamily>())).Callback<ArticleFamily>(f => articleFamilies.Remove(f));
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamiliesDbSetMock.Object);
        _contextMock.Setup(c => c.ArticleFamilies.FindAsync(familyId)).ReturnsAsync(family);

        // Setup Articles for validation
        var articles = new List<Article>();
        _contextMock.Setup(c => c.Articles).Returns(articles.BuildMockDbSet().Object);

        var command = new DeleteArticleFamilyCommand(familyId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeTrue();
        articleFamilies.Should().BeEmpty();
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

        var command = new DeleteArticleFamilyCommand(familyId);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenFamilyHasArticles()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            TaxTypeId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamilies = new List<ArticleFamily> { family };
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ArticleFamilies.FindAsync(familyId)).ReturnsAsync(family);

        var articles = new List<Article>
        {
            new Article { Id = Guid.NewGuid(), CompanyId = companyId, Code = "ART1", Name = "Article 1", ArticleFamilyId = familyId, CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        _contextMock.Setup(c => c.Articles).Returns(articles.BuildMockDbSet().Object);

        var command = new DeleteArticleFamilyCommand(familyId);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se puede eliminar la familia porque tiene artículos asociados*");
    }
}
"""
    with open('src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/DeleteArticleFamilyTests.cs', 'w') as f:
        f.write(content)

def write_getbyid():
    content = """using FluentAssertions;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Application.Queries.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class GetArticleFamilyByIdTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly GetArticleFamilyByIdQueryHandler _handler;

    public GetArticleFamilyByIdTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new GetArticleFamilyByIdQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnArticleFamily_WhenItExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var taxType = new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true };
        var taxTypes = new List<TaxType> { taxType };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            Code = "FAM1",
            Name = "Family 1",
            TaxTypeId = taxTypeId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var articleFamilies = new List<ArticleFamily> { family };
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var query = new GetArticleFamilyByIdQuery(familyId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(familyId);
        result.Code.Should().Be("FAM1");
        result.Name.Should().Be("Family 1");
        result.TaxType.Should().NotBeNull();
        result.TaxType!.Id.Should().Be(taxTypeId);
        result.TaxType.Code.Should().Be("T1");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenEntityNotFound()
    {
        // Arrange
        var articleFamilies = new List<ArticleFamily>();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(articleFamilies.BuildMockDbSet().Object);

        var query = new GetArticleFamilyByIdQuery(Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(query))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la familia de artículos*");
    }
}
"""
    with open('src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs', 'w') as f:
        f.write(content)

def write_getall():
    content = """using FluentAssertions;
using GesFer.Application.Handlers.ArticleFamilies;
using GesFer.Application.Queries.ArticleFamilies;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.ArticleFamilies;

public class GetAllArticleFamiliesTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly GetAllArticleFamiliesQueryHandler _handler;

    public GetAllArticleFamiliesTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new GetAllArticleFamiliesQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllArticleFamilies_ForCompany()
    {
        // Arrange
        var companyId1 = Guid.NewGuid();
        var companyId2 = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        var taxType = new TaxType { Id = taxTypeId, CompanyId = companyId1, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true };
        var taxTypes = new List<TaxType> { taxType };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        var families = new List<ArticleFamily>
        {
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId1, Code = "F1", Name = "Family 1", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId1, Code = "F2", Name = "Family 2", TaxTypeId = taxTypeId, CreatedAt = DateTime.UtcNow, IsActive = true },
            new ArticleFamily { Id = Guid.NewGuid(), CompanyId = companyId2, Code = "F3", Name = "Family 3", TaxTypeId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, IsActive = true }
        };

        _contextMock.Setup(c => c.ArticleFamilies).Returns(families.BuildMockDbSet().Object);

        var query = new GetAllArticleFamiliesQuery(companyId1);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Code == "F1");
        result.Should().Contain(f => f.Code == "F2");
        result.Should().NotContain(f => f.Code == "F3"); // Different company
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoFamiliesExistForCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var families = new List<ArticleFamily>();
        _contextMock.Setup(c => c.ArticleFamilies).Returns(families.BuildMockDbSet().Object);

        var query = new GetAllArticleFamiliesQuery(companyId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
"""
    with open('src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetAllArticleFamiliesTests.cs', 'w') as f:
        f.write(content)

write_create()
write_delete()
write_getbyid()
write_getall()
