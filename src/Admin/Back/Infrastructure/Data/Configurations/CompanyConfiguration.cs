using GesFer.Shared.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Admin.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
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

    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

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
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(200)
            .HasConversion(
                email => email.HasValue ? email.Value.Value : null,
                value => ConvertStringToEmail(value));

        // Relaciones de dirección (opcionales)
        // NOTA: Como estas entidades están en Shared, podemos mapearlas
        // pero debemos asegurarnos de que AdminDbContext las incluya o las ignore si no se usan
        builder.Ignore(c => c.PostalCode);
        builder.Ignore(c => c.City);
        builder.Ignore(c => c.State);
        builder.Ignore(c => c.Country);
        builder.Ignore(c => c.Language);

        // Índices
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.PostalCodeId);
        builder.HasIndex(c => c.CityId);
        builder.HasIndex(c => c.StateId);
        builder.HasIndex(c => c.CountryId);
        builder.HasIndex(c => c.LanguageId);
    }
}
