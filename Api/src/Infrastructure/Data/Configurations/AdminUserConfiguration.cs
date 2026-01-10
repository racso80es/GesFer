using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUsers");

        builder.HasKey(u => u.Id);

        // Nota: Pomelo.EntityFrameworkCore.MySql mapea automáticamente Guid a CHAR(36) en MySQL
        // No es necesario especificar HasColumnType("char(36)") explícitamente.
        // El tipo Guid en C# se almacena como CHAR(36) en MySQL, optimizado para ordenación lexicográfica.

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

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastLoginIp)
            .HasMaxLength(45);

        // Índice único para Username
        builder.HasIndex(u => u.Username)
            .IsUnique();
        
        // Índice para Role
        builder.HasIndex(u => u.Role);
    }
}
