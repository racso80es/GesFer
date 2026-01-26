using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para aplicar y verificar el cumplimiento de las Reglas de Oro
/// Permite continuar desde donde se quedó la última ejecución
/// </summary>
public class GoldenRulesComplianceService
{
    private readonly LogService _logService;
    private readonly string _rootPath;
    private readonly string _stateFilePath;
    private readonly string _entitiesPath;
    private readonly string _seedsPath;
    private readonly string _testsPath;

    public GoldenRulesComplianceService(LogService logService)
    {
        _logService = logService;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        _stateFilePath = Path.Combine(_rootPath, ".golden-rules-state.json");
        _entitiesPath = Path.Combine(_rootPath, "Api", "src", "Domain", "Entities");
        _seedsPath = Path.Combine(_rootPath, "Api", "src");
        _testsPath = Path.Combine(_rootPath, "Api", "src", "IntegrationTests");
    }

    /// <summary>
    /// Estado guardado del cumplimiento de reglas de oro
    /// </summary>
    public class ComplianceState
    {
        public DateTime LastRun { get; set; }
        public List<EntitySyncStatus> Entities { get; set; } = new();
        public List<string> CompletedTasks { get; set; } = new();
        public List<string> PendingTasks { get; set; } = new();
        public ValidationStatus LastValidation { get; set; } = new();
    }

    public class EntitySyncStatus
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityFile { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
        public List<string> Properties { get; set; } = new();
        public bool SeedsSynced { get; set; }
        public bool TestsSynced { get; set; }
        public DateTime LastChecked { get; set; }
        public List<string> SyncErrors { get; set; } = new();
    }

    public class ValidationStatus
    {
        public DateTime Timestamp { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, bool> Checks { get; set; } = new();
    }

