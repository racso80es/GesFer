using FluentAssertions;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GesFer.IntegrationTests.Persistence;

[Collection("DatabaseStep")]
public class ArticleTests
{
    private readonly DatabaseFixture _fixture;

    public ArticleTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Can_Create_And_Retrieve_Article()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Setup Dependencies
        // Intentar obtener una empresa existente (sembrada) o crear una nueva si es necesario
        var company = await context.Set<GesFer.Product.Back.Domain.Entities.Company>().FirstOrDefaultAsync();
        Guid companyId;

        if (company == null)
        {
            companyId = Guid.NewGuid();
            var newCompany = new GesFer.Product.Back.Domain.Entities.Company
            {
                Id = companyId,
                Name = "Test Company Persistence",
                Address = "Test Address",
                LanguageId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Asumiendo que el idioma ES existe por seeds maestros
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Set<GesFer.Product.Back.Domain.Entities.Company>().Add(newCompany);
            // Guardar Company primero para asegurar FKs
            await context.SaveChangesAsync();
        }
        else
        {
            companyId = company.Id;
        }

        // Create TaxType
        var taxType = new TaxType
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = "IVA21-" + Guid.NewGuid().ToString("N")[..4], // Randomize to avoid collisions
            Name = "IVA 21 Test",
            Value = 21m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.TaxTypes.Add(taxType);

        // Create ArticleFamily
        var articleFamily = new ArticleFamily
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = "FAM-" + Guid.NewGuid().ToString("N")[..4],
            Name = "Family Test",
            TaxTypeId = taxType.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.ArticleFamilies.Add(articleFamily);

        await context.SaveChangesAsync();

        // 2. Create Article
        var article = new Article
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ArticleFamilyId = articleFamily.Id,
            Code = "ART-" + Guid.NewGuid().ToString("N")[..4],
            Name = "Article Test",
            Description = "Description Test",
            BuyPrice = 100m,
            SellPrice = 150m,
            Stock = 10m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // 3. Act
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // 4. Assert
        // Limpiar tracker para asegurar lectura de DB
        context.ChangeTracker.Clear();

        var savedArticle = await context.Articles
            .Include(a => a.ArticleFamily)
            .FirstOrDefaultAsync(a => a.Id == article.Id);

        savedArticle.Should().NotBeNull();
        savedArticle!.Name.Should().Be("Article Test");
        savedArticle.ArticleFamily.Should().NotBeNull();
        savedArticle.ArticleFamily.Code.Should().Be(articleFamily.Code);
    }
}
