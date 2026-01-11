using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BCrypt.Net;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Resultado de la carga de datos de seed
/// </summary>
public class SeedResult
{
    public bool Loaded { get; set; }
    public List<string> Entities { get; set; } = new();
}

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
        // Prioridad: Data/Seeds/ (nueva ubicación profesional) > Seeds/ (legacy)
        var basePath = AppContext.BaseDirectory;
        var currentDir = new DirectoryInfo(basePath);
        
        string? foundPath = null;
        
        // Estrategia de búsqueda mejorada:
        // 1. Buscar Data/Seeds/ en el directorio de salida (bin/Debug/net8.0/Data/Seeds o bin/Release/net8.0/Data/Seeds)
        var dataSeedsInOutput = Path.Combine(basePath, "Data", "Seeds");
        if (Directory.Exists(dataSeedsInOutput))
        {
            foundPath = dataSeedsInOutput;
        }
        else
        {
            // 2. Buscar desde la raíz del proyecto (funciona desde consola, API y tests)
            // Estrategia: subir desde el directorio actual hasta encontrar GesFer.sln
            var searchDir = currentDir;
            var maxDepth = 10; // Limitar la profundidad de búsqueda para evitar bucles infinitos
            var depth = 0;
            
            while (searchDir != null && foundPath == null && depth < maxDepth)
            {
                depth++;
                
                // Buscar GesFer.sln en la raíz o en Api/GesFer.sln
                var solutionPathRoot = Path.Combine(searchDir.FullName, "GesFer.sln");
                var solutionPathApi = Path.Combine(searchDir.FullName, "Api", "GesFer.sln");
                var hasSolution = File.Exists(solutionPathRoot) || File.Exists(solutionPathApi);
                
                if (hasSolution)
                {
                    // Encontramos la raíz del proyecto (o Api/), buscar Api/src/Infrastructure/Data/Seeds
                    // Si GesFer.sln está en Api/, entonces searchDir ya está en la raíz
                    var rootDir = File.Exists(solutionPathRoot) ? searchDir.FullName : searchDir.FullName;
                    var projectSeedsPath = Path.Combine(rootDir, "Api", "src", "Infrastructure", "Data", "Seeds");
                    if (Directory.Exists(projectSeedsPath))
                    {
                        foundPath = projectSeedsPath;
                        break;
                    }
                    
                    // También buscar en ubicación legacy
                    var legacySeedsPath = Path.Combine(rootDir, "Api", "src", "Infrastructure", "Seeds");
                    if (Directory.Exists(legacySeedsPath))
                    {
                        foundPath = legacySeedsPath;
                        _logger.LogWarning("Usando ubicación legacy de seeds: {Path}. Se recomienda migrar a Data/Seeds/", foundPath);
                        break;
                    }
                }
                
                // Buscar directamente Api/src/Infrastructure/Data/Seeds desde cualquier punto
                var directApiSeedsPath = Path.Combine(searchDir.FullName, "Api", "src", "Infrastructure", "Data", "Seeds");
                if (Directory.Exists(directApiSeedsPath))
                {
                    foundPath = directApiSeedsPath;
                    break;
                }
                
                // Buscar directamente Api/src/Infrastructure/Seeds (legacy)
                var directApiLegacySeedsPath = Path.Combine(searchDir.FullName, "Api", "src", "Infrastructure", "Seeds");
                if (Directory.Exists(directApiLegacySeedsPath))
                {
                    foundPath = directApiLegacySeedsPath;
                    _logger.LogWarning("Usando ubicación legacy de seeds: {Path}. Se recomienda migrar a Data/Seeds/", foundPath);
                    break;
                }
                
                // Buscar Data/Seeds/ relativo al directorio actual (por si estamos en Infrastructure/Data/Seeds)
                var dataSeedsPath = Path.Combine(searchDir.FullName, "Data", "Seeds");
                if (Directory.Exists(dataSeedsPath))
                {
                    foundPath = dataSeedsPath;
                    break;
                }
                
                // Buscar Seeds/ relativo al directorio actual (ubicación legacy)
                var seedsPath = Path.Combine(searchDir.FullName, "Seeds");
                if (Directory.Exists(seedsPath))
                {
                    foundPath = seedsPath;
                    _logger.LogWarning("Usando ubicación legacy de seeds: {Path}. Se recomienda migrar a Data/Seeds/", foundPath);
                    break;
                }
                
                searchDir = searchDir.Parent;
            }
        }
        
        _seedsPath = foundPath ?? Path.Combine(basePath, "Data", "Seeds");
        
        if (!Directory.Exists(_seedsPath))
        {
            _logger.LogWarning("No se encontró la carpeta de seeds. Se usará: {Path}", _seedsPath);
            Console.WriteLine($"    ⚠ Advertencia: Carpeta de seeds no encontrada. Buscando en: {_seedsPath}");
            Console.WriteLine($"    ⚠ BaseDirectory: {basePath}");
        }
        else
        {
            _logger.LogInformation("Carpeta de seeds encontrada: {Path}", _seedsPath);
            Console.WriteLine($"    ✓ Carpeta de seeds encontrada: {_seedsPath}");
        }
    }

    /// <summary>
    /// Carga todos los datos maestros desde master-data.json
    /// </summary>
    /// <returns>Resultado con información de entidades cargadas</returns>
    public async Task<SeedResult> SeedMasterDataAsync()
    {
        var result = new SeedResult();
        var filePath = Path.Combine(_seedsPath, "master-data.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo master-data.json no encontrado en {Path}", filePath);
            return result;
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
            return result;
        }

        // Seed Languages
        if (data.Languages != null && data.Languages.Any())
        {
            await SeedLanguagesAsync(data.Languages);
            result.Entities.Add($"{data.Languages.Count} Language(s)");
        }

        // Seed Permissions
        if (data.Permissions != null && data.Permissions.Any())
        {
            await SeedPermissionsAsync(data.Permissions);
            result.Entities.Add($"{data.Permissions.Count} Permission(s)");
        }

        // Seed Groups
        if (data.Groups != null && data.Groups.Any())
        {
            await SeedGroupsAsync(data.Groups);
            result.Entities.Add($"{data.Groups.Count} Group(s)");
        }

        // Seed GroupPermissions
        if (data.GroupPermissions != null && data.GroupPermissions.Any())
        {
            await SeedGroupPermissionsAsync(data.GroupPermissions);
            result.Entities.Add($"{data.GroupPermissions.Count} GroupPermission(s)");
        }

        // Seed AdminUsers
        if (data.AdminUsers != null && data.AdminUsers.Any())
        {
            await SeedAdminUsersAsync(data.AdminUsers);
            result.Entities.Add($"{data.AdminUsers.Count} AdminUser(s)");
        }

        result.Loaded = true;
        _logger.LogInformation("Datos maestros cargados correctamente");
        return result;
    }

    /// <summary>
    /// Carga datos de demostración desde demo-data.json
    /// </summary>
    /// <returns>Resultado con información de entidades cargadas</returns>
    public async Task<SeedResult> SeedDemoDataAsync()
    {
        var result = new SeedResult();
        var filePath = Path.Combine(_seedsPath, "demo-data.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo demo-data.json no encontrado en {Path}", filePath);
            return result;
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
            return result;
        }

        // Seed Companies
        if (data.Companies != null && data.Companies.Any())
        {
            await SeedCompaniesAsync(data.Companies);
            result.Entities.Add($"{data.Companies.Count} Company(ies)");
        }

        // Seed Users
        if (data.Users != null && data.Users.Any())
        {
            await SeedUsersAsync(data.Users);
            result.Entities.Add($"{data.Users.Count} User(s)");
        }

        // Seed UserGroups
        if (data.UserGroups != null && data.UserGroups.Any())
        {
            await SeedUserGroupsAsync(data.UserGroups);
            result.Entities.Add($"{data.UserGroups.Count} UserGroup(s)");
        }

        // Seed UserPermissions
        if (data.UserPermissions != null && data.UserPermissions.Any())
        {
            await SeedUserPermissionsAsync(data.UserPermissions);
            result.Entities.Add($"{data.UserPermissions.Count} UserPermission(s)");
        }

        // Seed Families
        if (data.Families != null && data.Families.Any())
        {
            await SeedFamiliesAsync(data.Families);
            result.Entities.Add($"{data.Families.Count} Family(ies)");
        }

        // Seed Articles
        if (data.Articles != null && data.Articles.Any())
        {
            await SeedArticlesAsync(data.Articles);
            result.Entities.Add($"{data.Articles.Count} Article(s)");
        }

        // Seed Suppliers
        if (data.Suppliers != null && data.Suppliers.Any())
        {
            await SeedSuppliersAsync(data.Suppliers);
            result.Entities.Add($"{data.Suppliers.Count} Supplier(s)");
        }

        // Seed Customers
        if (data.Customers != null && data.Customers.Any())
        {
            await SeedCustomersAsync(data.Customers);
            result.Entities.Add($"{data.Customers.Count} Customer(s)");
        }

        result.Loaded = true;
        _logger.LogInformation("Datos de demostración cargados correctamente");
        return result;
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
        // Hash BCrypt fijo conocido para "admin123" (usado en tests y setup)
        // Este hash debe coincidir con el usado en SetupService y TestDataSeeder
        const string fixedAdminHash = "$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW";

        foreach (var userData in users)
        {
            var existing = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userData.Id));

            // Usar hash fijo para "admin123" para mantener consistencia con tests
            string passwordHash;
            if (userData.Password == "admin123")
            {
                passwordHash = fixedAdminHash;
            }
            else
            {
                passwordHash = BCrypt.Net.BCrypt.HashPassword(userData.Password);
            }

            if (existing == null)
            {
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
                    existing.PasswordHash = passwordHash;
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
        public List<AdminUserSeed>? AdminUsers { get; set; }
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

    private class AdminUserSeed
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = "Admin";
    }

    #endregion

    #region AdminUser Seeding

    private async Task SeedAdminUsersAsync(List<AdminUserSeed> adminUsers)
    {
        // Hash BCrypt fijo conocido para "admin123" (usado en tests y setup)
        const string fixedAdminHash = "$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW";

        foreach (var adminUserData in adminUsers)
        {
            var existing = await _context.AdminUsers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == adminUserData.Username);

            // Usar hash fijo para "admin123" para mantener consistencia
            string passwordHash;
            if (adminUserData.Password == "admin123")
            {
                passwordHash = fixedAdminHash;
            }
            else
            {
                passwordHash = BCrypt.Net.BCrypt.HashPassword(adminUserData.Password);
            }

            if (existing == null)
            {
                var adminUser = new AdminUser
                {
                    Id = Guid.Parse(adminUserData.Id),
                    Username = adminUserData.Username,
                    PasswordHash = passwordHash,
                    FirstName = adminUserData.FirstName,
                    LastName = adminUserData.LastName,
                    Email = adminUserData.Email,
                    Role = adminUserData.Role,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.AdminUsers.Add(adminUser);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                existing.PasswordHash = passwordHash;
                existing.Role = adminUserData.Role;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Actualizar hash si es necesario
                if (existing.PasswordHash != passwordHash)
                {
                    existing.PasswordHash = passwordHash;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        await _context.SaveChangesAsync();
    }

    #endregion
}
