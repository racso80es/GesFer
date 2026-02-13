using BenchmarkDotNet.Attributes;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Performance.Benchmarks;

[MemoryDiagnoser]
public class StockBenchmark
{
    private ApplicationDbContext _context = null!;
    private StockService _service = null!;
    private List<Guid> _articleIds = null!;
    private const int ArticleCount = 100;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new StockService(_context);

        _articleIds = new List<Guid>();
        var companyId = Guid.NewGuid();
        var taxType = new TaxType
        {
            Id = Guid.NewGuid(),
            Name = "IVA 21%",
            Code = "T21",
            Value = 21,
            CompanyId = companyId
        };
        var articleFamily = new ArticleFamily
        {
            Id = Guid.NewGuid(),
            Name = "Fam",
            Code = "FAM01",
            CompanyId = companyId,
            TaxTypeId = taxType.Id,
            TaxType = taxType
        };
        _context.TaxTypes.Add(taxType);
        _context.ArticleFamilies.Add(articleFamily);
        for (int i = 0; i < ArticleCount; i++)
        {
            var article = new Article
            {
                Id = Guid.NewGuid(),
                Name = $"Article {i}",
                Code = $"ART{i:000}",
                Stock = 100,
                CompanyId = companyId,
                ArticleFamilyId = articleFamily.Id,
                ArticleFamily = articleFamily
            };
            _context.Articles.Add(article);
            _articleIds.Add(article.Id);
        }
        _context.SaveChanges();
    }

    [Benchmark(Baseline = true)]
    public async Task UpdateStock_NPlusOne_Writes()
    {
        // This simulates the old behavior: Loop calling IncreaseStockAsync
        // which performs Fetch + Update + Save for EACH item.
        foreach (var id in _articleIds)
        {
            await _service.IncreaseStockAsync(id, 1);
        }
    }

    [Benchmark]
    public async Task UpdateStock_Batch_Write()
    {
        // This simulates the new behavior: Batch Fetch + In-Memory Update + Single Save
        var articles = await _context.Articles
            .Where(a => _articleIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);

        foreach (var id in _articleIds)
        {
            if (articles.TryGetValue(id, out var article))
            {
                _service.ApplyStockIncrease(article, 1);
            }
        }
        await _context.SaveChangesAsync();
    }
}
