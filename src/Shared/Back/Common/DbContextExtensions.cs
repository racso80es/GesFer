using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GesFer.Shared.Back.Domain.Common;

public static class DbContextExtensions
{
    /// <summary>
    /// Configures sequential GUID generation for all entities inheriting from BaseEntity.
    /// Uses SequentialGuidValueGenerator to optimize index performance.
    /// </summary>
    public static void ConfigureSequentialGuids(this ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));

            if (idProperty != null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.SetValueGeneratorFactory((property, entityType) => new SequentialGuidValueGenerator());
                idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
            }
        }
    }

    /// <summary>
    /// Configures global Soft Delete filter for all entities inheriting from BaseEntity.
    /// </summary>
    public static void ConfigureSoftDelete(this ModelBuilder modelBuilder)
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

    /// <summary>
    /// Updates audit fields (CreatedAt, UpdatedAt, DeletedAt, IsActive) based on entity state.
    /// Handles Soft Delete logic when state is Deleted.
    /// </summary>
    public static void UpdateAuditFields(this ChangeTracker changeTracker)
    {
        var entries = changeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = Guid.NewGuid(); // Fallback if generator hasn't run yet or manual
                    }
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsActive = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    // Soft Delete
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.IsActive = false;
                    break;
            }
        }
    }
}
