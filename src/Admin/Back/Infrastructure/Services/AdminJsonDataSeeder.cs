using GesFer.Admin.Back.Domain.Entities;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly string _seedsPath;

    public AdminJsonDataSeeder(
        AdminDbContext context,
        ILogger<AdminJsonDataSeeder> logger)
    {
        _context = context;
        _logger = logger;

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
        return File.Exists(Path.Combine(directoryPath, "admin-users.json"));
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

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userData.Password);

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
                if (!BCrypt.Net.BCrypt.Verify(userData.Password, existing.PasswordHash))
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
