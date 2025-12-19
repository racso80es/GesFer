using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class PostalCodeConfiguration : IEntityTypeConfiguration<PostalCode>
{
    public void Configure(EntityTypeBuilder<PostalCode> builder)
    {
        builder.ToTable("PostalCodes");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Code)
            .IsRequired()
            .HasMaxLength(20);

        // Relaciones
        builder.HasOne(pc => pc.City)
            .WithMany(c => c.PostalCodes)
            .HasForeignKey(pc => pc.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(pc => new { pc.CityId, pc.Code })
            .IsUnique();
        builder.HasIndex(pc => pc.CityId);
        builder.HasIndex(pc => pc.Code);
    }
}

