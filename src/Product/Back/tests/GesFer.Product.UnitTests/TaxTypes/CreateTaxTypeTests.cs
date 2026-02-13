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
    private readonly CreateTaxTypeCommandHandler _handler;

    public CreateTaxTypeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
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
        var created = await _context.TaxTypes.FindAsync(id);
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

        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CompanyId*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCodeAlreadyExists()
    {
        var companyId = Guid.NewGuid();
        _context.TaxTypes.Add(new TaxType
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = "IVA21",
            Name = "Existente",
            Value = 21,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new CreateTaxTypeCommand(
            new CreateTaxTypeDto
            {
                Code = "IVA21",
                Name = "Otro",
                Value = 21
            },
            companyId);

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

        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*valor*");
    }
}
