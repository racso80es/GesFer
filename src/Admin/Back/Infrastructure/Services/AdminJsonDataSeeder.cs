using GesFer.Admin.Back.Domain.Entities;
using GesFer.Admin.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Shared.Back.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using BCrypt.Net;

namespace GesFer.Admin.Infrastructure.Services;

/// <summary>
/// Resultado de la carga de datos de seed para Admin
/// </summary>
public class AdminSeedResult
{
    public bool Loaded { get; set; }
    public List<string> Entities { get; set; } = new();
}

/// <summary>
/// Servicio para cargar datos de seed de Admin desde archivos JSON
/// </summary>
public class AdminJsonDataSeeder
{
    private readonly AdminDbContext _context;
    private readonly ILogger<AdminJsonDataSeeder> _logger;
    private readonly ISensitiveDataSanitizer _sanitizer;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly string _seedsPath;

    public AdminJsonDataSeeder(
        AdminDbContext context,
        ILogger<AdminJsonDataSeeder> logger,
        ISensitiveDataSanitizer sanitizer,
        IHostEnvironment hostEnvironment)
    {
        _context = context;
        _logger = logger;
        _sanitizer = sanitizer;
        _hostEnvironment = hostEnvironment;

        // Obtener la ruta de los archivos de seed
        // Ubicación canónica: src/Admin/Back/Infrastructure/Data/Seeds/
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
                var canonicalPath = Path.Combine(solutionDir.FullName, "src", "Admin", "Back", "Infrastructure", "Data", "Seeds");
                if (Directory.Exists(canonicalPath))
                {
                    foundPath = canonicalPath;
                }
            }
        }

        _seedsPath = foundPath ?? Path.Combine(basePath, "Data", "Seeds");

        if (!Directory.Exists(_seedsPath))
        {
            _logger.LogWarning("No se encontró la carpeta de seeds de Admin. Se esperaba en: {Path}", _seedsPath);
        }
        else
        {
            _logger.LogInformation("Carpeta de seeds de Admin encontrada: {Path}", _seedsPath);
        }
    }

    private static bool HasAnySeedJson(string directoryPath)
    {
        return File.Exists(Path.Combine(directoryPath, "admin-users.json"))
            || File.Exists(Path.Combine(directoryPath, "companies.json"));
    }

    /// <summary>
    /// Carga todos los seeds de Admin en orden: companies.json y luego admin-users.json.
    /// Responsabilidad única: carga conjunta de datos Admin para BD compartida.
    /// </summary>
    public async Task<AdminSeedResult> SeedAllAsync()
    {
        var result = new AdminSeedResult();
        var companiesResult = await SeedCompaniesAsync();
        if (companiesResult.Loaded)
        {
            result.Loaded = true;
            result.Entities.AddRange(companiesResult.Entities);
        }
        var usersResult = await SeedAdminUsersAsync();
        if (usersResult.Loaded)
        {
            result.Loaded = true;
            result.Entities.AddRange(usersResult.Entities);
        }
        return result;
    }

    /// <summary>
    /// Carga usuarios administrativos desde admin-users.json
    /// </summary>
    public async Task<AdminSeedResult> SeedAdminUsersAsync()
    {
        var result = new AdminSeedResult();
        var filePath = Path.Combine(_seedsPath, "admin-users.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo admin-users.json no encontrado en {Path}", filePath);
            return result;
        }

        _logger.LogInformation("Cargando usuarios admin desde {Path}", filePath);
        var json = await File.ReadAllTextAsync(filePath);
        var users = JsonSerializer.Deserialize<List<AdminUserSeed>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (users == null || !users.Any())
        {
             _logger.LogWarning("No se encontraron usuarios en admin-users.json");
            return result;
        }

        int count = 0;
        foreach (var userData in users)
        {
            var existing = await _context.AdminUsers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == userData.Username);

            string passwordHash;
            if (string.IsNullOrEmpty(userData.Password))
            {
                string rawPassword;
                if (_hostEnvironment.IsDevelopment() || _hostEnvironment.IsEnvironment("Testing"))
                {
                    rawPassword = "admin123";
                    _logger.LogWarning("[SEED ADMIN] ⚠️ DEV/TEST MODE: Setting fixed password '{Password}' for Admin '{Username}'", rawPassword, userData.Username);
                }
                else
                {
                    rawPassword = _sanitizer.GenerateRandomPassword();
                    _logger.LogWarning("[SEED ADMIN] 🔐 Generated RANDOM password for Admin '{Username}': {Password}", userData.Username, rawPassword);
                }
                passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
            }
            else
            {
                passwordHash = BCrypt.Net.BCrypt.HashPassword(userData.Password);
            }

            if (existing == null)
            {
                Guid id;
                if (!Guid.TryParse(userData.Id, out id))
                {
                    id = Guid.NewGuid();
                }

                var user = new AdminUser
                {
                    Id = id,
                    Username = userData.Username,
                    PasswordHash = passwordHash,
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    Email = userData.Email,
                    Role = userData.Role ?? "Admin",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.AdminUsers.Add(user);
                count++;
                _logger.LogInformation("[SEED ADMIN] Creado usuario admin: {Username}", userData.Username);
            }
            else
            {
                bool modified = false;
                if (existing.DeletedAt != null)
                {
                    existing.DeletedAt = null;
                    existing.IsActive = true;
                    modified = true;
                    _logger.LogInformation("[SEED ADMIN] Reactivado usuario admin: {Username}", userData.Username);
                }

                // Actualizar contraseña si es un seed (para asegurar que coincida)
                // Esto podría ser debatible en prod, pero para seed/reset es útil.
                // Verificamos si la contraseña ha cambiado
                // Si la password en JSON es vacía (random), NO la actualizamos si ya existe, para no sobrescribir la del usuario.
                if (!string.IsNullOrEmpty(userData.Password) && !BCrypt.Net.BCrypt.Verify(userData.Password, existing.PasswordHash))
                {
                    existing.PasswordHash = passwordHash;
                    modified = true;
                    _logger.LogInformation("[SEED ADMIN] Actualizada contraseña usuario admin: {Username}", userData.Username);
                }

                if (modified) count++;
            }
        }

        if (count > 0)
        {
            await _context.SaveChangesAsync();
            result.Loaded = true;
            result.Entities.Add($"{count} Admin User(s) created/updated");
        }
        else
        {
             result.Loaded = true; // Loaded checked, nothing new
             result.Entities.Add("No new Admin Users");
        }

        return result;
    }

    /// <summary>
    /// Carga empresas (Companies) desde companies.json usando AdminDbContext.
    /// Admin es SSOT para Company; en entornos con BD compartida, ejecutar este seed antes que el de Product.
    /// </summary>
    public async Task<AdminSeedResult> SeedCompaniesAsync()
    {
        var result = new AdminSeedResult();
        var filePath = Path.Combine(_seedsPath, "companies.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Archivo companies.json no encontrado en {Path}", filePath);
            return result;
        }

        _logger.LogInformation("Cargando companies desde {Path}", filePath);
        var json = await File.ReadAllTextAsync(filePath);
        var companies = JsonSerializer.Deserialize<List<CompanySeed>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (companies == null || !companies.Any())
        {
            _logger.LogWarning("No se encontraron companies en companies.json");
            return result;
        }

        int processedCount = 0;
        int skippedCount = 0;

        foreach (var companyData in companies)
        {
            try
            {
                var existing = await _context.Companies
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == Guid.Parse(companyData.Id));

                if (existing == null)
                {
                    TaxId? taxId = null;
                    if (!string.IsNullOrWhiteSpace(companyData.TaxId))
                    {
                        if (!TaxId.TryCreate(companyData.TaxId, out var parsedTaxId))
                        {
                            _logger.LogWarning("[SEED ADMIN] TaxId inválido en Company '{Name}' (Id: {Id}). Registro ignorado.",
                                companyData.Name, companyData.Id);
                            skippedCount++;
                            continue;
                        }
                        taxId = parsedTaxId;
                    }

                    Email? email = null;
                    if (!string.IsNullOrWhiteSpace(companyData.Email))
                    {
                        if (!Email.TryCreate(companyData.Email, out var parsedEmail))
                        {
                            _logger.LogWarning("[SEED ADMIN] Email inválido en Company '{Name}' (Id: {Id}). Registro ignorado.",
                                companyData.Name, companyData.Id);
                            skippedCount++;
                            continue;
                        }
                        email = parsedEmail;
                    }

                    var company = new Company
                    {
                        Id = Guid.Parse(companyData.Id),
                        Name = companyData.Name,
                        TaxId = taxId,
                        Address = companyData.Address,
                        Phone = string.IsNullOrWhiteSpace(companyData.Phone) ? null : companyData.Phone,
                        Email = email,
                        LanguageId = string.IsNullOrWhiteSpace(companyData.LanguageId) ? null : Guid.Parse(companyData.LanguageId),
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Companies.Add(company);
                    processedCount++;
                    _logger.LogInformation("[SEED ADMIN] Creada company: '{Name}' (Id: {Id})", companyData.Name, companyData.Id);
                }
                else if (existing.DeletedAt != null)
                {
                    existing.DeletedAt = null;
                    existing.IsActive = true;
                    processedCount++;
                    _logger.LogInformation("[SEED ADMIN] Reactivada company: '{Name}' (Id: {Id})", companyData.Name, companyData.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SEED ADMIN] Error al procesar Company '{Name}' (Id: {Id})", companyData.Name, companyData.Id);
                skippedCount++;
            }
        }

        if (processedCount > 0)
        {
            await _context.SaveChangesAsync();
            result.Loaded = true;
            result.Entities.Add($"{processedCount} Company(ies)");
        }
        if (skippedCount > 0)
            _logger.LogWarning("[SEED ADMIN] Companies: {Skipped} ignorado(s)", skippedCount);

        return result;
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

    private class AdminUserSeed
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