    /// <summary>
    /// Ejecuta el cumplimiento de reglas de oro, continuando desde el último punto
    /// </summary>
    public async Task<ComplianceResult> EnforceGoldenRulesAsync(bool forceFullCheck = false)
    {
        _logService.WriteLog("========================================");
        _logService.WriteLog("Inicio de cumplimiento de Reglas de Oro");
        _logService.WriteLog("========================================");

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("   Cumplimiento de Reglas de Oro");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Cargar estado previo
        var state = LoadState();
        if (state == null || forceFullCheck)
        {
            state = new ComplianceState { LastRun = DateTime.MinValue };
            Console.WriteLine("   → Ejecución completa (sin estado previo o forzado)");
        }
        else
        {
            Console.WriteLine($"   → Continuando desde: {state.LastRun:yyyy-MM-dd HH:mm:ss}");
        }

        var result = new ComplianceResult
        {
            StartTime = DateTime.Now
        };

        try
        {
            // Paso 1: Detectar entidades y cambios
            Console.WriteLine();
            Console.WriteLine("[1/4] Detectando entidades y cambios...");
            var entities = await DetectEntitiesAndChangesAsync(state);
            result.EntitiesFound = entities.Count;
            result.EntitiesChanged = entities.Count(e => !state.Entities.Any(s => s.EntityName == e.EntityName && s.FileHash == e.FileHash));
            
            Console.WriteLine($"    ✓ {entities.Count} entidades encontradas");
            Console.WriteLine($"    ✓ {result.EntitiesChanged} entidades con cambios detectados");

            // Paso 2: Verificar sincronización de Seeds
            Console.WriteLine();
            Console.WriteLine("[2/4] Verificando sincronización de Seeds...");
            await VerifySeedsSyncAsync(entities, state);
            result.SeedsChecked = entities.Count;

            // Paso 3: Verificar sincronización de Tests
            Console.WriteLine();
            Console.WriteLine("[3/4] Verificando sincronización de Tests...");
            await VerifyTestsSyncAsync(entities, state);
            result.TestsChecked = entities.Count;

            // Paso 4: Guardar estado
            Console.WriteLine();
            Console.WriteLine("[4/4] Guardando estado de progreso...");
            state.LastRun = DateTime.Now;
            state.Entities = entities;
            SaveState(state);
            Console.WriteLine("    ✓ Estado guardado correctamente");

            result.Success = true;
            result.EndTime = DateTime.Now;
            
            // Mostrar resumen
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("   Resumen de Cumplimiento");
            Console.WriteLine("========================================");
            Console.WriteLine($"   Entidades analizadas: {result.EntitiesFound}");
            Console.WriteLine($"   Entidades con cambios: {result.EntitiesChanged}");
            Console.WriteLine($"   Seeds verificados: {result.SeedsChecked}");
            Console.WriteLine($"   Tests verificados: {result.TestsChecked}");
            Console.WriteLine($"   Tiempo transcurrido: {(result.EndTime - result.StartTime).TotalSeconds:F2}s");
            Console.WriteLine();

            // Mostrar entidades que requieren atención
            var needsAttention = entities.Where(e => !e.SeedsSynced || !e.TestsSynced || e.SyncErrors.Any()).ToList();
            if (needsAttention.Any())
            {
                Console.WriteLine("   ⚠ Entidades que requieren atención:");
                foreach (var entity in needsAttention)
                {
                    Console.WriteLine($"      - {entity.EntityName}");
                    if (!entity.SeedsSynced)
                        Console.WriteLine("        → Seeds no sincronizados");
                    if (!entity.TestsSynced)
                        Console.WriteLine("        → Tests no sincronizados");
                    foreach (var error in entity.SyncErrors)
                    {
                        Console.WriteLine($"        → Error: {error}");
                    }
                }
                Console.WriteLine();
                result.HasWarnings = true;
            }
            else
            {
                Console.WriteLine("   ✓ Todas las entidades están sincronizadas correctamente");
                Console.WriteLine();
            }

            _logService.WriteLog($"Cumplimiento de reglas de oro completado: {result.EntitiesFound} entidades, {result.EntitiesChanged} cambios");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
            _logService.WriteError("Error en cumplimiento de reglas de oro", ex);
            Console.WriteLine($"   ✗ Error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Detecta todas las entidades y sus cambios
    /// </summary>
    private async Task<List<EntitySyncStatus>> DetectEntitiesAndChangesAsync(ComplianceState previousState)
    {
        var entities = new List<EntitySyncStatus>();

        if (!Directory.Exists(_entitiesPath))
        {
            _logService.WriteLog($"Directorio de entidades no encontrado: {_entitiesPath}");
            return entities;
        }

        var entityFiles = Directory.GetFiles(_entitiesPath, "*.cs", SearchOption.TopDirectoryOnly)
            .Where(f => !f.Contains(".g.cs") && !Path.GetFileName(f).StartsWith("Base"))
            .ToList();

        foreach (var file in entityFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var content = await File.ReadAllTextAsync(file);
                var hash = ComputeFileHash(content);

                // Detectar propiedades de la entidad
                var properties = ExtractProperties(content);

                var previousStatus = previousState.Entities.FirstOrDefault(e => e.EntityName == fileName);
                var hasChanged = previousStatus == null || previousStatus.FileHash != hash;

                var entityStatus = new EntitySyncStatus
                {
                    EntityName = fileName,
                    EntityFile = file,
                    FileHash = hash,
                    Properties = properties,
                    SeedsSynced = previousStatus?.SeedsSynced ?? false,
                    TestsSynced = previousStatus?.TestsSynced ?? false,
                    LastChecked = DateTime.Now,
                    SyncErrors = previousStatus?.SyncErrors ?? new List<string>()
                };

                // Si cambió, marcar como no sincronizado
                if (hasChanged)
                {
                    entityStatus.SeedsSynced = false;
                    entityStatus.TestsSynced = false;
                    entityStatus.SyncErrors.Clear();
                }

                entities.Add(entityStatus);
            }
            catch (Exception ex)
            {
                _logService.WriteError($"Error al procesar entidad {file}", ex);
            }
        }

        return entities;
    }

    /// <summary>
    /// Extrae las propiedades públicas de una clase
    /// </summary>
    private List<string> ExtractProperties(string content)
    {
        var properties = new List<string>();
        
        // Buscar propiedades con regex
        var propertyPattern = @"public\s+(?:virtual\s+)?(?:readonly\s+)?(\w+(\?|\[\])?)\s+(\w+)\s*\{";
        var matches = Regex.Matches(content, propertyPattern, RegexOptions.Multiline);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 4)
            {
                var propertyName = match.Groups[3].Value;
                // Excluir propiedades de navegación comunes
                if (!propertyName.EndsWith("Id") || propertyName == "Id")
                {
                    properties.Add(propertyName);
                }
            }
        }

        // También buscar propiedades con inicializadores
        var propertyPattern2 = @"public\s+(\w+(\?|\[\])?)\s+(\w+)\s*\{[^}]*get[^}]*set[^}]*\}\s*=\s*";
        matches = Regex.Matches(content, propertyPattern2, RegexOptions.Multiline);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 4)
            {
                var propertyName = match.Groups[3].Value;
                if (!properties.Contains(propertyName))
                {
                    properties.Add(propertyName);
                }
            }
        }

