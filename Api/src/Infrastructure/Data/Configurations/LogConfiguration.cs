using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("Logs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Level)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(l => l.Exception)
            .HasMaxLength(10000);

        builder.Property(l => l.Properties)
            .HasColumnType("TEXT");

        builder.Property(l => l.Source)
            .HasMaxLength(500);

        builder.Property(l => l.Timestamp)
            .IsRequired();

        builder.Property(l => l.ClientInfo)
            .HasColumnType("TEXT");

        // Índices para mejorar el rendimiento de consultas
        builder.HasIndex(l => l.Level);
        builder.HasIndex(l => l.Timestamp);
        builder.HasIndex(l => l.CompanyId);
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => new { l.Level, l.Timestamp });
        builder.HasIndex(l => new { l.CompanyId, l.Timestamp });
    }
}
