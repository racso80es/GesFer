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

        // Id como INT AUTO_INCREMENT para compatibilidad con Serilog.Sinks.MySQL
        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();

        builder.Property(l => l.Level)
            .IsRequired(false) // NULL permitido para compatibilidad con Serilog.Sinks.MySQL
            .HasMaxLength(128); // Aumentado a 128 para compatibilidad con Serilog.Sinks.MySQL

        builder.Property(l => l.Message)
            .IsRequired(false) // NULL permitido para compatibilidad con Serilog.Sinks.MySQL
            .HasColumnType("longtext");

        builder.Property(l => l.Template)
            .HasColumnName("Template")
            .HasColumnType("longtext");

        builder.Property(l => l.Exception)
            .HasColumnType("longtext");

        builder.Property(l => l.Properties)
            .HasColumnType("longtext");

        builder.Property(l => l.Source)
            .HasMaxLength(500);

        // TimeStamp con mayúscula para compatibilidad con Serilog
        builder.Property(l => l.TimeStamp)
            .IsRequired()
            .HasColumnName("TimeStamp");

        builder.Property(l => l.ClientInfo)
            .HasColumnType("longtext");

        // Índices para mejorar el rendimiento de consultas
        builder.HasIndex(l => l.Level);
        builder.HasIndex(l => l.TimeStamp);
        builder.HasIndex(l => l.CompanyId);
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => new { l.Level, l.TimeStamp });
        builder.HasIndex(l => new { l.CompanyId, l.TimeStamp });
    }
}