        return properties.Distinct().ToList();
    }

    /// <summary>
    /// Verifica que los Seeds estén sincronizados
    /// </summary>
    private async Task VerifySeedsSyncAsync(List<EntitySyncStatus> entities, ComplianceState state)
    {
        foreach (var entity in entities.Where(e => !e.SeedsSynced))
        {
            try
            {
                var entityName = entity.EntityName;
                
                // Limpiar errores relacionados con seeds si se va a verificar de nuevo
                var seedError = "Seeds no sincronizados - requiere actualización manual";
                entity.SyncErrors.RemoveAll(e => e.Contains("Seeds no sincronizados"));
                
                var seedsSynced = await CheckSeedsSyncAsync(entityName, entity.Properties);
                
                if (seedsSynced)
                {
                    entity.SeedsSynced = true;
                    Console.WriteLine($"      ✓ {entityName}: Seeds sincronizados");
                }
                else
                {
                    Console.WriteLine($"      ✗ {entityName}: Seeds no sincronizados (requiere revisión manual)");
                    if (!entity.SyncErrors.Contains(seedError))
                    {
                        entity.SyncErrors.Add(seedError);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error verificando seeds: {ex.Message}";
                if (!entity.SyncErrors.Any(e => e.Contains("Error verificando seeds")))
                {
                    entity.SyncErrors.Add(errorMsg);
                }
                _logService.WriteError($"Error verificando seeds para {entity.EntityName}", ex);
            }
        }
    }

    /// <summary>
    /// Verifica que los Tests estén sincronizados
    /// </summary>
    private async Task VerifyTestsSyncAsync(List<EntitySyncStatus> entities, ComplianceState state)
    {
        foreach (var entity in entities.Where(e => !e.TestsSynced))
        {
            try
            {
                var entityName = entity.EntityName;
                
                // Limpiar errores relacionados con tests si se va a verificar de nuevo
                var testError = "Tests no sincronizados - requiere actualización manual";
                entity.SyncErrors.RemoveAll(e => e.Contains("Tests no sincronizados"));
                
                var testsSynced = await CheckTestsSyncAsync(entityName, entity.Properties);
                
                if (testsSynced)
                {
                    entity.TestsSynced = true;
                    Console.WriteLine($"      ✓ {entityName}: Tests sincronizados");
                }
                else
                {
                    Console.WriteLine($"      ✗ {entityName}: Tests no sincronizados (requiere revisión manual)");
                    if (!entity.SyncErrors.Contains(testError))
                    {
                        entity.SyncErrors.Add(testError);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error verificando tests: {ex.Message}";
                if (!entity.SyncErrors.Any(e => e.Contains("Error verificando tests")))
                {
                    entity.SyncErrors.Add(errorMsg);
                }
                _logService.WriteError($"Error verificando tests para {entity.EntityName}", ex);
            }
        }
    }

    /// <summary>
    /// Verifica si los seeds están sincronizados para una entidad
    /// </summary>
    private async Task<bool> CheckSeedsSyncAsync(string entityName, List<string> properties)
    {
        // Buscar en SetupService.SeedInitialDataAsync
        var setupServicePath = Path.Combine(_seedsPath, "Api", "Services", "SetupService.cs");
        if (File.Exists(setupServicePath))
        {
            var content = await File.ReadAllTextAsync(setupServicePath);
            // Verificar que la entidad se menciona en el seeding
            if (content.Contains(entityName, StringComparison.OrdinalIgnoreCase))
            {
                // Verificar que las propiedades principales están presentes
                // (esto es una verificación básica, puede mejorarse)
                return true;
            }
        }

        // Buscar en MasterDataSeeder
        var masterSeederPath = Path.Combine(_seedsPath, "Infrastructure", "Services", "MasterDataSeeder.cs");
        if (File.Exists(masterSeederPath))
        {
            var content = await File.ReadAllTextAsync(masterSeederPath);
            if (content.Contains(entityName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Si la entidad no necesita seeding explícito (como entidades de relación), considerar sincronizado
        var noSeedEntities = new[] { "GroupPermission", "UserGroup", "UserPermission", "PurchaseDeliveryNoteLine", "SalesDeliveryNoteLine" };
        if (noSeedEntities.Contains(entityName))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Verifica si los tests están sincronizados para una entidad
    /// </summary>
    private async Task<bool> CheckTestsSyncAsync(string entityName, List<string> properties)
    {
        // Buscar en TestDataSeeder
        var testSeederPath = Path.Combine(_testsPath, "Helpers", "TestDataSeeder.cs");
        if (File.Exists(testSeederPath))
        {
            var content = await File.ReadAllTextAsync(testSeederPath);
            if (content.Contains(entityName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Buscar tests específicos de la entidad
        var testFiles = Directory.GetFiles(_testsPath, $"*{entityName}*Tests.cs", SearchOption.AllDirectories);
        if (testFiles.Any())
        {
            return true;
        }

        // Si la entidad no necesita tests explícitos, considerar sincronizado
        var noTestEntities = new[] { "GroupPermission", "UserGroup", "UserPermission", "PurchaseDeliveryNoteLine", "SalesDeliveryNoteLine" };
        if (noTestEntities.Contains(entityName))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calcula el hash de un archivo para detectar cambios
    /// </summary>
    private string ComputeFileHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Carga el estado guardado
    /// </summary>
    private ComplianceState? LoadState()
    {
        if (!File.Exists(_stateFilePath))
            return null;

        try
        {
            var json = File.ReadAllText(_stateFilePath);
            return JsonSerializer.Deserialize<ComplianceState>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al cargar estado de cumplimiento", ex);
            return null;
        }
    }

    /// <summary>
    /// Guarda el estado actual
    /// </summary>
    private void SaveState(ComplianceState state)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(_stateFilePath, json);
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al guardar estado de cumplimiento", ex);
        }
    }

    /// <summary>
    /// Resultado del cumplimiento de reglas de oro
    /// </summary>
    public class ComplianceResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int EntitiesFound { get; set; }
        public int EntitiesChanged { get; set; }
        public int SeedsChecked { get; set; }
        public int TestsChecked { get; set; }
        public bool HasWarnings { get; set; }
    }
}
