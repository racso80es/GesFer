using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class PurchaseDeliveryNoteConfiguration : IEntityTypeConfiguration<PurchaseDeliveryNote>
{
    public void Configure(EntityTypeBuilder<PurchaseDeliveryNote> builder)
    {
        builder.ToTable("PurchaseDeliveryNotes");

        builder.HasKey(pdn => pdn.Id);

        builder.Property(pdn => pdn.Date)
            .IsRequired();

        builder.Property(pdn => pdn.Reference)
            .HasMaxLength(100);

        builder.Property(pdn => pdn.BillingStatus)
            .IsRequired()
            .HasConversion<int>();

        // Relaciones
        builder.HasOne(pdn => pdn.Company)
            .WithMany()
            .HasForeignKey(pdn => pdn.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pdn => pdn.Supplier)
            .WithMany(s => s.PurchaseDeliveryNotes)
            .HasForeignKey(pdn => pdn.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pdn => pdn.PurchaseInvoice)
            .WithMany(pi => pi.PurchaseDeliveryNotes)
            .HasForeignKey(pdn => pdn.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(pdn => new { pdn.CompanyId, pdn.Date });
        builder.HasIndex(pdn => pdn.Reference);
    }
}

