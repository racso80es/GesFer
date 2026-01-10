using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace GesFer.IntegrationTests.Helpers;

/// <summary>
/// Clase helper para insertar datos de prueba en la base de datos
/// </summary>
public static class TestDataSeeder
{
    /// <summary>
    /// Inserta datos de prueba en la base de datos
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

        // Idiomas maestros
        var languages = new[]
        {
            new Language { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Español", Code = "es", Description = "Español", CreatedAt = DateTime.UtcNow, IsActive = true },
            new Language { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "English", Code = "en", Description = "Inglés", CreatedAt = DateTime.UtcNow, IsActive = true },
            new Language { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Català", Code = "ca", Description = "Catalán", CreatedAt = DateTime.UtcNow, IsActive = true }
        };
        foreach (var lang in languages)
        {
            if (!await context.Languages.IgnoreQueryFilters().AnyAsync(l => l.Id == lang.Id))
            {
                context.Languages.Add(lang);
            }
        }
        await context.SaveChangesAsync();

        // Crear empresa
        // Nota: En tests, las direcciones completas son opcionales, pero se incluyen para mantener consistencia con SetupService
        var company = new Company
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Empresa Demo",
            TaxId = "B12345678",
            Address = "Calle Demo 123",
            Phone = "912345678",
            Email = "demo@empresa.com",
            LanguageId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            // Campos de dirección (opcionales en tests)
            PostalCodeId = null,
            CityId = null,
            StateId = null,
            CountryId = null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Companies.Add(company);

        // Crear grupo
        var group = new Group
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Administradores",
            Description = "Grupo de administradores del sistema",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Groups.Add(group);

        // Crear permisos
        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Key = "users.read",
                Description = "Ver usuarios",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Permission
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Key = "users.write",
                Description = "Crear/editar usuarios",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Permission
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Key = "articles.read",
                Description = "Ver artículos",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };
        context.Permissions.AddRange(permissions);

        // Asignar permisos al grupo
        var groupPermissions = new List<GroupPermission>
        {
            new GroupPermission
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                GroupId = group.Id,
                PermissionId = permissions[0].Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new GroupPermission
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                GroupId = group.Id,
                PermissionId = permissions[1].Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };
        context.GroupPermissions.AddRange(groupPermissions);

        // Crear usuario
        // Nota: En tests, las direcciones completas son opcionales, pero se incluyen para mantener consistencia con SetupService
        var user = new User
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            CompanyId = company.Id,
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", BCrypt.Net.BCrypt.GenerateSalt(11)),
            FirstName = "Administrador",
            LastName = "Sistema",
            Email = "admin@empresa.com",
            Phone = "912345678",
            LanguageId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            // Campos de dirección (opcionales en tests)
            Address = null,
            PostalCodeId = null,
            CityId = null,
            StateId = null,
            CountryId = null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Users.Add(user);

        // Asignar usuario al grupo
        var userGroup = new UserGroup
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            UserId = user.Id,
            GroupId = group.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.UserGroups.Add(userGroup);

        // Asignar permiso directo al usuario
        var userPermission = new UserPermission
        {
            Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            UserId = user.Id,
            PermissionId = permissions[2].Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.UserPermissions.Add(userPermission);

        // Crear proveedores de prueba
        // Nota: En tests, las direcciones completas y tarifas son opcionales, pero se incluyen para mantener consistencia con SetupService
        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                Id = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
                CompanyId = company.Id,
                Name = "Proveedor Test 1",
                TaxId = "B11111111",
                Address = "Calle Proveedor 1",
                Phone = "911111111",
                Email = "proveedor1@test.com",
                // Campos de dirección y tarifa (opcionales en tests)
                BuyTariffId = null,
                PostalCodeId = null,
                CityId = null,
                StateId = null,
                CountryId = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Supplier
            {
                Id = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"),
                CompanyId = company.Id,
                Name = "Proveedor Test 2",
                TaxId = "B22222222",
                Address = "Calle Proveedor 2",
                Phone = "922222222",
                Email = "proveedor2@test.com",
                // Campos de dirección y tarifa (opcionales en tests)
                BuyTariffId = null,
                PostalCodeId = null,
                CityId = null,
                StateId = null,
                CountryId = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };
        context.Suppliers.AddRange(suppliers);

        // Crear clientes de prueba
        // Nota: En tests, las direcciones completas y tarifas son opcionales, pero se incluyen para mantener consistencia con SetupService
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.Parse("cccccccc-1111-1111-1111-111111111111"),
                CompanyId = company.Id,
                Name = "Cliente Test 1",
                TaxId = "B33333333",
                Address = "Calle Cliente 1",
                Phone = "933333333",
                Email = "cliente1@test.com",
                // Campos de dirección y tarifa (opcionales en tests)
                SellTariffId = null,
                PostalCodeId = null,
                CityId = null,
                StateId = null,
                CountryId = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Customer
            {
                Id = Guid.Parse("dddddddd-2222-2222-2222-222222222222"),
                CompanyId = company.Id,
                Name = "Cliente Test 2",
                TaxId = "B44444444",
                Address = "Calle Cliente 2",
                Phone = "944444444",
                Email = "cliente2@test.com",
                // Campos de dirección y tarifa (opcionales en tests)
                SellTariffId = null,
                PostalCodeId = null,
                CityId = null,
                StateId = null,
                CountryId = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };
        context.Customers.AddRange(customers);

        // Crear usuario administrativo para tests
        // Nota: Este AdminUser se usa en los tests de AdminAuthController y DashboardController
        var adminUser = new AdminUser
        {
            Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", BCrypt.Net.BCrypt.GenerateSalt(11)),
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

        // Nota: AuditLogs no se crean aquí porque son generados automáticamente
        // por el sistema cuando se realizan acciones administrativas.
        // Los tests verifican que se crean correctamente cuando se llama a DashboardController.

        await context.SaveChangesAsync();
    }
}

