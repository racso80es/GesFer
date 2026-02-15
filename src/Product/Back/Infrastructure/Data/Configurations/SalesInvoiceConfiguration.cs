using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.ToTable("SalesInvoices");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(si => si.Date)
            .IsRequired();

        builder.Property(si => si.Subtotal)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(si => si.IvaAmount)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(si => si.Total)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(si => si.PaymentStatus)
            .IsRequired()
            .HasConversion<int>();

        // CompanyId: FK a Companies (Admin); sin navegación
        // Índices
        builder.HasIndex(si => new { si.CompanyId, si.InvoiceNumber })
            .IsUnique();

        builder.HasIndex(si => si.InvoiceNumber);
    }
}

