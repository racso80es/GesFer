using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
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
    public static async Task SeedTestDataAsync(ApplicationDbContext context)
    {
        // Limpiar datos existentes usando IgnoreQueryFilters para incluir soft-deleted
        var existingCompanies = await context.Companies.IgnoreQueryFilters().ToListAsync();
        var existingUsers = await context.Users.IgnoreQueryFilters().ToListAsync();
        var existingGroups = await context.Groups.IgnoreQueryFilters().ToListAsync();
        var existingPermissions = await context.Permissions.IgnoreQueryFilters().ToListAsync();
        var existingUserGroups = await context.UserGroups.IgnoreQueryFilters().ToListAsync();
        var existingUserPermissions = await context.UserPermissions.IgnoreQueryFilters().ToListAsync();
        var existingGroupPermissions = await context.GroupPermissions.IgnoreQueryFilters().ToListAsync();
        var existingSuppliers = await context.Suppliers.IgnoreQueryFilters().ToListAsync();
        var existingCustomers = await context.Customers.IgnoreQueryFilters().ToListAsync();
        var existingAdminUsers = await context.AdminUsers.IgnoreQueryFilters().ToListAsync();
        var existingAuditLogs = await context.AuditLogs.IgnoreQueryFilters().ToListAsync();
        
        context.Companies.RemoveRange(existingCompanies);
        context.Users.RemoveRange(existingUsers);
        context.Groups.RemoveRange(existingGroups);
        context.Permissions.RemoveRange(existingPermissions);
        context.UserGroups.RemoveRange(existingUserGroups);
        context.UserPermissions.RemoveRange(existingUserPermissions);
        context.GroupPermissions.RemoveRange(existingGroupPermissions);
        context.Suppliers.RemoveRange(existingSuppliers);
        context.Customers.RemoveRange(existingCustomers);
        context.AdminUsers.RemoveRange(existingAdminUsers);
        context.AuditLogs.RemoveRange(existingAuditLogs);
        await context.SaveChangesAsync();

        // Usar JsonDataSeeder para cargar datos de prueba desde test-data.json
        // Crear un logger mínimo usando LoggerFactory
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<JsonDataSeeder>();
        var jsonDataSeeder = new JsonDataSeeder(context, logger);
        await jsonDataSeeder.SeedTestDataAsync();

        // Crear usuario administrativo (AdminUser) para acceso administrativo
        // Esto se hace manualmente porque JsonDataSeeder no soporta AdminUser todavía
        var existingAdminUser = await context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == "admin");
        
        // Hash BCrypt fijo para "admin123" (mismo hash que el usuario normal)
        const string adminUserPasswordHash = "$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW";
        
        if (existingAdminUser == null)
        {
            var adminUser = new AdminUser
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
                Username = "admin",
                PasswordHash = adminUserPasswordHash,
                FirstName = "Administrador",
                LastName = "Sistema",
                Email = "admin@gesfer.local",
                Role = "Admin",
                LastLoginAt = null, // Se actualiza después del primer login
                LastLoginIp = null, // Se actualiza después del primer login
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.AdminUsers.Add(adminUser);
            await context.SaveChangesAsync();
        }
        else
        {
            // Actualizar hash del admin user existente si es necesario
            if (existingAdminUser.PasswordHash != adminUserPasswordHash)
            {
                existingAdminUser.PasswordHash = adminUserPasswordHash;
                existingAdminUser.IsActive = true;
                existingAdminUser.DeletedAt = null;
                existingAdminUser.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        // Nota: AuditLogs no se crean aquí porque son generados automáticamente
        // por el sistema cuando se realizan acciones administrativas.
        // Los tests verifican que se crean correctamente cuando se llama a DashboardController.
    }
}
