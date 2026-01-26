using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class SalesDeliveryNoteLineConfiguration : IEntityTypeConfiguration<SalesDeliveryNoteLine>
{
    public void Configure(EntityTypeBuilder<SalesDeliveryNoteLine> builder)
    {
        builder.ToTable("SalesDeliveryNoteLines", t =>
        {
            t.HasCheckConstraint("CK_SalesDeliveryNoteLine_Quantity", "`Quantity` > 0");
            t.HasCheckConstraint("CK_SalesDeliveryNoteLine_Price", "`Price` >= 0");
        });

        builder.HasKey(sdnl => sdnl.Id);

        builder.Property(sdnl => sdnl.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(sdnl => sdnl.Price)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(sdnl => sdnl.Subtotal)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(sdnl => sdnl.IvaAmount)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(sdnl => sdnl.Total)
            .IsRequired()
            .HasPrecision(18, 4);

        // Relaciones
        builder.HasOne(sdnl => sdnl.SalesDeliveryNote)
            .WithMany(sdn => sdn.Lines)
            .HasForeignKey(sdnl => sdnl.SalesDeliveryNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sdnl => sdnl.Article)
            .WithMany(a => a.SalesDeliveryNoteLines)
            .HasForeignKey(sdnl => sdnl.ArticleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(sdnl => sdnl.SalesDeliveryNoteId);
    }
}

