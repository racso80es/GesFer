using FluentAssertions;
using GesFer.Application.Commands.SalesDeliveryNote;
using GesFer.Application.Handlers.SalesDeliveryNote;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

// Alias to resolve ambiguity
using SalesDeliveryNoteEntity = GesFer.Product.Back.Domain.Entities.SalesDeliveryNote;

namespace GesFer.Product.UnitTests.Handlers.SalesDeliveryNote;

public class CreateSalesDeliveryNoteCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IStockService> _mockStockService;
    private readonly CreateSalesDeliveryNoteCommandHandler _handler;

    public CreateSalesDeliveryNoteCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _mockStockService = new Mock<IStockService>();
        _handler = new CreateSalesDeliveryNoteCommandHandler(_context, _mockStockService.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_CreatesDeliveryNoteAndDecreasesStock()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        // Seed TaxType
        _context.TaxTypes.Add(new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Value = 21.0m,
            Name = "IVA 21%",
            Code = "IVA21"
        });

        // Seed Family
        _context.ArticleFamilies.Add(new ArticleFamily
        {
            Id = familyId,
            CompanyId = companyId,
            TaxTypeId = taxTypeId,
            Code = "FAM01",
            Name = "Familia 1"
        });

        // Seed Customer
        _context.Customers.Add(new Customer
        {
            Id = customerId,
            CompanyId = companyId,
            Name = "Cliente Test",
            TaxId = "A08001851", // Valid CIF
            Address = "Calle Test 123"
        });

        // Seed Article with sufficient stock
        var article = new Article
        {
            Id = articleId,
            CompanyId = companyId,
            ArticleFamilyId = familyId,
            Name = "Articulo Test",
            Code = "REF01",
            SellPrice = 20.0m,
            Stock = 10,
            DeletedAt = null
        };
        _context.Articles.Add(article);

        await _context.SaveChangesAsync();

        var command = new CreateSalesDeliveryNoteCommand
        {
            CompanyId = companyId,
            CustomerId = customerId,
            Date = DateTime.UtcNow,
            Reference = "SALES-001",
            Lines = new List<SalesDeliveryNoteLineDto>
            {
                new SalesDeliveryNoteLineDto
                {
                    ArticleId = articleId,
                    Quantity = 5,
                    Price = 22.0m // Override price
                }
            }
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        result.Lines.Should().HaveCount(1);

        var line = result.Lines.First();
        line.ArticleId.Should().Be(articleId);
        line.Quantity.Should().Be(5);
        line.Price.Should().Be(22.0m);
        line.Subtotal.Should().Be(110.0m); // 5 * 22
        line.IvaAmount.Should().Be(23.1m); // 110 * 0.21
        line.Total.Should().Be(133.1m); // 110 + 23.1

        // Verify stock decrease call
        _mockStockService.Verify(s => s.ApplyStockDecrease(
            It.Is<Article>(a => a.Id == articleId),
            5), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        // Seed dependencies
        _context.TaxTypes.Add(new TaxType { Id = taxTypeId, CompanyId = companyId, Value = 21.0m, Name = "IVA", Code = "IVA" });
        _context.ArticleFamilies.Add(new ArticleFamily { Id = familyId, CompanyId = companyId, TaxTypeId = taxTypeId, Code = "F1", Name = "Fam" });
        _context.Customers.Add(new Customer { Id = customerId, CompanyId = companyId, Name = "C1", Address = "A1" });

        // Seed Article with INSUFFICIENT stock
        _context.Articles.Add(new Article
        {
            Id = articleId,
            CompanyId = companyId,
            ArticleFamilyId = familyId,
            Name = "Articulo Stock Bajo",
            Code = "REF02",
            SellPrice = 20.0m,
            Stock = 2 // Only 2 in stock
        });

        await _context.SaveChangesAsync();

        var command = new CreateSalesDeliveryNoteCommand
        {
            CompanyId = companyId,
            CustomerId = customerId,
            Date = DateTime.UtcNow,
            Lines = new List<SalesDeliveryNoteLineDto>
            {
                new SalesDeliveryNoteLineDto
                {
                    ArticleId = articleId,
                    Quantity = 5 // Requesting 5
                }
            }
        };

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Stock insuficiente*");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCustomer_ThrowsException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new CreateSalesDeliveryNoteCommand
        {
            CompanyId = companyId,
            CustomerId = Guid.NewGuid(), // Non-existent customer
            Date = DateTime.UtcNow
        };

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El cliente*no existe*");
    }
}
