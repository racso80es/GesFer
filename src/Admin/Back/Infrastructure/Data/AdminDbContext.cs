using GesFer.Admin.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Shared.Back.Infrastructure.Persistence;
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

        // Configurar Shared Entities (Sequential GUIDs + Soft Delete)
        modelBuilder.ConfigureSharedEntities();
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
