using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CursorId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.HttpMethod)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.AdditionalData)
            .HasMaxLength(2000);

        builder.Property(a => a.ActionTimestamp)
            .IsRequired();

        // Índices para mejorar las consultas de auditoría
        builder.HasIndex(a => a.CursorId);
        builder.HasIndex(a => a.Username);
        builder.HasIndex(a => a.ActionTimestamp);
        builder.HasIndex(a => new { a.CursorId, a.ActionTimestamp });
    }
}
