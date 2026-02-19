using FluentAssertions;
using GesFer.Product.Back.Domain.Entities;
using Xunit;

namespace GesFer.Product.UnitTests.Tariffs;

public class TariffTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var name = "General Tariff";

        // Act
        var tariff = new Tariff
        {
            Id = id,
            CompanyId = companyId,
            Name = name,
            Type = TariffType.Sell
        };

        // Assert
        tariff.Id.Should().Be(id);
        tariff.CompanyId.Should().Be(companyId);
        tariff.Name.Should().Be(name);
        tariff.Type.Should().Be(TariffType.Sell);
        tariff.TariffItems.Should().BeEmpty();
    }
}
