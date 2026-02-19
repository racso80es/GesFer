using FluentAssertions;
using GesFer.Application.Commands.PurchaseDeliveryNote;
using GesFer.Application.Handlers.PurchaseDeliveryNote;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

// Alias to resolve ambiguity
using PurchaseDeliveryNoteEntity = GesFer.Product.Back.Domain.Entities.PurchaseDeliveryNote;

namespace GesFer.Product.UnitTests.Handlers.PurchaseDeliveryNote;

public class CreatePurchaseDeliveryNoteCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IStockService> _mockStockService;
    private readonly CreatePurchaseDeliveryNoteCommandHandler _handler;

    public CreatePurchaseDeliveryNoteCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _mockStockService = new Mock<IStockService>();
        _handler = new CreatePurchaseDeliveryNoteCommandHandler(_context, _mockStockService.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_CreatesDeliveryNoteAndUpdatesStock()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        // Seed TaxType
        var taxType = new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Value = 21.0m,
            Name = "IVA 21%",
            Code = "IVA21"
        };
        _context.TaxTypes.Add(taxType);

        // Seed Family
        var family = new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            TaxTypeId = taxTypeId,
            Code = "FAM01",
            Name = "Familia 1"
        };
        _context.ArticleFamilies.Add(family);

        // Seed Supplier
        var supplier = new Supplier
        {
            Id = supplierId,
            CompanyId = companyId,
            Name = "Proveedor Test",
            TaxId = "B12345678",
            Address = "Calle Test 123",
            DeletedAt = null
        };
        _context.Suppliers.Add(supplier);

        // Seed Article
        var article = new Article
        {
            Id = articleId,
            CompanyId = companyId,
            ArticleFamilyId = familyId,
            Name = "Articulo Test",
            Code = "REF01",
            BuyPrice = 10.0m,
            DeletedAt = null
        };
        _context.Articles.Add(article);

        await _context.SaveChangesAsync();

        var command = new CreatePurchaseDeliveryNoteCommand
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            Date = DateTime.UtcNow,
            Reference = "ALB-001",
            Lines = new List<PurchaseDeliveryNoteLineDto>
            {
                new PurchaseDeliveryNoteLineDto
                {
                    ArticleId = articleId,
                    Quantity = 5,
                    Price = 12.0m // Override price
                }
            }
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.SupplierId.Should().Be(supplierId);
        result.Lines.Should().HaveCount(1);

        var line = result.Lines.First();
        line.ArticleId.Should().Be(articleId);
        line.Quantity.Should().Be(5);
        line.Price.Should().Be(12.0m);
        line.Subtotal.Should().Be(60.0m); // 5 * 12
        line.IvaAmount.Should().Be(12.6m); // 60 * 0.21
        line.Total.Should().Be(72.6m); // 60 + 12.6

        // Verify stock update call
        _mockStockService.Verify(s => s.ApplyStockIncrease(
            It.Is<Article>(a => a.Id == articleId),
            5), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidSupplier_ThrowsException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new CreatePurchaseDeliveryNoteCommand
        {
            CompanyId = companyId,
            SupplierId = Guid.NewGuid(), // Non-existent supplier
            Date = DateTime.UtcNow
        };

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El proveedor*no existe*");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidArticle_ThrowsException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();

        // Seed Supplier
        _context.Suppliers.Add(new Supplier
        {
            Id = supplierId,
            CompanyId = companyId,
            Name = "Proveedor Test",
            Address = "Calle Test 123"
        });
        await _context.SaveChangesAsync();

        var command = new CreatePurchaseDeliveryNoteCommand
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            Date = DateTime.UtcNow,
            Lines = new List<PurchaseDeliveryNoteLineDto>
            {
                new PurchaseDeliveryNoteLineDto
                {
                    ArticleId = Guid.NewGuid(), // Non-existent article
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El artículo*no existe*");
    }
}
