using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class ArticleFamilyConfiguration : IEntityTypeConfiguration<ArticleFamily>
{
    public void Configure(EntityTypeBuilder<ArticleFamily> builder)
    {
        builder.ToTable("ArticleFamilies");

        builder.HasKey(af => af.Id);

        builder.Property(af => af.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(af => af.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(af => af.Description)
            .HasMaxLength(500);

        // CompanyId: FK a Companies (Admin); sin navegación
        builder.HasOne(af => af.TaxType)
            .WithMany()
            .HasForeignKey(af => af.TaxTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(af => new { af.CompanyId, af.Code })
            .IsUnique();
    }
}
