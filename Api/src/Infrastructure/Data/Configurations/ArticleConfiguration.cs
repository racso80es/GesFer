using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");

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

        // Validaciones de negocio (sintaxis MySQL)
        builder.HasCheckConstraint("CK_Article_BuyPrice", "`BuyPrice` >= 0");
        builder.HasCheckConstraint("CK_Article_SellPrice", "`SellPrice` >= 0");

        // Relaciones
        builder.HasOne(a => a.Company)
            .WithMany(c => c.Articles)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Family)
            .WithMany(f => f.Articles)
            .HasForeignKey(a => a.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(a => new { a.CompanyId, a.Code })
            .IsUnique();

        builder.HasIndex(a => a.Code);
        builder.HasIndex(a => a.Name);
    }
}

