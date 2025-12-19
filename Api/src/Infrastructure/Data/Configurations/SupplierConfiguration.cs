using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.TaxId)
            .HasMaxLength(50);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.Phone)
            .HasMaxLength(50);

        builder.Property(s => s.Email)
            .HasMaxLength(200);

        // Relaciones
        builder.HasOne(s => s.Company)
            .WithMany(c => c.Suppliers)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.BuyTariff)
            .WithMany(t => t.Suppliers)
            .HasForeignKey(s => s.BuyTariffId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relaciones de dirección (opcionales)
        builder.HasOne(s => s.PostalCode)
            .WithMany()
            .HasForeignKey(s => s.PostalCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.City)
            .WithMany()
            .HasForeignKey(s => s.CityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.State)
            .WithMany()
            .HasForeignKey(s => s.StateId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Country)
            .WithMany()
            .HasForeignKey(s => s.CountryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(s => new { s.CompanyId, s.Name });
        builder.HasIndex(s => s.PostalCodeId);
        builder.HasIndex(s => s.CityId);
        builder.HasIndex(s => s.StateId);
        builder.HasIndex(s => s.CountryId);
    }
}

