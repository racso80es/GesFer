using GesFer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesFer.Infrastructure.Data.Configurations;

public class GroupPermissionConfiguration : IEntityTypeConfiguration<GroupPermission>
{
    public void Configure(EntityTypeBuilder<GroupPermission> builder)
    {
        builder.ToTable("GroupPermissions");

        builder.HasKey(gp => gp.Id);

        // Propiedades
        builder.Property(gp => gp.GroupId)
            .IsRequired();

        builder.Property(gp => gp.PermissionId)
            .IsRequired();

        // Relaciones
        builder.HasOne(gp => gp.Group)
            .WithMany(g => g.GroupPermissions)
            .HasForeignKey(gp => gp.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gp => gp.Permission)
            .WithMany(p => p.GroupPermissions)
            .HasForeignKey(gp => gp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único para evitar duplicados
        builder.HasIndex(gp => new { gp.GroupId, gp.PermissionId })
            .IsUnique();
    }
}

