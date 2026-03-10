using FluentAssertions;
using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Handlers.TaxTypes;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.TaxTypes;

public class UpdateTaxTypeTests
{
    private readonly ApplicationDbContext _context;
    private readonly UpdateTaxTypeCommandHandler _handler;

    public UpdateTaxTypeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new UpdateTaxTypeCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateTaxType_WhenValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        _context.TaxTypes.Add(new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Code = "IVA21",
            Name = "IVA 21%",
            Value = 21,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new UpdateTaxTypeCommand(new UpdateTaxTypeDto
        {
            Id = taxTypeId,
            Code = "IVA21",
            Name = "IVA 21% Updated",
            Description = "Updated description",
            Value = 21
        }, companyId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var updated = await _context.TaxTypes.FindAsync(taxTypeId);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("IVA 21% Updated");
        updated.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCompanyIdIsEmpty()
    {
        var command = new UpdateTaxTypeCommand(new UpdateTaxTypeDto(), null);

        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CompanyId*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenTaxTypeNotFound()
    {
        var command = new UpdateTaxTypeCommand(new UpdateTaxTypeDto { Id = Guid.NewGuid() }, Guid.NewGuid());

        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tipo de impuesto no encontrado*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCodeAlreadyExists()
    {
        var companyId = Guid.NewGuid();
        var taxTypeId1 = Guid.NewGuid();
        var taxTypeId2 = Guid.NewGuid();

        _context.TaxTypes.AddRange(
            new TaxType { Id = taxTypeId1, CompanyId = companyId, Code = "IVA10", Name = "IVA 10%", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = taxTypeId2, CompanyId = companyId, Code = "IVA21", Name = "IVA 21%", Value = 21, CreatedAt = DateTime.UtcNow, IsActive = true }
        );
        await _context.SaveChangesAsync();

        var command = new UpdateTaxTypeCommand(new UpdateTaxTypeDto
        {
            Id = taxTypeId1,
            Code = "IVA21", // Changing code to existing one
            Name = "IVA 10%",
            Value = 10
        }, companyId);

        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe un tipo de impuesto con este código*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenNameAlreadyExists()
    {
        var companyId = Guid.NewGuid();
        var taxTypeId1 = Guid.NewGuid();
        var taxTypeId2 = Guid.NewGuid();

        _context.TaxTypes.AddRange(
            new TaxType { Id = taxTypeId1, CompanyId = companyId, Code = "IVA10", Name = "IVA 10%", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TaxType { Id = taxTypeId2, CompanyId = companyId, Code = "IVA21", Name = "IVA 21%", Value = 21, CreatedAt = DateTime.UtcNow, IsActive = true }
        );
        await _context.SaveChangesAsync();

        var command = new UpdateTaxTypeCommand(new UpdateTaxTypeDto
        {
            Id = taxTypeId1,
            Code = "IVA10",
            Name = "IVA 21%", // Changing name to existing one
            Value = 10
        }, companyId);

        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe un tipo de impuesto con este nombre*");
    }

    [Theory]
    [InlineData("", "Name", 10, "El código es obligatorio y máximo 10 caracteres.")]
    [InlineData("12345678901", "Name", 10, "El código es obligatorio y máximo 10 caracteres.")]
    [InlineData("Code", "", 10, "El nombre es obligatorio y máximo 50 caracteres.")]
    [InlineData("Code", "123456789012345678901234567890123456789012345678901", 10, "El nombre es obligatorio y máximo 50 caracteres.")]
    [InlineData("Code", "Name", -1, "El valor debe ser mayor o igual a 0.")]
    public async Task HandleAsync_ShouldThrow_WhenValidationFails(string code, string name, decimal value, string expectedMessage)
    {
        var companyId = Guid.NewGuid();
        var taxTypeId = Guid.NewGuid();

        _context.TaxTypes.Add(new TaxType
        {
            Id = taxTypeId,
            CompanyId = companyId,
            Code = "IVA21",
            Name = "IVA 21%",
            Value = 21,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new UpdateTaxTypeCommand(new UpdateTaxTypeDto
        {
            Id = taxTypeId,
            Code = code,
            Name = name,
            Value = value
        }, companyId);

        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{expectedMessage}*");
    }
}
