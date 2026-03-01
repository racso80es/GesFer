import sys

taxtype = """using FluentAssertions;
using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Handlers.TaxTypes;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.TaxTypes;

public class CreateTaxTypeTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly CreateTaxTypeCommandHandler _handler;

    public CreateTaxTypeTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _handler = new CreateTaxTypeCommandHandler(_contextMock.Object);
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

        var taxTypes = new List<TaxType>();
        var taxTypesDbSetMock = taxTypes.BuildMockDbSet();
        taxTypesDbSetMock.Setup(d => d.Add(It.IsAny<TaxType>())).Callback<TaxType>(taxTypes.Add);
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypesDbSetMock.Object);

        var id = await _handler.HandleAsync(command);

        id.Should().NotBe(Guid.Empty);
        taxTypes.Should().ContainSingle();
        var created = taxTypes.First();
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
        var existingTax = new TaxType
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = "IVA21",
            Name = "Existente",
            Value = 21,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var taxTypes = new List<TaxType> { existingTax };
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

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

        var taxTypes = new List<TaxType>();
        _contextMock.Setup(c => c.TaxTypes).Returns(taxTypes.BuildMockDbSet().Object);

        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*valor*");
    }
}
"""
with open('src/Product/Back/tests/GesFer.Product.UnitTests/TaxTypes/CreateTaxTypeTests.cs', 'w') as f:
    f.write(taxtype)
