using FluentAssertions;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Common;
using Xunit;

namespace GesFer.Product.UnitTests.Invoices;

public class PurchaseInvoiceTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act
        var invoice = new PurchaseInvoice
        {
            Id = id,
            CompanyId = companyId,
            InvoiceNumber = "INV-001",
            Date = date,
            Total = 100m
        };

        // Assert
        invoice.Id.Should().Be(id);
        invoice.CompanyId.Should().Be(companyId);
        invoice.InvoiceNumber.Should().Be("INV-001");
        invoice.Total.Should().Be(100m);
        invoice.PaymentStatus.Should().Be(PaymentStatus.Pending);
        invoice.PurchaseDeliveryNotes.Should().BeEmpty();
    }
}
