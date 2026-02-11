using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Reflection;

namespace GesFer.Shared.Back.Infrastructure.Persistence;

public static class DbContextExtensions
{
    public static void ConfigureSharedEntities(this ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            // Sequential GUIDs
            var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));
            if (idProperty != null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.SetValueGeneratorFactory((property, entityType) => new SequentialGuidValueGenerator());
                idProperty.ValueGenerated = ValueGenerated.OnAdd;
            }

            // Soft Delete
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
            var nullConstant = Expression.Constant(null, typeof(DateTime?));
            var condition = Expression.Equal(property, nullConstant);
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public static void UpdateSharedAuditFields(this ChangeTracker changeTracker)
    {
        var entries = changeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
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
