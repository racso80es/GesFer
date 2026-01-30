using GesFer.Admin.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Linq.Expressions;

namespace MyCompany.SysAdmin.Infrastructure.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Log> Logs => Set<Log>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración manual para Log (Serilog)
        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Logs"); // Asegurar nombre de tabla
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd(); // AUTO_INCREMENT
        });

        // Configurar Sequential GUIDs
        ConfigureSequentialGuids(modelBuilder);

        // Configurar Soft Delete
        ConfigureSoftDelete(modelBuilder);
    }

    private void ConfigureSequentialGuids(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));
            if (idProperty != null && idProperty.ClrType == typeof(Guid))
            {
                // Usar SequentialGuidValueGenerator estándar de EF Core si no tenemos el custom
                idProperty.SetValueGeneratorFactory((property, entityType) => new SequentialGuidValueGenerator());
            }
        }
    }

    private void ConfigureSoftDelete(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
            var nullConstant = Expression.Constant(null, typeof(DateTime?));
            var condition = Expression.Equal(property, nullConstant);
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsActive = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.IsActive = false;
                    break;
            }
        }
    }
}
