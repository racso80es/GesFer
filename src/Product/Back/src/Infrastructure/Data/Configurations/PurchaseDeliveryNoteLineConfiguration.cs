using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class PurchaseDeliveryNoteLineConfiguration : IEntityTypeConfiguration<PurchaseDeliveryNoteLine>
{
    public void Configure(EntityTypeBuilder<PurchaseDeliveryNoteLine> builder)
    {
        builder.ToTable("PurchaseDeliveryNoteLines", t =>
        {
            t.HasCheckConstraint("CK_PurchaseDeliveryNoteLine_Quantity", "`Quantity` > 0");
            t.HasCheckConstraint("CK_PurchaseDeliveryNoteLine_Price", "`Price` >= 0");
        });

        builder.HasKey(pdnl => pdnl.Id);

        builder.Property(pdnl => pdnl.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(pdnl => pdnl.Price)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(pdnl => pdnl.Subtotal)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(pdnl => pdnl.IvaAmount)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(pdnl => pdnl.Total)
            .IsRequired()
            .HasPrecision(18, 4);

        // Relaciones
        builder.HasOne(pdnl => pdnl.PurchaseDeliveryNote)
            .WithMany(pdn => pdn.Lines)
            .HasForeignKey(pdnl => pdnl.PurchaseDeliveryNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pdnl => pdnl.Article)
            .WithMany(a => a.PurchaseDeliveryNoteLines)
            .HasForeignKey(pdnl => pdnl.ArticleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(pdnl => pdnl.PurchaseDeliveryNoteId);
    }
}

