using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BCrypt.Net;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Servicio para cargar datos de seed desde archivos JSON
/// </summary>
public class JsonDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<JsonDataSeeder> _logger;
    private readonly string _seedsPath;

    public JsonDataSeeder(
        ApplicationDbContext context,
        ILogger<JsonDataSeeder> logger)
    {
        _context = context;
        _logger = logger;

        // Obtener la ruta de los archivos de seed
        var basePath = AppContext.BaseDirectory;
        var currentDir = new DirectoryInfo(basePath);
        
        string? foundPath = null;
        
        // Buscar la carpeta Seeds
        // Primero intentar en el directorio de salida (bin/Debug/net8.0/Seeds)
        var seedsInOutput = Path.Combine(basePath, "Seeds");
        if (Directory.Exists(seedsInOutput))
        {
            foundPath = seedsInOutput;
        }
        else
        {
            // Buscar en Infrastructure/Seeds desde el código fuente
            var infrastructureDir = currentDir;
            while (infrastructureDir != null && foundPath == null)
            {
                var seedsPath = Path.Combine(infrastructureDir.FullName, "Seeds");
                if (Directory.Exists(seedsPath))
                {
                    foundPath = seedsPath;
                    break;
                }
                
                // Si estamos en bin/Debug/net8.0, subir a src/Infrastructure
                if (infrastructureDir.Name == "net8.0" && infrastructureDir.Parent?.Name == "Debug")
                {
                    var debugDir = infrastructureDir.Parent;
                    var binDir = debugDir?.Parent;
                    var netDir = binDir?.Parent;
                    var projectDir = netDir?.Parent;
                    
                    if (projectDir != null)
                    {
                        var infrastructurePath = Path.Combine(
                            projectDir.FullName, 
                            "Infrastructure", 
                            "Seeds");
                        if (Directory.Exists(infrastructurePath))
                        {
                            foundPath = infrastructurePath;
                            break;
                        }
                    }
                }
                
                infrastructureDir = infrastructureDir.Parent;
                
                // Fallback: buscar desde la raíz del proyecto
                if (infrastructureDir != null && File.Exists(Path.Combine(infrastructureDir.FullName, "GesFer.sln")))
                {
                    var fallbackPath = Path.Combine(infrastructureDir.FullName, "Api", "src", "Infrastructure", "Seeds");
                    if (Directory.Exists(fallbackPath))
                    {
                        foundPath = fallbackPath;
                        break;
                    }
                }
            }
        }
        
        _seedsPath = foundPath ?? Path.Combine(basePath, "Seeds");
    }

    /// <summary>
    /// Carga todos los datos maestros desde master-data.json
    /// </summary>
    public async Task SeedMasterDataAsync()
    {
        var filePath = Path.Combine(_seedsPath, "master-data.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo master-data.json no encontrado en {Path}", filePath);
            return;
        }

        _logger.LogInformation("Cargando datos maestros desde {Path}", filePath);
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<MasterDataSeed>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data == null)
        {
            _logger.LogError("No se pudo deserializar master-data.json");
            return;
        }

        // Seed Languages
        if (data.Languages != null)
        {
            await SeedLanguagesAsync(data.Languages);
        }

        // Seed Permissions
        if (data.Permissions != null)
        {
            await SeedPermissionsAsync(data.Permissions);
        }

        // Seed Groups
        if (data.Groups != null)
        {
            await SeedGroupsAsync(data.Groups);
        }

        // Seed GroupPermissions
        if (data.GroupPermissions != null)
        {
            await SeedGroupPermissionsAsync(data.GroupPermissions);
        }

        _logger.LogInformation("Datos maestros cargados correctamente");
    }

    /// <summary>
    /// Carga datos de demostración desde demo-data.json
    /// </summary>
    public async Task SeedDemoDataAsync()
    {
        var filePath = Path.Combine(_seedsPath, "demo-data.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo demo-data.json no encontrado en {Path}", filePath);
            return;
        }

        _logger.LogInformation("Cargando datos de demostración desde {Path}", filePath);
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<DemoDataSeed>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data == null)
        {
            _logger.LogError("No se pudo deserializar demo-data.json");
            return;
        }

        // Seed Companies
        if (data.Companies != null)
        {
            await SeedCompaniesAsync(data.Companies);
        }

        // Seed Users
        if (data.Users != null)
        {
            await SeedUsersAsync(data.Users);
        }

        // Seed UserGroups
        if (data.UserGroups != null)
        {
            await SeedUserGroupsAsync(data.UserGroups);
        }

        // Seed UserPermissions
        if (data.UserPermissions != null)
        {
            await SeedUserPermissionsAsync(data.UserPermissions);
        }

        // Seed Families
        if (data.Families != null)
        {
            await SeedFamiliesAsync(data.Families);
        }

        // Seed Articles
        if (data.Articles != null)
        {
            await SeedArticlesAsync(data.Articles);
        }

        // Seed Suppliers
        if (data.Suppliers != null)
        {
            await SeedSuppliersAsync(data.Suppliers);
        }

        // Seed Customers
        if (data.Customers != null)
        {
            await SeedCustomersAsync(data.Customers);
        }

        _logger.LogInformation("Datos de demostración cargados correctamente");
    }

    /// <summary>
    /// Carga datos de prueba desde test-data.json
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        var filePath = Path.Combine(_seedsPath, "test-data.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo test-data.json no encontrado en {Path}", filePath);
            return;
        }

        _logger.LogInformation("Cargando datos de prueba desde {Path}", filePath);
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<TestDataSeed>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data == null)
        {
            _logger.LogError("No se pudo deserializar test-data.json");
            return;
        }

        // Seed Companies
        if (data.Companies != null)
        {
            await SeedCompaniesAsync(data.Companies);
        }

        // Seed Users
        if (data.Users != null)
        {
            await SeedUsersAsync(data.Users);
        }

        // Seed Groups
        if (data.Groups != null)
        {
            await SeedGroupsAsync(data.Groups);
        }

        // Seed Permissions
        if (data.Permissions != null)
        {
            await SeedPermissionsAsync(data.Permissions);
        }

        // Seed UserGroups
        if (data.UserGroups != null)
        {
            await SeedUserGroupsAsync(data.UserGroups);
        }

        // Seed GroupPermissions
        if (data.GroupPermissions != null)
        {
            await SeedGroupPermissionsAsync(data.GroupPermissions);
        }

        _logger.LogInformation("Datos de prueba cargados correctamente");
    }

    #region Private Seed Methods

    private async Task SeedLanguagesAsync(List<LanguageSeed> languages)
    {
        foreach (var langData in languages)
        {
            var existing = await _context.Languages
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Code == langData.Code);

            if (existing == null)
            {
                var lang = new Language
                {
                    Id = Guid.Parse(langData.Id),
                    Name = langData.Name,
                    Code = langData.Code,
                    Description = langData.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Languages.Add(lang);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedPermissionsAsync(List<PermissionSeed> permissions)
    {
        foreach (var permData in permissions)
        {
            var existing = await _context.Permissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Key == permData.Key);

            if (existing == null)
            {
                var perm = new Permission
                {
                    Id = Guid.Parse(permData.Id),
                    Key = permData.Key,
                    Description = permData.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Permissions.Add(perm);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedGroupsAsync(List<GroupSeed> groups)
    {
        foreach (var groupData in groups)
        {
            var existing = await _context.Groups
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(g => g.Name == groupData.Name);

            if (existing == null)
            {
                var group = new Group
                {
                    Id = Guid.Parse(groupData.Id),
                    Name = groupData.Name,
                    Description = groupData.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Groups.Add(group);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedGroupPermissionsAsync(List<GroupPermissionSeed> groupPermissions)
    {
        foreach (var gpData in groupPermissions)
        {
            var existing = await _context.GroupPermissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(gp => 
                    gp.GroupId == Guid.Parse(gpData.GroupId) && 
                    gp.PermissionId == Guid.Parse(gpData.PermissionId));

            if (existing == null)
            {
                // Verificar también por ID para evitar conflictos de tracking
                var existingById = await _context.GroupPermissions
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(gp => gp.Id == Guid.Parse(gpData.Id));

                if (existingById == null)
                {
                    var gp = new GroupPermission
                    {
                        Id = Guid.Parse(gpData.Id),
                        GroupId = Guid.Parse(gpData.GroupId),
                        PermissionId = Guid.Parse(gpData.PermissionId),
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.GroupPermissions.Add(gp);
                }
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedCompaniesAsync(List<CompanySeed> companies)
    {
        foreach (var companyData in companies)
        {
            var existing = await _context.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == Guid.Parse(companyData.Id));

            if (existing == null)
            {
                var company = new Company
                {
                    Id = Guid.Parse(companyData.Id),
                    Name = companyData.Name,
                    TaxId = companyData.TaxId,
                    Address = companyData.Address,
                    Phone = companyData.Phone,
                    Email = companyData.Email,
                    LanguageId = Guid.Parse(companyData.LanguageId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Companies.Add(company);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync(List<UserSeed> users)
    {
        foreach (var userData in users)
        {
            var existing = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userData.Id));

            if (existing == null)
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(userData.Password);
                var user = new User
                {
                    Id = Guid.Parse(userData.Id),
                    CompanyId = Guid.Parse(userData.CompanyId),
                    Username = userData.Username,
                    PasswordHash = passwordHash,
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    Email = userData.Email,
                    Phone = userData.Phone,
                    LanguageId = Guid.Parse(userData.LanguageId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Users.Add(user);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                // Actualizar password hash si es necesario
                if (!string.IsNullOrEmpty(userData.Password))
                {
                    existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userData.Password);
                }
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedUserGroupsAsync(List<UserGroupSeed> userGroups)
    {
        foreach (var ugData in userGroups)
        {
            var existing = await _context.UserGroups
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(ug => 
                    ug.UserId == Guid.Parse(ugData.UserId) && 
                    ug.GroupId == Guid.Parse(ugData.GroupId));

            if (existing == null)
            {
                var ug = new UserGroup
                {
                    Id = Guid.Parse(ugData.Id),
                    UserId = Guid.Parse(ugData.UserId),
                    GroupId = Guid.Parse(ugData.GroupId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.UserGroups.Add(ug);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedUserPermissionsAsync(List<UserPermissionSeed> userPermissions)
    {
        foreach (var upData in userPermissions)
        {
            var existing = await _context.UserPermissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(up => 
                    up.UserId == Guid.Parse(upData.UserId) && 
                    up.PermissionId == Guid.Parse(upData.PermissionId));

            if (existing == null)
            {
                var up = new UserPermission
                {
                    Id = Guid.Parse(upData.Id),
                    UserId = Guid.Parse(upData.UserId),
                    PermissionId = Guid.Parse(upData.PermissionId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.UserPermissions.Add(up);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedFamiliesAsync(List<FamilySeed> families)
    {
        foreach (var familyData in families)
        {
            var existing = await _context.Families
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == Guid.Parse(familyData.Id));

            if (existing == null)
            {
                var family = new Family
                {
                    Id = Guid.Parse(familyData.Id),
                    CompanyId = Guid.Parse(familyData.CompanyId),
                    Name = familyData.Name,
                    Description = familyData.Description,
                    IvaPercentage = familyData.IvaPercentage,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Families.Add(family);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedArticlesAsync(List<ArticleSeed> articles)
    {
        foreach (var articleData in articles)
        {
            var existing = await _context.Articles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == Guid.Parse(articleData.Id));

            if (existing == null)
            {
                var article = new Article
                {
                    Id = Guid.Parse(articleData.Id),
                    CompanyId = Guid.Parse(articleData.CompanyId),
                    FamilyId = Guid.Parse(articleData.FamilyId),
                    Code = articleData.Code,
                    Name = articleData.Name,
                    Description = articleData.Description,
                    BuyPrice = articleData.BuyPrice,
                    SellPrice = articleData.SellPrice,
                    Stock = articleData.Stock,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Articles.Add(article);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedSuppliersAsync(List<SupplierSeed> suppliers)
    {
        foreach (var supplierData in suppliers)
        {
            var existing = await _context.Suppliers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == Guid.Parse(supplierData.Id));

            if (existing == null)
            {
                var supplier = new Supplier
                {
                    Id = Guid.Parse(supplierData.Id),
                    CompanyId = Guid.Parse(supplierData.CompanyId),
                    Name = supplierData.Name,
                    TaxId = supplierData.TaxId,
                    Address = supplierData.Address,
                    Phone = supplierData.Phone,
                    Email = supplierData.Email,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Suppliers.Add(supplier);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedCustomersAsync(List<CustomerSeed> customers)
    {
        foreach (var customerData in customers)
        {
            var existing = await _context.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == Guid.Parse(customerData.Id));

            if (existing == null)
            {
                var customer = new Customer
                {
                    Id = Guid.Parse(customerData.Id),
                    CompanyId = Guid.Parse(customerData.CompanyId),
                    Name = customerData.Name,
                    TaxId = customerData.TaxId,
                    Address = customerData.Address,
                    Phone = customerData.Phone,
                    Email = customerData.Email,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Customers.Add(customer);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Seed Data Models

    private class MasterDataSeed
    {
        public List<LanguageSeed>? Languages { get; set; }
        public List<PermissionSeed>? Permissions { get; set; }
        public List<GroupSeed>? Groups { get; set; }
        public List<GroupPermissionSeed>? GroupPermissions { get; set; }
    }

    private class DemoDataSeed
    {
        public List<CompanySeed>? Companies { get; set; }
        public List<UserSeed>? Users { get; set; }
        public List<UserGroupSeed>? UserGroups { get; set; }
        public List<UserPermissionSeed>? UserPermissions { get; set; }
        public List<FamilySeed>? Families { get; set; }
        public List<ArticleSeed>? Articles { get; set; }
        public List<SupplierSeed>? Suppliers { get; set; }
        public List<CustomerSeed>? Customers { get; set; }
    }

    private class TestDataSeed
    {
        public List<CompanySeed>? Companies { get; set; }
        public List<UserSeed>? Users { get; set; }
        public List<GroupSeed>? Groups { get; set; }
        public List<PermissionSeed>? Permissions { get; set; }
        public List<UserGroupSeed>? UserGroups { get; set; }
        public List<GroupPermissionSeed>? GroupPermissions { get; set; }
    }

    private class LanguageSeed
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private class PermissionSeed
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private class GroupSeed
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private class GroupPermissionSeed
    {
        public string Id { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string PermissionId { get; set; } = string.Empty;
    }

    private class CompanySeed
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string LanguageId { get; set; } = string.Empty;
    }

    private class UserSeed
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string LanguageId { get; set; } = string.Empty;
    }

    private class UserGroupSeed
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
    }

    private class UserPermissionSeed
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string PermissionId { get; set; } = string.Empty;
    }

    private class FamilySeed
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal IvaPercentage { get; set; }
    }

    private class ArticleSeed
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string FamilyId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal Stock { get; set; }
    }

    private class SupplierSeed
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    private class CustomerSeed
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    #endregion
}
