using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.LanguageId)
            .IsRequired();

        builder.HasOne(c => c.Language)
            .WithMany(l => l.Countries)
            .HasForeignKey(c => c.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones
        builder.HasMany(c => c.States)
            .WithOne(s => s.Country)
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(c => c.Code)
            .IsUnique();
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.LanguageId);
    }
}

