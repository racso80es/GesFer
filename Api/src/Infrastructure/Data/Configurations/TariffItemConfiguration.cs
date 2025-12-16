using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class TariffItemConfiguration : IEntityTypeConfiguration<TariffItem>
{
    public void Configure(EntityTypeBuilder<TariffItem> builder)
    {
        builder.ToTable("TariffItems");

        builder.HasKey(ti => ti.Id);

        builder.Property(ti => ti.Price)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.HasCheckConstraint("CK_TariffItem_Price", "`Price` >= 0");

        // Relaciones
        builder.HasOne(ti => ti.Tariff)
            .WithMany(t => t.TariffItems)
            .HasForeignKey(ti => ti.TariffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ti => ti.Article)
            .WithMany(a => a.TariffItems)
            .HasForeignKey(ti => ti.ArticleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice único para evitar duplicados (un artículo solo puede tener un precio por tarifa)
        builder.HasIndex(ti => new { ti.TariffId, ti.ArticleId })
            .IsUnique();
    }
}

