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
