using FluentAssertions;
using GesFer.Shared.Back.Domain.ValueObjects;
using Xunit;

namespace GesFer.Shared.Back.UnitTests.ValueObjects;

public class TaxIdTests
{
    [Theory]
    [InlineData("B87654315")] // "Empresa Test Update" from test-data.json
    [InlineData("B87654323")] // "Empresa Demo" from master-data.json
    public void Create_WithValidCIF_ShouldSucceed(string validCif)
    {
        // Act
        var taxId = TaxId.Create(validCif);

        // Assert
        taxId.Value.Should().Be(validCif);
        taxId.Type.Should().Be(TaxIdType.CIF);
    }

    [Theory]
    [InlineData("INVALID123")]
    [InlineData("B12345678")] // Check digit mismatch (8 vs expected 5 for B1234567) -> 1234567: Even(2,4,6), Odd(1,3,5,7).
                               // 1: 2->2. 2: 2. 3: 6->6. 4: 4. 5: 10->1. 6: 6. 7: 14->5.
                               // Odd: 2+6+1+5=14. Even: 2+4+6=12. Total: 26. Unit: 6. Check: 4.
                               // So B12345674 would be valid. B12345678 is invalid.
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCIF_ShouldThrowArgumentException(string invalidCif)
    {
        // Act
        Action act = () => TaxId.Create(invalidCif);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("12345678Z")]
    public void Create_WithValidNIF_ShouldSucceed(string validNif)
    {
         // Act
        var taxId = TaxId.Create(validNif);

        // Assert
        taxId.Value.Should().Be(validNif);
        taxId.Type.Should().Be(TaxIdType.NIF);
    }

    [Theory]
    [InlineData("X1234567L")]
    public void Create_WithValidNIE_ShouldSucceed(string validNie)
    {
         // Act
        var taxId = TaxId.Create(validNie);

        // Assert
        taxId.Value.Should().Be(validNie);
        taxId.Type.Should().Be(TaxIdType.NIE);
    }
}
