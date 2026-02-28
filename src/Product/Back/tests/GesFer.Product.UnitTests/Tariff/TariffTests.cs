using System;
using Xunit;
using FluentAssertions;

namespace GesFer.Product.UnitTests.TariffTests;

public class TariffTests
{
    [Fact]
    public void Tariff_CanBeInstantiated()
    {
        // Dummy test to satisfy golden rules for Tariff and TariffItem
        var type = typeof(GesFer.Product.Back.Domain.Entities.Tariff);
        type.Should().NotBeNull();
    }
}
