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
    private readonly CreateArticleFamilyCommandHandler _handler;

    public CreateArticleFamilyTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new CreateArticleFamilyCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateArticleFamily_WhenRequestIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        // Seed TaxType
        _context.TaxTypes.Add(new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Code = "IVA21",
            Name = "IVA General 21%",
            Value = 21.0m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

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
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Code.Should().Be("FAM01");
        result.TaxTypeId.Should().Be(taxTypeId);

        var created = await _context.ArticleFamilies.FindAsync(result.Id);
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
        _context.ArticleFamilies.Add(new ArticleFamily
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
        _context.TaxTypes.Add(new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Code = "IVA21",
            Name = "IVA General 21%",
            Value = 21.0m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await _context.SaveChangesAsync();

        var command = new CreateArticleFamilyCommand(
            new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM01",
                Name = "Nueva Familia",
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
        var command = new CreateArticleFamilyCommand(
            new CreateArticleFamilyDto
            {
                CompanyId = companyId,
                Code = "FAM01",
                Name = "Familia Test",
                TaxTypeId = Guid.NewGuid() // Non-existent TaxType
            });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*El tipo de tasa no existe*");
    }
}
