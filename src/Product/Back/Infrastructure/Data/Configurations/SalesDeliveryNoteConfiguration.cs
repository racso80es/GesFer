using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class SalesDeliveryNoteConfiguration : IEntityTypeConfiguration<SalesDeliveryNote>
{
    public void Configure(EntityTypeBuilder<SalesDeliveryNote> builder)
    {
        builder.ToTable("SalesDeliveryNotes");

        builder.HasKey(sdn => sdn.Id);

        builder.Property(sdn => sdn.Date)
            .IsRequired();

        builder.Property(sdn => sdn.Reference)
            .HasMaxLength(100);

        builder.Property(sdn => sdn.BillingStatus)
            .IsRequired()
            .HasConversion<int>();

        // CompanyId: FK a Companies (Admin); sin navegación
        builder.HasOne(sdn => sdn.Customer)
            .WithMany(c => c.SalesDeliveryNotes)
            .HasForeignKey(sdn => sdn.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sdn => sdn.SalesInvoice)
            .WithMany(si => si.SalesDeliveryNotes)
            .HasForeignKey(sdn => sdn.SalesInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(sdn => new { sdn.CompanyId, sdn.Date });
        builder.HasIndex(sdn => sdn.Reference);
    }
}

