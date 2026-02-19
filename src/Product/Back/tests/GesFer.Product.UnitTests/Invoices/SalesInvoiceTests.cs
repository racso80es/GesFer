using FluentAssertions;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Common;
using Xunit;

namespace GesFer.Product.UnitTests.Invoices;

public class SalesInvoiceTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act
        var invoice = new SalesInvoice
        {
            Id = id,
            CompanyId = companyId,
            InvoiceNumber = "SINV-001",
            Date = date,
            Total = 200m
        };

        // Assert
        invoice.Id.Should().Be(id);
        invoice.CompanyId.Should().Be(companyId);
        invoice.InvoiceNumber.Should().Be("SINV-001");
        invoice.Total.Should().Be(200m);
        invoice.PaymentStatus.Should().Be(PaymentStatus.Pending);
        invoice.SalesDeliveryNotes.Should().BeEmpty();
    }
}
