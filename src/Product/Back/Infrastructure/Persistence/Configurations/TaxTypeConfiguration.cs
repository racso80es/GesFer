using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class TaxTypeConfiguration : IEntityTypeConfiguration<TaxType>
{
    public void Configure(EntityTypeBuilder<TaxType> builder)
    {
        builder.ToTable("TaxTypes", t =>
        {
            t.HasCheckConstraint("CK_TaxType_Value", "`Value` >= 0");
        });

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(255);

        builder.Property(t => t.Value)
            .IsRequired()
            .HasPrecision(18, 2); // 2 decimales para porcentaje (ej. 21.00)

        // CompanyId: FK a Companies (Admin); sin navegación
        // Índices de Unicidad por Empresa
        builder.HasIndex(t => new { t.CompanyId, t.Code })
            .IsUnique();

        builder.HasIndex(t => new { t.CompanyId, t.Name })
            .IsUnique();
    }
}
