using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using GesFer.Infrastructure.Configuration;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    private readonly ISensitiveDataSanitizer _sanitizer;
    private readonly IConfiguration _configuration;
    private readonly string _seedsPath;

    public JsonDataSeeder(
        ApplicationDbContext context,
        ILogger<JsonDataSeeder> logger,
        ISensitiveDataSanitizer sanitizer,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _sanitizer = sanitizer;
        _configuration = configuration;

        // Obtener la ruta de los archivos de seed
        // Ubicación canónica: src/Product/Back/Infrastructure/Data/Seeds/
        var basePath = AppContext.BaseDirectory;
        string? foundPath = null;

        // 1. Buscar en Output Directory (Production/Docker)
        var dataSeedsInOutput = Path.Combine(basePath, "Data", "Seeds");
        if (Directory.Exists(dataSeedsInOutput) && HasAnySeedJson(dataSeedsInOutput))
        {
            foundPath = dataSeedsInOutput;
        }
        else
        {
            // 2. Buscar en Source (Development)
            // Buscar GesFer.sln para encontrar la raíz del repo
            var currentDir = new DirectoryInfo(basePath);
            DirectoryInfo? solutionDir = null;
            var maxDepth = 10;
            var depth = 0;

            while (currentDir != null && solutionDir == null && depth < maxDepth)
            {
                if (File.Exists(Path.Combine(currentDir.FullName, "GesFer.sln")))
                {
                    solutionDir = currentDir;
                }
                else
                {
                    currentDir = currentDir.Parent;
                    depth++;
                }
            }

            if (solutionDir != null)
            {
                // Ruta canónica desde la raíz de la solución
                var canonicalPath = Path.Combine(solutionDir.FullName, "src", "Product", "Back", "Infrastructure", "Data", "Seeds");
                if (Directory.Exists(canonicalPath))
                {
                    foundPath = canonicalPath;
                }
            }
        }

        _seedsPath = foundPath ?? Path.Combine(basePath, "Data", "Seeds");

        if (!Directory.Exists(_seedsPath))
        {
            _logger.LogWarning("No se encontró la carpeta de seeds. Se esperaba en: {Path}", _seedsPath);
        }
        else
        {
            _logger.LogInformation("Carpeta de seeds encontrada: {Path}", _seedsPath);
        }
    }

    private static bool HasAnySeedJson(string directoryPath)
    {
        return File.Exists(Path.Combine(directoryPath, "master-data.json")) ||
               File.Exists(Path.Combine(directoryPath, "demo-data.json")) ||
               File.Exists(Path.Combine(directoryPath, "test-data.json"));
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

        // NOTA: AdminUsers se gestionan en el dominio Admin, no en Product

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

        // Companies: SSOT en Admin. Product usa CompanyId desde config (Seed:CompanyId).
        var validCompanyIds = SeedConfig.GetValidCompanyIds(_configuration);
        if (validCompanyIds.Count == 0)
            _logger.LogWarning("[SEED] Seed:CompanyId no configurado. Configurar en appsettings.Seed.json o Seed:CompanyId.");

        var validUserIds = new HashSet<Guid>();

        // Seed Users
        if (data.Users != null && data.Users.Any())
        {
            await SeedUsersAsync(data.Users, validCompanyIds, validUserIds);
            await _context.SaveChangesAsync();
            result.Entities.Add($"{data.Users.Count} User(s)");
        }

        // Seed UserGroups
        if (data.UserGroups != null && data.UserGroups.Any())
        {
            await SeedUserGroupsAsync(data.UserGroups, validUserIds);
            await _context.SaveChangesAsync();
            result.Entities.Add($"{data.UserGroups.Count} UserGroup(s)");
        }

        // Seed UserPermissions
        if (data.UserPermissions != null && data.UserPermissions.Any())
        {
            await SeedUserPermissionsAsync(data.UserPermissions, validUserIds);
            await _context.SaveChangesAsync();
            result.Entities.Add($"{data.UserPermissions.Count} UserPermission(s)");
        }

        // Seed TaxTypes
        if (data.TaxTypes != null && data.TaxTypes.Any())
        {
            await SeedTaxTypesAsync(data.TaxTypes);
            result.Entities.Add($"{data.TaxTypes.Count} TaxType(s)");
        }

        // Seed ArticleFamilies (tras TaxTypes)
        if (data.ArticleFamilies != null && data.ArticleFamilies.Any())
        {
            await SeedArticleFamiliesAsync(data.ArticleFamilies);
            await _context.SaveChangesAsync();
            result.Entities.Add($"{data.ArticleFamilies.Count} ArticleFamily(ies)");
        }

        // Seed Articles (requieren articleFamilyId)
        if (data.Articles != null && data.Articles.Any())
        {
            await SeedArticlesAsync(data.Articles);
            result.Entities.Add($"{data.Articles.Count} Article(s)");
        }

        // Seed Suppliers
        if (data.Suppliers != null && data.Suppliers.Any())
        {
            await SeedSuppliersAsync(data.Suppliers, validCompanyIds);
            await _context.SaveChangesAsync();
            result.Entities.Add($"{data.Suppliers.Count} Supplier(s)");
        }

        // Seed Customers
        if (data.Customers != null && data.Customers.Any())
        {
            await SeedCustomersAsync(data.Customers, validCompanyIds);
            await _context.SaveChangesAsync();
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

        // 4. Companies: SSOT en Admin. Product usa CompanyId desde config (Seed:CompanyId).
        var validCompanyIds = SeedConfig.GetValidCompanyIds(_configuration);
        if (validCompanyIds.Count == 0)
            _logger.LogWarning("[SEED] Seed:CompanyId no configurado. Configurar en appsettings.Seed.json para test-data.");

        // CASCADA RESILIENTE: Crear HashSet de IDs válidos de usuarios para evitar referencias huérfanas
        var validUserIds = new HashSet<Guid>();

        // 5. Users (depende de Companies y Languages) - DEBE ejecutarse después de Companies
        if (data.Users != null && data.Users.Any())
        {
            // CASCADA RESILIENTE: Validar CompanyId contra lista blanca antes de procesar
            await SeedUsersAsync(data.Users, validCompanyIds, validUserIds);
            await _context.SaveChangesAsync();
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
            await SeedUserGroupsAsync(data.UserGroups, validUserIds);
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
            await SeedUserPermissionsAsync(data.UserPermissions, validUserIds);
            await _context.SaveChangesAsync();
            _logger.LogInformation("UserPermissions sembrados: {Count}", data.UserPermissions.Count);
        }

        // NOTA: AdminUsers se gestionan en el dominio Admin, no en Product

        // 12. Suppliers (depende de Companies) - DEBE ejecutarse después de Companies
        if (data.Suppliers != null && data.Suppliers.Any())
        {
            await SeedSuppliersAsync(data.Suppliers, validCompanyIds);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Suppliers sembrados: {Count}", data.Suppliers.Count);
        }

        // 13. Customers (depende de Companies) - DEBE ejecutarse después de Companies
        if (data.Customers != null && data.Customers.Any())
        {
            await SeedCustomersAsync(data.Customers, validCompanyIds);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Customers sembrados: {Count}", data.Customers.Count);
        }

        _logger.LogInformation("Datos de prueba cargados correctamente");

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

    private async Task SeedUsersAsync(List<UserSeed> users, HashSet<Guid> validCompanyIds, HashSet<Guid> validUserIds)
    {
        // Hash BCrypt fijo conocido para "admin123" (usado en tests y setup)
        // Este hash debe coincidir con el usado en SetupService y TestDataSeeder
        const string fixedAdminHash = "$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW";

        int skippedCount = 0;
        int processedCount = 0;

        foreach (var userData in users)
        {
            try
            {
                // CASCADA RESILIENTE: Validar CompanyId contra lista blanca ANTES de cualquier otra cosa
                var companyId = Guid.Parse(userData.CompanyId);
                if (!validCompanyIds.Contains(companyId))
                {
                    _logger.LogWarning("Usuario {Name} descartado por empresa padre inexistente (CompanyId: {CompanyId})",
                        userData.Username, companyId);
                    skippedCount++;
                    continue;
                }

                var existing = await _context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userData.Id));

                // Usar hash fijo para "admin123" para mantener consistencia con tests
                string passwordHash;
                if (userData.Password == "admin123")
                {
                    passwordHash = fixedAdminHash;
                }
                else if (string.IsNullOrEmpty(userData.Password))
                {
                    var randomPwd = _sanitizer.GenerateRandomPassword();
                    passwordHash = BCrypt.Net.BCrypt.HashPassword(randomPwd);
                    // IMPORTANTE: En entorno real, esto debería comunicarse de forma segura.
                    // Aquí lo logueamos como Warning para que el desarrollador lo vea en la consola al iniciar.
                    _logger.LogWarning("[SEED SECURE] 🔐 Generated RANDOM password for user '{Username}': {Password}", userData.Username, randomPwd);
                }
                else
                {
                    passwordHash = BCrypt.Net.BCrypt.HashPassword(userData.Password);
                }

                if (existing == null)
                {
                    // Validar y convertir Email si se proporciona
                    Email? email = null;
                    if (!string.IsNullOrWhiteSpace(userData.Email))
                    {
                        try
                        {
                            email = Email.Create(userData.Email);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogWarning("[SEED] Violación de Dominio - Email inválido en User '{Username}' (Id: {Id}): {Error}. Registro ignorado.",
                                userData.Username, userData.Id, ex.Message);
                            skippedCount++;
                            continue;
                        }
                    }

                    var user = new User
                    {
                        Id = Guid.Parse(userData.Id),
                        CompanyId = companyId,
                        Username = userData.Username,
                        PasswordHash = passwordHash,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        Email = email,
                        Phone = userData.Phone,
                        LanguageId = Guid.Parse(userData.LanguageId),
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Users.Add(user);
                    // CASCADA RESILIENTE: Agregar ID a lista blanca solo si el usuario pasa validación
                    validUserIds.Add(user.Id);
                    processedCount++;
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
                    // CASCADA RESILIENTE: Agregar ID a lista blanca si se reactiva
                    validUserIds.Add(existing.Id);
                    processedCount++;
                    _logger.LogInformation("[SEED] Reactivado registro existente: User '{Username}' (Id: {Id})",
                        userData.Username, userData.Id);
                }
                else
                {
                    // CASCADA RESILIENTE: Agregar ID a lista blanca si ya existe y está activa
                    validUserIds.Add(existing.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SEED] Error inesperado al procesar User '{Username}' (Id: {Id}): {Error}. Registro ignorado.",
                    userData.Username, userData.Id, ex.Message);
                skippedCount++;
                continue;
            }
        }

        if (skippedCount > 0)
        {
            _logger.LogWarning("[SEED] Users: {SkippedCount} registro(s) ignorado(s) por Violación de Dominio (Email inválido) o empresa padre inexistente de {TotalCount} totales",
                skippedCount, users.Count);
        }

        _logger.LogInformation("[SEED] Users procesados: {ProcessedCount} exitoso(s), {SkippedCount} ignorado(s) de {TotalCount} totales",
            processedCount, skippedCount, users.Count);
        // NOTA: SaveChangesAsync se llama explícitamente en SeedTestDataAsync después de SeedUsersAsync
        // para garantizar persistencia inmediata y evitar problemas de concurrencia
    }

    private async Task SeedUserGroupsAsync(List<UserGroupSeed> userGroups, HashSet<Guid> validUserIds)
    {
        int skippedCount = 0;
        int processedCount = 0;

        foreach (var ugData in userGroups)
        {
            // CASCADA RESILIENTE: Validar UserId contra lista blanca ANTES de cualquier otra cosa
            var userId = Guid.Parse(ugData.UserId);
            if (!validUserIds.Contains(userId))
            {
                _logger.LogWarning("UserGroup descartado por usuario padre inexistente (UserId: {UserId})", userId);
                skippedCount++;
                continue;
            }

            var existing = await _context.UserGroups
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(ug =>
                    ug.UserId == userId &&
                    ug.GroupId == Guid.Parse(ugData.GroupId));

            if (existing == null)
            {
                var ug = new UserGroup
                {
                    Id = Guid.Parse(ugData.Id),
                    UserId = userId,
                    GroupId = Guid.Parse(ugData.GroupId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.UserGroups.Add(ug);
                processedCount++;
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                processedCount++;
            }
        }

        if (skippedCount > 0)
        {
            _logger.LogWarning("[SEED] UserGroups: {SkippedCount} registro(s) ignorado(s) por usuario padre inexistente de {TotalCount} totales",
                skippedCount, userGroups.Count);
        }

        _logger.LogInformation("[SEED] UserGroups procesados: {ProcessedCount} exitoso(s), {SkippedCount} ignorado(s) de {TotalCount} totales",
            processedCount, skippedCount, userGroups.Count);
        await _context.SaveChangesAsync();
    }

    private async Task SeedUserPermissionsAsync(List<UserPermissionSeed> userPermissions, HashSet<Guid> validUserIds)
    {
        int skippedCount = 0;
        int processedCount = 0;

        // CRÍTICO: Validar explícitamente que Users y Permissions existen antes de insertar UserPermissions
        var userIds = userPermissions.Select(up => Guid.Parse(up.UserId)).Distinct().ToList();
        var permissionIds = userPermissions.Select(up => Guid.Parse(up.PermissionId)).Distinct().ToList();

        // CASCADA RESILIENTE: Filtrar UserPermissions que referencian usuarios inexistentes
        var validUserPermissions = userPermissions.Where(up =>
        {
            var userId = Guid.Parse(up.UserId);
            return validUserIds.Contains(userId);
        }).ToList();

        skippedCount = userPermissions.Count - validUserPermissions.Count;

        if (skippedCount > 0)
        {
            _logger.LogWarning("[SEED] UserPermissions: {SkippedCount} registro(s) ignorado(s) por usuario padre inexistente de {TotalCount} totales",
                skippedCount, userPermissions.Count);
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
            _logger.LogWarning("[SEED] UserPermissions: Algunos PermissionId no existen: {MissingIds}. Los registros afectados serán ignorados.",
                string.Join(", ", missingPermissions));
        }

        _logger.LogInformation("Validación exitosa: {UserCount} usuarios válidos y {PermissionCount} permisos encontrados para UserPermissions",
            validUserIds.Count, existingPermissions.Count);

        // Ahora insertar UserPermissions con la garantía de que las FK existen
        foreach (var upData in validUserPermissions)
        {
            var userId = Guid.Parse(upData.UserId);
            var permissionId = Guid.Parse(upData.PermissionId);

            // Verificación adicional por si acaso
            var userExists = validUserIds.Contains(userId);
            var permissionExists = existingPermissions.Contains(permissionId);

            if (!userExists || !permissionExists)
            {
                _logger.LogWarning("UserPermission descartado: UserId={UserId} existe={UserExists}, PermissionId={PermissionId} existe={PermissionExists}",
                    userId, userExists, permissionId, permissionExists);
                skippedCount++;
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
                processedCount++;
                _logger.LogDebug("UserPermission añadido: UserId={UserId}, PermissionId={PermissionId}", userId, permissionId);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
                processedCount++;
                _logger.LogDebug("UserPermission reactivado: UserId={UserId}, PermissionId={PermissionId}", userId, permissionId);
            }
        }

        _logger.LogInformation("[SEED] UserPermissions procesados: {ProcessedCount} exitoso(s), {SkippedCount} ignorado(s) de {TotalCount} totales",
            processedCount, skippedCount, userPermissions.Count);
        // NOTA: SaveChangesAsync se llama explícitamente en SeedTestDataAsync después de SeedUserPermissionsAsync
        // para garantizar persistencia inmediata y evitar problemas de concurrencia
    }

    private async Task SeedTaxTypesAsync(List<TaxTypeSeed> taxTypes)
    {
        foreach (var taxTypeData in taxTypes)
        {
            var existing = await _context.TaxTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == Guid.Parse(taxTypeData.Id));

            if (existing == null)
            {
                var taxType = new TaxType
                {
                    Id = Guid.Parse(taxTypeData.Id),
                    CompanyId = Guid.Parse(taxTypeData.CompanyId),
                    Code = taxTypeData.Code,
                    Name = taxTypeData.Name,
                    Description = taxTypeData.Description,
                    Value = taxTypeData.Value,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.TaxTypes.Add(taxType);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedArticleFamiliesAsync(List<ArticleFamilySeed> articleFamilies)
    {
        foreach (var afData in articleFamilies)
        {
            var existing = await _context.ArticleFamilies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(af => af.Id == Guid.Parse(afData.Id));

            if (existing == null)
            {
                var af = new ArticleFamily
                {
                    Id = Guid.Parse(afData.Id),
                    CompanyId = Guid.Parse(afData.CompanyId),
                    Code = afData.Code,
                    Name = afData.Name,
                    Description = afData.Description,
                    TaxTypeId = Guid.Parse(afData.TaxTypeId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.ArticleFamilies.Add(af);
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
                if (string.IsNullOrEmpty(articleData.ArticleFamilyId))
                    continue;
                var article = new Article
                {
                    Id = Guid.Parse(articleData.Id),
                    CompanyId = Guid.Parse(articleData.CompanyId),
                    ArticleFamilyId = Guid.Parse(articleData.ArticleFamilyId),
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

    private async Task SeedSuppliersAsync(List<SupplierSeed> suppliers, HashSet<Guid> validCompanyIds)
    {
        int skippedCount = 0;
        int processedCount = 0;

        foreach (var supplierData in suppliers)
        {
            try
            {
                // CASCADA RESILIENTE: Validar CompanyId contra lista blanca ANTES de cualquier otra cosa
                var companyId = Guid.Parse(supplierData.CompanyId);
                if (!validCompanyIds.Contains(companyId))
                {
                    _logger.LogWarning("Supplier {Name} descartado por empresa padre inexistente (CompanyId: {CompanyId})",
                        supplierData.Name, companyId);
                    skippedCount++;
                    continue;
                }

                var existing = await _context.Suppliers
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Id == Guid.Parse(supplierData.Id));

                if (existing == null)
                {
                    var supplier = new Supplier
                    {
                        Id = Guid.Parse(supplierData.Id),
                        CompanyId = companyId,
                        Name = supplierData.Name,
                        TaxId = supplierData.TaxId,
                        Address = supplierData.Address,
                        Phone = supplierData.Phone,
                        Email = supplierData.Email,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Suppliers.Add(supplier);
                    processedCount++;
                    _logger.LogInformation("[SEED] Cargado registro específico para test: Supplier '{Name}' (Id: {Id})",
                        supplierData.Name, supplierData.Id);
                }
                else if (existing.DeletedAt != null)
                {
                    existing.DeletedAt = null;
                    existing.IsActive = true;
                    processedCount++;
                    _logger.LogInformation("[SEED] Reactivado registro existente: Supplier '{Name}' (Id: {Id})",
                        supplierData.Name, supplierData.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SEED] Error inesperado al procesar Supplier '{Name}' (Id: {Id}): {Error}. Registro ignorado.",
                    supplierData.Name, supplierData.Id, ex.Message);
                skippedCount++;
                continue;
            }
        }

        if (skippedCount > 0)
        {
            _logger.LogWarning("[SEED] Suppliers: {SkippedCount} registro(s) ignorado(s) por empresa padre inexistente de {TotalCount} totales",
                skippedCount, suppliers.Count);
        }

        _logger.LogInformation("[SEED] Suppliers procesados: {ProcessedCount} exitoso(s), {SkippedCount} ignorado(s) de {TotalCount} totales",
            processedCount, skippedCount, suppliers.Count);
        await _context.SaveChangesAsync();
    }

    private async Task SeedCustomersAsync(List<CustomerSeed> customers, HashSet<Guid> validCompanyIds)
    {
        int skippedCount = 0;
        int processedCount = 0;

        foreach (var customerData in customers)
        {
            try
            {
                // CASCADA RESILIENTE: Validar CompanyId contra lista blanca ANTES de cualquier otra cosa
                var companyId = Guid.Parse(customerData.CompanyId);
                if (!validCompanyIds.Contains(companyId))
                {
                    _logger.LogWarning("Customer {Name} descartado por empresa padre inexistente (CompanyId: {CompanyId})",
                        customerData.Name, companyId);
                    skippedCount++;
                    continue;
                }

                var existing = await _context.Customers
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == Guid.Parse(customerData.Id));

                if (existing == null)
                {
                    // Validar y convertir TaxId si se proporciona
                    TaxId? taxId = null;
                    if (!string.IsNullOrWhiteSpace(customerData.TaxId))
                    {
                        try
                        {
                            taxId = TaxId.Create(customerData.TaxId);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogWarning("[SEED] Violación de Dominio - TaxId inválido en Customer '{Name}' (Id: {Id}): {Error}. Registro ignorado.",
                                customerData.Name, customerData.Id, ex.Message);
                            skippedCount++;
                            continue;
                        }
                    }

                    // Validar y convertir Email si se proporciona
                    Email? email = null;
                    if (!string.IsNullOrWhiteSpace(customerData.Email))
                    {
                        try
                        {
                            email = Email.Create(customerData.Email);
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogWarning("[SEED] Violación de Dominio - Email inválido en Customer '{Name}' (Id: {Id}): {Error}. Registro ignorado.",
                                customerData.Name, customerData.Id, ex.Message);
                            skippedCount++;
                            continue;
                        }
                    }

                    var customer = new Customer
                    {
                        Id = Guid.Parse(customerData.Id),
                        CompanyId = companyId,
                        Name = customerData.Name,
                        TaxId = taxId,
                        Address = customerData.Address,
                        Phone = customerData.Phone,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Customers.Add(customer);
                    processedCount++;
                    _logger.LogInformation("[SEED] Cargado registro específico para test: Customer '{Name}' (Id: {Id})",
                        customerData.Name, customerData.Id);
                }
                else if (existing.DeletedAt != null)
                {
                    existing.DeletedAt = null;
                    existing.IsActive = true;
                    processedCount++;
                    _logger.LogInformation("[SEED] Reactivado registro existente: Customer '{Name}' (Id: {Id})",
                        customerData.Name, customerData.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SEED] Error inesperado al procesar Customer '{Name}' (Id: {Id}): {Error}. Registro ignorado.",
                    customerData.Name, customerData.Id, ex.Message);
                skippedCount++;
                continue;
            }
        }

        if (skippedCount > 0)
        {
            _logger.LogWarning("[SEED] Customers: {SkippedCount} registro(s) ignorado(s) por Violación de Dominio (Email/TaxId inválidos) o empresa padre inexistente de {TotalCount} totales",
                skippedCount, customers.Count);
        }

        _logger.LogInformation("[SEED] Customers procesados: {ProcessedCount} exitoso(s), {SkippedCount} ignorado(s) de {TotalCount} totales",
            processedCount, skippedCount, customers.Count);
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
    }

    private class DemoDataSeed
    {
        public List<CompanySeed>? Companies { get; set; }
        public List<UserSeed>? Users { get; set; }
        public List<UserGroupSeed>? UserGroups { get; set; }
        public List<UserPermissionSeed>? UserPermissions { get; set; }
        public List<TaxTypeSeed>? TaxTypes { get; set; }
        public List<ArticleFamilySeed>? ArticleFamilies { get; set; }
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

    private class TaxTypeSeed
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Value { get; set; }
    }

    private class ArticleFamilySeed
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TaxTypeId { get; set; } = string.Empty;
    }

    private class ArticleSeed
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string? ArticleFamilyId { get; set; }
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

    // NOTA: AdminUser seeding se gestiona en el dominio Admin
}
