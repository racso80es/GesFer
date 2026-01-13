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

        // Orden jerárquico EXACTO para evitar errores de Foreign Key:
        // Este orden es CRÍTICO y no debe cambiarse sin revisar todas las dependencias
        
        // 1. Languages (sin dependencias) - DEBE ejecutarse primero
        if (data.Languages != null && data.Languages.Any())
        {
            await SeedLanguagesAsync(data.Languages);
            // Guardar cambios explícitamente para asegurar que Languages estén disponibles
            await _context.SaveChangesAsync();
            _logger.LogInformation("Languages sembrados: {Count}", data.Languages.Count);
        }

        // 2. Countries (depende de Languages) - DEBE ejecutarse después de Languages
        if (data.Countries != null && data.Countries.Any())
        {
            // Validar que todos los LanguageId referenciados existen
            var countryLanguageIds = data.Countries.Select(c => Guid.Parse(c.LanguageId)).Distinct().ToList();
            var existingCountryLanguages = await _context.Languages
                .IgnoreQueryFilters()
                .Where(l => countryLanguageIds.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync();
            
            var missingCountryLanguages = countryLanguageIds.Except(existingCountryLanguages).ToList();
            if (missingCountryLanguages.Any())
            {
                _logger.LogError("Error de integridad referencial: Los siguientes LanguageId no existen para Countries: {MissingIds}", 
                    string.Join(", ", missingCountryLanguages));
                throw new InvalidOperationException(
                    $"No se pueden insertar Countries: Los siguientes LanguageId no existen en la base de datos: {string.Join(", ", missingCountryLanguages)}");
            }
            
            await SeedCountriesAsync(data.Countries);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Countries sembrados: {Count}", data.Countries.Count);
        }

        // 3. Cities (depende de Countries/States) - DEBE ejecutarse después de Countries
        if (data.Cities != null && data.Cities.Any())
        {
            await SeedCitiesAsync(data.Cities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cities sembrados: {Count}", data.Cities.Count);
        }

        // 4. Companies (depende de Languages) - DEBE ejecutarse después de Languages
        if (data.Companies != null && data.Companies.Any())
        {
            // Validar que todos los LanguageId referenciados existen
            var languageIds = data.Companies.Select(c => Guid.Parse(c.LanguageId)).Distinct().ToList();
            var existingLanguages = await _context.Languages
                .IgnoreQueryFilters()
                .Where(l => languageIds.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync();
            
            var missingLanguages = languageIds.Except(existingLanguages).ToList();
            if (missingLanguages.Any())
            {
                _logger.LogError("Error de integridad referencial: Los siguientes LanguageId no existen: {MissingIds}", 
                    string.Join(", ", missingLanguages));
                throw new InvalidOperationException(
                    $"No se pueden insertar Companies: Los siguientes LanguageId no existen en la base de datos: {string.Join(", ", missingLanguages)}");
            }
            
            await SeedCompaniesAsync(data.Companies);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Companies sembrados: {Count}", data.Companies.Count);
        }

        // 5. Users (depende de Companies y Languages) - DEBE ejecutarse después de Companies
        if (data.Users != null && data.Users.Any())
        {
            // Validar que todos los CompanyId y LanguageId referenciados existen
            var companyIds = data.Users.Select(u => Guid.Parse(u.CompanyId)).Distinct().ToList();
            var userLanguageIds = data.Users.Select(u => Guid.Parse(u.LanguageId)).Distinct().ToList();
            
            var existingCompanies = await _context.Companies
                .IgnoreQueryFilters()
                .Where(c => companyIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();
            
            var existingUserLanguages = await _context.Languages
                .IgnoreQueryFilters()
                .Where(l => userLanguageIds.Contains(l.Id))
                .Select(l => l.Id)
                .ToListAsync();
            
            var missingCompanies = companyIds.Except(existingCompanies).ToList();
            var missingUserLanguages = userLanguageIds.Except(existingUserLanguages).ToList();
            
            if (missingCompanies.Any() || missingUserLanguages.Any())
            {
                var errors = new List<string>();
                if (missingCompanies.Any())
                    errors.Add($"CompanyId no existen: {string.Join(", ", missingCompanies)}");
                if (missingUserLanguages.Any())
                    errors.Add($"LanguageId no existen: {string.Join(", ", missingUserLanguages)}");
                
                _logger.LogError("Error de integridad referencial: {Errors}", string.Join("; ", errors));
                throw new InvalidOperationException(
                    $"No se pueden insertar Users: {string.Join("; ", errors)}");
            }
            
            await SeedUsersAsync(data.Users);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Users sembrados: {Count}", data.Users.Count);
        }

        // 6. Groups (sin dependencias)
        if (data.Groups != null && data.Groups.Any())
        {
            await SeedGroupsAsync(data.Groups);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Groups sembrados: {Count}", data.Groups.Count);
        }

        // 7. Permissions (sin dependencias)
        if (data.Permissions != null && data.Permissions.Any())
        {
            await SeedPermissionsAsync(data.Permissions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Permissions sembrados: {Count}", data.Permissions.Count);
        }

        // 8. UserGroups (depende de Users y Groups) - DEBE ejecutarse después de Users y Groups
        if (data.UserGroups != null && data.UserGroups.Any())
        {
            await SeedUserGroupsAsync(data.UserGroups);
            await _context.SaveChangesAsync();
            _logger.LogInformation("UserGroups sembrados: {Count}", data.UserGroups.Count);
        }

        // 9. GroupPermissions (depende de Groups y Permissions) - DEBE ejecutarse después de Groups y Permissions
        if (data.GroupPermissions != null && data.GroupPermissions.Any())
        {
            await SeedGroupPermissionsAsync(data.GroupPermissions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("GroupPermissions sembrados: {Count}", data.GroupPermissions.Count);
        }

        // 10. UserPermissions (depende de Users y Permissions) - DEBE ejecutarse después de Users y Permissions
        // CRÍTICO: SaveChangesAsync ya se ejecutó después de Users (línea 427) y Permissions (línea 443)
        if (data.UserPermissions != null && data.UserPermissions.Any())
        {
            await SeedUserPermissionsAsync(data.UserPermissions);
            await _context.SaveChangesAsync();
            _logger.LogInformation("UserPermissions sembrados: {Count}", data.UserPermissions.Count);
        }

        // 11. AdminUsers (sin dependencias) - Puede ejecutarse en cualquier momento
        if (data.AdminUsers != null && data.AdminUsers.Any())
        {
            await SeedAdminUsersAsync(data.AdminUsers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("AdminUsers sembrados: {Count}", data.AdminUsers.Count);
        }

        // 12. Suppliers (depende de Companies) - DEBE ejecutarse después de Companies
        if (data.Suppliers != null && data.Suppliers.Any())
        {
            await SeedSuppliersAsync(data.Suppliers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Suppliers sembrados: {Count}", data.Suppliers.Count);
        }

        // 13. Customers (depende de Companies) - DEBE ejecutarse después de Companies
        if (data.Customers != null && data.Customers.Any())
        {
            await SeedCustomersAsync(data.Customers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Customers sembrados: {Count}", data.Customers.Count);
        }

        _logger.LogInformation("Datos de prueba cargados correctamente");
        Console.WriteLine("Datos de prueba cargados correctamente");
        
        // CRÍTICO: Limpiar el ChangeTracker para forzar a EF Core a consultar la base de datos real
        // en lugar de usar objetos en memoria. Esto asegura que los datos sembrados estén disponibles
        // para las consultas posteriores en los tests.
        _context.ChangeTracker.Clear();
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
                _logger.LogInformation("[SEED] Cargado registro específico para test: Language '{Name}' (Code: {Code}, Id: {Id})", 
                    langData.Name, langData.Code, langData.Id);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                _logger.LogInformation("[SEED] Reactivado registro existente: Language '{Name}' (Code: {Code}, Id: {Id})", 
                    langData.Name, langData.Code, langData.Id);
            }
        }
        // NOTA: SaveChangesAsync se llama explícitamente en SeedTestDataAsync después de SeedLanguagesAsync
        // para garantizar persistencia inmediata y evitar problemas de concurrencia
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
                _logger.LogInformation("[SEED] Cargado registro específico para test: Permission '{Key}' (Id: {Id})", 
                    permData.Key, permData.Id);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                _logger.LogInformation("[SEED] Reactivado registro existente: Permission '{Key}' (Id: {Id})", 
                    permData.Key, permData.Id);
            }
        }
        // NOTA: SaveChangesAsync se llama explícitamente en SeedTestDataAsync después de SeedPermissionsAsync
        // para garantizar persistencia inmediata y evitar problemas de concurrencia
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
                _logger.LogInformation("[SEED] Cargado registro específico para test: Group '{Name}' (Id: {Id})", 
                    groupData.Name, groupData.Id);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                _logger.LogInformation("[SEED] Reactivado registro existente: Group '{Name}' (Id: {Id})", 
                    groupData.Name, groupData.Id);
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedGroupPermissionsAsync(List<GroupPermissionSeed> groupPermissions)
    {
        // CRÍTICO: Validar explícitamente que Groups y Permissions existen antes de insertar GroupPermissions
        var groupIds = groupPermissions.Select(gp => Guid.Parse(gp.GroupId)).Distinct().ToList();
        var permissionIds = groupPermissions.Select(gp => Guid.Parse(gp.PermissionId)).Distinct().ToList();
        
        // Verificar que todos los GroupId existen en el contexto local
        var existingGroups = await _context.Groups
            .IgnoreQueryFilters()
            .Where(g => groupIds.Contains(g.Id))
            .Select(g => g.Id)
            .ToListAsync();
        
        var missingGroups = groupIds.Except(existingGroups).ToList();
        if (missingGroups.Any())
        {
            _logger.LogError("Error de integridad referencial: Los siguientes GroupId no existen para GroupPermissions: {MissingIds}", 
                string.Join(", ", missingGroups));
            throw new InvalidOperationException(
                $"No se pueden insertar GroupPermissions: Los siguientes GroupId no existen en la base de datos: {string.Join(", ", missingGroups)}");
        }
        
        // Verificar que todos los PermissionId existen en el contexto local
        var existingPermissions = await _context.Permissions
            .IgnoreQueryFilters()
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();
        
        var missingPermissions = permissionIds.Except(existingPermissions).ToList();
        if (missingPermissions.Any())
        {
            _logger.LogError("Error de integridad referencial: Los siguientes PermissionId no existen para GroupPermissions: {MissingIds}", 
                string.Join(", ", missingPermissions));
            throw new InvalidOperationException(
                $"No se pueden insertar GroupPermissions: Los siguientes PermissionId no existen en la base de datos: {string.Join(", ", missingPermissions)}");
        }
        
        _logger.LogInformation("Validación exitosa: {GroupCount} grupos y {PermissionCount} permisos encontrados para GroupPermissions", 
            existingGroups.Count, existingPermissions.Count);
        
        // Ahora insertar GroupPermissions con la garantía de que las FK existen
        foreach (var gpData in groupPermissions)
        {
            var groupId = Guid.Parse(gpData.GroupId);
            var permissionId = Guid.Parse(gpData.PermissionId);
            
            // Verificación adicional por si acaso
            var groupExists = existingGroups.Contains(groupId);
            var permissionExists = existingPermissions.Contains(permissionId);
            
            if (!groupExists || !permissionExists)
            {
                _logger.LogError("Error crítico: GroupId={GroupId} existe={GroupExists}, PermissionId={PermissionId} existe={PermissionExists}", 
                    groupId, groupExists, permissionId, permissionExists);
                continue; // Saltar este registro
            }
            
            var existing = await _context.GroupPermissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(gp => 
                    gp.GroupId == groupId && 
                    gp.PermissionId == permissionId);

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
                        GroupId = groupId,
                        PermissionId = permissionId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.GroupPermissions.Add(gp);
                    _logger.LogDebug("GroupPermission añadido: GroupId={GroupId}, PermissionId={PermissionId}", groupId, permissionId);
                }
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                _logger.LogDebug("GroupPermission reactivado: GroupId={GroupId}, PermissionId={PermissionId}", groupId, permissionId);
            }
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation("GroupPermissions sembrados: {Count}", groupPermissions.Count);
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
                _logger.LogInformation("[SEED] Cargado registro específico para test: Company '{Name}' (Id: {Id})", 
                    companyData.Name, companyData.Id);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                _logger.LogInformation("[SEED] Reactivado registro existente: Company '{Name}' (Id: {Id})", 
                    companyData.Name, companyData.Id);
            }
        }
        // NOTA: SaveChangesAsync se llama explícitamente en SeedTestDataAsync después de SeedCompaniesAsync
        // para garantizar persistencia inmediata y evitar problemas de concurrencia
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
                _logger.LogInformation("[SEED] Cargado registro específico para test: User '{Username}' (Id: {Id})", 
                    userData.Username, userData.Id);
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
                _logger.LogInformation("[SEED] Reactivado registro existente: User '{Username}' (Id: {Id})", 
                    userData.Username, userData.Id);
            }
        }
        // NOTA: SaveChangesAsync se llama explícitamente en SeedTestDataAsync después de SeedUsersAsync
        // para garantizar persistencia inmediata y evitar problemas de concurrencia
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
        // CRÍTICO: Validar explícitamente que Users y Permissions existen antes de insertar UserPermissions
        var userIds = userPermissions.Select(up => Guid.Parse(up.UserId)).Distinct().ToList();
        var permissionIds = userPermissions.Select(up => Guid.Parse(up.PermissionId)).Distinct().ToList();
        
        // Verificar que todos los UserId existen en el contexto local
        var existingUsers = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync();
        
        var missingUsers = userIds.Except(existingUsers).ToList();
        if (missingUsers.Any())
        {
            _logger.LogError("Error de integridad referencial: Los siguientes UserId no existen para UserPermissions: {MissingIds}", 
                string.Join(", ", missingUsers));
            throw new InvalidOperationException(
                $"No se pueden insertar UserPermissions: Los siguientes UserId no existen en la base de datos: {string.Join(", ", missingUsers)}");
        }
        
        // Verificar que todos los PermissionId existen en el contexto local
        var existingPermissions = await _context.Permissions
            .IgnoreQueryFilters()
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();
        
        var missingPermissions = permissionIds.Except(existingPermissions).ToList();
        if (missingPermissions.Any())
        {
            _logger.LogError("Error de integridad referencial: Los siguientes PermissionId no existen para UserPermissions: {MissingIds}", 
                string.Join(", ", missingPermissions));
            throw new InvalidOperationException(
                $"No se pueden insertar UserPermissions: Los siguientes PermissionId no existen en la base de datos: {string.Join(", ", missingPermissions)}");
        }
        
        _logger.LogInformation("Validación exitosa: {UserCount} usuarios y {PermissionCount} permisos encontrados para UserPermissions", 
            existingUsers.Count, existingPermissions.Count);
        
        // Ahora insertar UserPermissions con la garantía de que las FK existen
        foreach (var upData in userPermissions)
        {
            var userId = Guid.Parse(upData.UserId);
            var permissionId = Guid.Parse(upData.PermissionId);
            
            // Verificación adicional por si acaso
            var userExists = existingUsers.Contains(userId);
            var permissionExists = existingPermissions.Contains(permissionId);
            
            if (!userExists || !permissionExists)
            {
                _logger.LogError("Error crítico: UserId={UserId} existe={UserExists}, PermissionId={PermissionId} existe={PermissionExists}", 
                    userId, userExists, permissionId, permissionExists);
                continue; // Saltar este registro
            }
            
            var existing = await _context.UserPermissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(up => 
                    up.UserId == userId && 
                    up.PermissionId == permissionId);

            if (existing == null)
            {
                var up = new UserPermission
                {
                    Id = Guid.Parse(upData.Id),
                    UserId = userId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.UserPermissions.Add(up);
                _logger.LogDebug("UserPermission añadido: UserId={UserId}, PermissionId={PermissionId}", userId, permissionId);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                _logger.LogDebug("UserPermission reactivado: UserId={UserId}, PermissionId={PermissionId}", userId, permissionId);
            }
        }
        // NOTA: SaveChangesAsync se llama explícitamente en SeedTestDataAsync después de SeedUserPermissionsAsync
        // para garantizar persistencia inmediata y evitar problemas de concurrencia
        _logger.LogInformation("UserPermissions preparados para guardar: {Count}", userPermissions.Count);
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
                _logger.LogInformation("[SEED] Cargado registro específico para test: Supplier '{Name}' (Id: {Id})", 
                    supplierData.Name, supplierData.Id);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                _logger.LogInformation("[SEED] Reactivado registro existente: Supplier '{Name}' (Id: {Id})", 
                    supplierData.Name, supplierData.Id);
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
                _logger.LogInformation("[SEED] Cargado registro específico para test: Customer '{Name}' (Id: {Id})", 
                    customerData.Name, customerData.Id);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                _logger.LogInformation("[SEED] Reactivado registro existente: Customer '{Name}' (Id: {Id})", 
                    customerData.Name, customerData.Id);
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedCountriesAsync(List<CountrySeed> countries)
    {
        foreach (var countryData in countries)
        {
            var existing = await _context.Countries
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Code == countryData.Code);

            if (existing == null)
            {
                var country = new Country
                {
                    Id = Guid.Parse(countryData.Id),
                    Name = countryData.Name,
                    Code = countryData.Code,
                    LanguageId = Guid.Parse(countryData.LanguageId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Countries.Add(country);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedCitiesAsync(List<CitySeed> cities)
    {
        foreach (var cityData in cities)
        {
            var existing = await _context.Cities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == Guid.Parse(cityData.Id));

            if (existing == null)
            {
                var city = new City
                {
                    Id = Guid.Parse(cityData.Id),
                    StateId = Guid.Parse(cityData.StateId),
                    Name = cityData.Name,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Cities.Add(city);
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
        public List<LanguageSeed>? Languages { get; set; }
        public List<CountrySeed>? Countries { get; set; }
        public List<CitySeed>? Cities { get; set; }
        public List<CompanySeed>? Companies { get; set; }
        public List<UserSeed>? Users { get; set; }
        public List<GroupSeed>? Groups { get; set; }
        public List<PermissionSeed>? Permissions { get; set; }
        public List<UserGroupSeed>? UserGroups { get; set; }
        public List<GroupPermissionSeed>? GroupPermissions { get; set; }
        public List<UserPermissionSeed>? UserPermissions { get; set; }
        public List<AdminUserSeed>? AdminUsers { get; set; }
        public List<SupplierSeed>? Suppliers { get; set; }
        public List<CustomerSeed>? Customers { get; set; }
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

    private class CountrySeed
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string LanguageId { get; set; } = string.Empty;
    }

    private class CitySeed
    {
        public string Id { get; set; } = string.Empty;
        public string StateId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
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
