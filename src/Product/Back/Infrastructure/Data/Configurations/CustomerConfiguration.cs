using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    private static TaxId? ConvertStringToTaxId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return TaxId.TryCreate(value, out var taxId) ? taxId : (TaxId?)null;
    }

    private static Email? ConvertStringToEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Email.TryCreate(value, out var email) ? email : (Email?)null;
    }
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.TaxId)
            .HasMaxLength(50)
            .HasConversion(
                taxId => taxId.HasValue ? taxId.Value.Value : null,
                value => ConvertStringToTaxId(value));

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(200)
            .HasConversion(
                email => email.HasValue ? email.Value.Value : null,
                value => ConvertStringToEmail(value));

        // CompanyId: FK a Companies (Admin); sin navegación
        builder.HasOne(c => c.SellTariff)
            .WithMany(t => t.Customers)
            .HasForeignKey(c => c.SellTariffId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relaciones de dirección (opcionales)
        builder.HasOne(c => c.PostalCode)
            .WithMany()
            .HasForeignKey(c => c.PostalCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.City)
            .WithMany()
            .HasForeignKey(c => c.CityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.State)
            .WithMany()
            .HasForeignKey(c => c.StateId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Country)
            .WithMany()
            .HasForeignKey(c => c.CountryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(c => new { c.CompanyId, c.Name });
        builder.HasIndex(c => c.PostalCodeId);
        builder.HasIndex(c => c.CityId);
        builder.HasIndex(c => c.StateId);
        builder.HasIndex(c => c.CountryId);
    }
}

