using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles", t =>
        {
            t.HasCheckConstraint("CK_Article_BuyPrice", "`BuyPrice` >= 0");
            t.HasCheckConstraint("CK_Article_SellPrice", "`SellPrice` >= 0");
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Description)
            .HasMaxLength(255);

        builder.Property(a => a.BuyPrice)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(a => a.SellPrice)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(a => a.Stock)
            .IsRequired()
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        // CompanyId: FK a tabla Companies (Admin); sin navegación en Product
        builder.HasOne(a => a.ArticleFamily)
            .WithMany()
            .HasForeignKey(a => a.ArticleFamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(a => new { a.CompanyId, a.Code })
            .IsUnique();

        builder.HasIndex(a => a.Code);
        builder.HasIndex(a => a.Name);
    }
}

