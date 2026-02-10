using GesFer.Admin.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GesFer.Admin.Infrastructure.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Log> Logs => Set<Log>();
    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones de entidades (incluyendo CompanyConfiguration)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminDbContext).Assembly);

        // Configuración manual para Log (Serilog)
        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Logs"); // Asegurar nombre de tabla
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        // Configurar Sequential GUIDs
        ConfigureSequentialGuids(modelBuilder);

        // Configurar Soft Delete
        ConfigureSoftDelete(modelBuilder);
    }

    private void ConfigureSequentialGuids(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));
                if (idProperty != null && idProperty.ClrType == typeof(Guid))
                {
                    idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
                }
            }
        }
    }

    private void ConfigureSoftDelete(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTime?));
                var condition = Expression.Equal(property, nullConstant);
                var lambda = Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
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
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = Guid.NewGuid(); // Fallback si no se genera
                    }
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
