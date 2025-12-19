using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .HasMaxLength(200);

        builder.Property(u => u.Phone)
            .HasMaxLength(50);

        builder.Property(u => u.Address)
            .HasMaxLength(500);

        // Relaciones
        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones de dirección (opcionales)
        builder.HasOne(u => u.PostalCode)
            .WithMany()
            .HasForeignKey(u => u.PostalCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.City)
            .WithMany()
            .HasForeignKey(u => u.CityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.State)
            .WithMany()
            .HasForeignKey(u => u.StateId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Country)
            .WithMany()
            .HasForeignKey(u => u.CountryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(u => new { u.CompanyId, u.Username })
            .IsUnique();
        builder.HasIndex(u => u.PostalCodeId);
        builder.HasIndex(u => u.CityId);
        builder.HasIndex(u => u.StateId);
        builder.HasIndex(u => u.CountryId);
    }
}

