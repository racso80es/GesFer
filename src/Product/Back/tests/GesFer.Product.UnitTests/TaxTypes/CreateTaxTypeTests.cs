using FluentAssertions;
using FluentValidation.TestHelper;
using GesFer.Product.Application.Commands.TaxTypes;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Product.Application.Handlers.TaxTypes;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GesFer.Product.UnitTests.TaxTypes;

public class CreateTaxTypeTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly CreateTaxTypeCommandHandler _handler;
    private readonly CreateTaxTypeValidator _validator;

    public CreateTaxTypeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _userContextMock = new Mock<IUserContext>();
        _userContextMock.Setup(x => x.CompanyId).Returns(Guid.NewGuid());

        _handler = new CreateTaxTypeCommandHandler(_context, _userContextMock.Object);
        _validator = new CreateTaxTypeValidator();
    }

    [Fact]
    public async Task Handle_ShouldCreateTaxType_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateTaxTypeCommand(new CreateTaxTypeDto
        {
            Code = "IVA21",
            Name = "IVA General 21%",
            Value = 21.0m
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var created = await _context.TaxTypes.FindAsync(result.Value);
        created.Should().NotBeNull();
        created!.Code.Should().Be("IVA21");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenCodeIsEmpty()
    {
        var model = new CreateTaxTypeCommand(new CreateTaxTypeDto { Code = "" });
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.TaxType.Code);
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenValueIsNegative()
    {
        var model = new CreateTaxTypeCommand(new CreateTaxTypeDto { Value = -1 });
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.TaxType.Value);
    }
}
