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

        // Índices
        builder.HasIndex(s => new { s.CompanyId, s.Name });
    }
}

