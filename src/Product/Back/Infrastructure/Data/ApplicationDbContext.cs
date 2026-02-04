using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// DbContext principal de la aplicación con soporte para Soft Delete
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets - Solo entidades del dominio Product
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<TariffItem> TariffItems => Set<TariffItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PurchaseDeliveryNote> PurchaseDeliveryNotes => Set<PurchaseDeliveryNote>();
    public DbSet<PurchaseDeliveryNoteLine> PurchaseDeliveryNoteLines => Set<PurchaseDeliveryNoteLine>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<SalesDeliveryNote> SalesDeliveryNotes => Set<SalesDeliveryNote>();
    public DbSet<SalesDeliveryNoteLine> SalesDeliveryNoteLines => Set<SalesDeliveryNoteLine>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<PostalCode> PostalCodes => Set<PostalCode>();
    // NOTA: AuditLog, Log y AdminUser se gestionan en el dominio Admin

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones de entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configurar Sequential GUIDs para todas las entidades que heredan de BaseEntity
        ConfigureSequentialGuids(modelBuilder);

        // Configurar Soft Delete global para todas las entidades que heredan de BaseEntity
        ConfigureSoftDelete(modelBuilder);

        // Configurar UTF8 para MySQL
        ConfigureUtf8(modelBuilder);
    }

    /// <summary>
    /// Configura el generador de GUIDs secuenciales para todas las propiedades Id de tipo Guid
    /// en entidades que heredan de BaseEntity.
    /// 
    /// Esto mejora el rendimiento de los índices agrupados al reducir la fragmentación
    /// y permitir un mejor ordenamiento natural por fecha de creación.
    /// 
    /// Usa inversión de dependencias para soportar múltiples proveedores de BD (MySQL, SQL Server, PostgreSQL).
    /// </summary>
    private void ConfigureSequentialGuids(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            // Buscar la propiedad Id de tipo Guid
            var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));
            
            if (idProperty != null && idProperty.ClrType == typeof(Guid))
            {
                // Configurar el ValueGenerator secuencial
                // El ServiceProvider se resolverá en el método Next() del ValueGenerator desde el EntityEntry
                idProperty.SetValueGeneratorFactory((property, entityType) => new SequentialGuidValueGenerator());
                idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
            }
        }
    }

    /// <summary>
    /// Configura el filtro de Soft Delete globalmente
    /// </summary>
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

    /// <summary>
    /// Configura UTF8 para todas las columnas de tipo string que no tengan una configuración explícita de tipo.
    /// No sobrescribe configuraciones explícitas como longtext, TEXT, etc.
    /// </summary>
    private void ConfigureUtf8(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            var properties = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(string));

            foreach (var property in properties)
            {
                // Solo configurar varchar si no hay una configuración explícita de tipo de columna
                // Las configuraciones explícitas (como longtext, TEXT) tienen prioridad
                var storeType = property.GetColumnType();
                if (string.IsNullOrEmpty(storeType) || storeType == "nvarchar(max)" || storeType == "varchar(max)")
                {
                    // MySQL usa utf8mb4_unicode_ci por defecto si se configura en el servidor
                    // Pero podemos forzarlo aquí también para propiedades sin configuración explícita
                    // No establecemos varchar sin longitud, solo si hay HasMaxLength configurado
                    var maxLength = property.GetMaxLength();
                    if (maxLength.HasValue)
                    {
                        // Si tiene MaxLength, usar varchar con esa longitud
                        property.SetColumnType($"varchar({maxLength.Value})");
                    }
                    // Si no tiene MaxLength y no tiene tipo explícito, dejar que EF Core use su configuración por defecto
                }
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

    /// <summary>
    /// Actualiza automáticamente los campos de auditoría (CreatedAt, UpdatedAt, DeletedAt)
    /// </summary>
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
                    // Soft Delete
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.IsActive = false;
                    break;
            }
        }
    }
}

