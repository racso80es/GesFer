using FluentAssertions;
using Xunit;

namespace GesFer.Product.UnitTests.DeliveryNote;

/// <summary>
/// Kaizen: documenta y protege la fórmula de IVA usada en albaranes (venta/compra).
/// Los handlers usan: ivaAmount = subtotal * (article.ArticleFamily.TaxType.Value / 100).
/// </summary>
public class DeliveryNoteIvaCalculationTests
{
    /// <summary>
    /// Fórmula alineada con CreateSalesDeliveryNoteCommandHandler y CreatePurchaseDeliveryNoteCommandHandler.
    /// </summary>
    private static decimal IvaAmount(decimal subtotal, decimal taxTypeValuePercent) => subtotal * (taxTypeValuePercent / 100);

    [Fact]
    public void IvaAmount_ForSubtotal100_AndTax21_ShouldBe21()
    {
        IvaAmount(100m, 21m).Should().Be(21m);
    }

    [Fact]
    public void IvaAmount_ForSubtotal50_AndTax10_ShouldBe5()
    {
        IvaAmount(50m, 10m).Should().Be(5m);
    }

    [Fact]
    public void IvaAmount_ForTax0_ShouldBe0()
    {
        IvaAmount(100m, 0m).Should().Be(0m);
    }
}
