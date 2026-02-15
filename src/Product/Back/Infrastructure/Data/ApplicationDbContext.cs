using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<GesFer.Product.Back.Domain.Entities.Company> Companies => Set<GesFer.Product.Back.Domain.Entities.Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<TaxType> TaxTypes => Set<TaxType>();
    public DbSet<ArticleFamily> ArticleFamilies => Set<ArticleFamily>();
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
    public DbSet<GesFer.Shared.Back.Domain.Entities.Country> Countries => Set<GesFer.Shared.Back.Domain.Entities.Country>();
    public DbSet<GesFer.Shared.Back.Domain.Entities.Language> Languages => Set<GesFer.Shared.Back.Domain.Entities.Language>();
    public DbSet<GesFer.Shared.Back.Domain.Entities.State> States => Set<GesFer.Shared.Back.Domain.Entities.State>();
    public DbSet<GesFer.Shared.Back.Domain.Entities.City> Cities => Set<GesFer.Shared.Back.Domain.Entities.City>();
    public DbSet<GesFer.Shared.Back.Domain.Entities.PostalCode> PostalCodes => Set<GesFer.Shared.Back.Domain.Entities.PostalCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones de entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configurar Shared Entities (Sequential GUIDs + Soft Delete)
        modelBuilder.ConfigureSharedEntities();

        // Configurar UTF8 para MySQL
        ConfigureUtf8(modelBuilder);
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
        ChangeTracker.UpdateSharedAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.UpdateSharedAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }
}

