using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GesFer.IntegrationTests.Helpers;

/// <summary>
/// Clase helper para insertar datos de prueba en la base de datos
/// </summary>
public static class TestDataSeeder
{
    /// <summary>
    /// Inserta datos de prueba en la base de datos desde test-data.json
    /// </summary>
    public static async Task SeedTestDataAsync(ApplicationDbContext context, IConfiguration? configuration = null)
    {
        // Companies: SSOT en Admin; Product no borra Companies. Limpiar solo datos de Product.
        var existingUsers = await context.Users.IgnoreQueryFilters().ToListAsync();
        var existingGroups = await context.Groups.IgnoreQueryFilters().ToListAsync();
        var existingPermissions = await context.Permissions.IgnoreQueryFilters().ToListAsync();
        var existingUserGroups = await context.UserGroups.IgnoreQueryFilters().ToListAsync();
        var existingUserPermissions = await context.UserPermissions.IgnoreQueryFilters().ToListAsync();
        var existingGroupPermissions = await context.GroupPermissions.IgnoreQueryFilters().ToListAsync();
        var existingSuppliers = await context.Suppliers.IgnoreQueryFilters().ToListAsync();
        var existingCustomers = await context.Customers.IgnoreQueryFilters().ToListAsync();
        
        context.Users.RemoveRange(existingUsers);
        context.Groups.RemoveRange(existingGroups);
        context.Permissions.RemoveRange(existingPermissions);
        context.UserGroups.RemoveRange(existingUserGroups);
        context.UserPermissions.RemoveRange(existingUserPermissions);
        context.GroupPermissions.RemoveRange(existingGroupPermissions);
        context.Suppliers.RemoveRange(existingSuppliers);
        context.Customers.RemoveRange(existingCustomers);
        await context.SaveChangesAsync();

        // Usar JsonDataSeeder para cargar datos de prueba desde test-data.json
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<JsonDataSeeder>();
        var sanitizer = new GesFer.Shared.Back.Domain.Services.SensitiveDataSanitizer();
        var config = configuration ?? new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:CompanyId"] = "11111111-1111-1111-1111-111111111115" })
            .Build();
        var jsonDataSeeder = new JsonDataSeeder(context, logger, sanitizer, config);
        await jsonDataSeeder.SeedTestDataAsync();

        // Nota: AuditLogs no se crean aquí porque son generados automáticamente
        // por el sistema cuando se realizan acciones administrativas.
        // Los tests verifican que se crean correctamente cuando se llama a DashboardController.
    }
}
