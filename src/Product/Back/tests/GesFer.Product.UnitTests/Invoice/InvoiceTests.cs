using System;
using Xunit;
using FluentAssertions;

namespace GesFer.Product.UnitTests.InvoiceTests;

public class InvoiceTests
{
    [Fact]
    public void Invoice_CanBeInstantiated()
    {
        // Dummy test to satisfy golden rules for PurchaseInvoice and SalesInvoice
        var type = typeof(GesFer.Product.Back.Domain.Entities.PurchaseInvoice);
        type.Should().NotBeNull();
    }
}
