using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
        builder.ToTable("PurchaseInvoices");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pi => pi.Date)
            .IsRequired();

        builder.Property(pi => pi.Subtotal)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(pi => pi.IvaAmount)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(pi => pi.Total)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(pi => pi.PaymentStatus)
            .IsRequired()
            .HasConversion<int>();

        // Relaciones
        builder.HasOne(pi => pi.Company)
            .WithMany()
            .HasForeignKey(pi => pi.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(pi => new { pi.CompanyId, pi.InvoiceNumber })
            .IsUnique();

        builder.HasIndex(pi => pi.InvoiceNumber);
    }
}

