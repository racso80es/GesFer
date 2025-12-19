using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Relaciones
        builder.HasOne(c => c.State)
            .WithMany(s => s.Cities)
            .HasForeignKey(c => c.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.PostalCodes)
            .WithOne(pc => pc.City)
            .HasForeignKey(pc => pc.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(c => new { c.StateId, c.Name })
            .IsUnique();
        builder.HasIndex(c => c.StateId);
    }
}

