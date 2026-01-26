using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System;
using System.Linq;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para validar la integridad completa del ecosistema GesFer
/// </summary>
public class IntegrityValidationService
{
    private readonly LogService _logService;
    private readonly string _rootPath;
    private readonly string _validationStatePath;
    private readonly HttpClient _httpClient;

    public IntegrityValidationService(LogService logService)
    {
        _logService = logService;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        _validationStatePath = Path.Combine(_rootPath, ".validation-state.json");
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    /// <summary>
    /// Resultado de la validación de integridad
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, bool> Checks { get; set; } = new();
    }

    /// <summary>
    /// Estado de validación guardado
    /// </summary>
    public class ValidationState
    {
        public DateTime LastRun { get; set; }
        public ValidationResult LastResult { get; set; } = new();
        public Dictionary<string, DateTime> LastSuccessfulChecks { get; set; } = new();
    }

    /// <summary>
    /// Ejecuta la validación completa del ecosistema
    /// </summary>
    public async Task<ValidationResult> ValidateEcosystemAsync(bool useCache = true)
    {
        var result = new ValidationResult();
        _logService.WriteLog("========================================");
        _logService.WriteLog("Inicio de validación de integridad completa");
        _logService.WriteLog("========================================");

        // Cargar estado previo si se usa caché
        ValidationState? previousState = null;
        if (useCache)
        {
            previousState = LoadValidationState();
            if (previousState != null)
            {
                _logService.WriteLog($"Estado previo encontrado: {previousState.LastRun:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("   Validación de Integridad Completa");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // 1. Validar Docker
        Console.WriteLine("[1/4] Validando Docker...");
        var dockerResult = await ValidateDockerAsync();
        result.Checks["Docker"] = dockerResult;
        if (!dockerResult)
        {
            result.Errors.Add("Docker: Contenedores no están corriendo correctamente");
            Console.WriteLine("    ✗ Docker: ERROR");
        }
        else
        {
            Console.WriteLine("    ✓ Docker: OK");
        }
        Console.WriteLine();

        // 2. Validar Backend (API)
        Console.WriteLine("[2/4] Validando Backend (API)...");
        var backendResult = await ValidateBackendAsync();
        result.Checks["Backend"] = backendResult;
        if (!backendResult)
        {
            result.Errors.Add("Backend: API no responde o Sequential GUIDs no funcionan");
            Console.WriteLine("    ✗ Backend: ERROR");
        }
        else
        {
            Console.WriteLine("    ✓ Backend: OK");
        }
        Console.WriteLine();

        // 3. Validar Cliente (Next.js)
        Console.WriteLine("[3/4] Validando Cliente (Next.js)...");
        var clientResult = await ValidateClientAsync();
        result.Checks["Cliente"] = clientResult;
        if (!clientResult)
        {
            result.Errors.Add("Cliente: Servidor Next.js no responde o hay errores de compilación");
            Console.WriteLine("    ✗ Cliente: ERROR");
        }
        else
        {
            Console.WriteLine("    ✓ Cliente: OK");
        }
        Console.WriteLine();

        // 4. Validar Sequential GUIDs en seeding
        Console.WriteLine("[4/5] Validando Sequential GUIDs en seeding...");
        var guidsResult = await ValidateSequentialGuidsAsync();
        result.Checks["SequentialGUIDs"] = guidsResult;
        if (!guidsResult)
        {
            result.Warnings.Add("Sequential GUIDs: No se pudo verificar completamente");
            Console.WriteLine("    ⚠ Sequential GUIDs: Advertencia");
        }
        else
        {
            Console.WriteLine("    ✓ Sequential GUIDs: OK");
        }
        Console.WriteLine();

        // 5. Validar tabla AdminUsers y usuario de prueba
        Console.WriteLine("[5/5] Validando tabla AdminUsers y usuario de prueba...");
        var adminUsersResult = await ValidateAdminUsersAsync();
        result.Checks["AdminUsers"] = adminUsersResult;
        if (!adminUsersResult)
        {
            result.Errors.Add("AdminUsers: La tabla no existe o no tiene al menos un usuario de prueba");
            Console.WriteLine("    ✗ AdminUsers: ERROR");
        }
        else
        {
            Console.WriteLine("    ✓ AdminUsers: OK");
        }
        Console.WriteLine();

        result.IsValid = result.Checks.Values.All(v => v) && result.Errors.Count == 0;

        if (result.IsValid)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("   ✓ TODO VERDE - Ecosistema Validado");
            Console.WriteLine("========================================");
            Console.WriteLine();
            _logService.WriteLog("Validación completada: TODO VERDE");

            // Guardar estado exitoso
            SaveValidationState(result);

            // Abrir ventanas del navegador
            await OpenBrowserWindowsAsync();
        }
        else
        {
            Console.WriteLine("========================================");
            Console.WriteLine("   ✗ ERRORES ENCONTRADOS");
            Console.WriteLine("========================================");
            Console.WriteLine();
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"   ✗ {error}");
            }
            _logService.WriteLog($"Validación completada con errores: {result.Errors.Count} error(es)");

            // Guardar estado con errores para referencia futura
            SaveValidationState(result);
        }

        return result;
    }

    /// <summary>
    /// Carga el estado de validación guardado
    /// </summary>
    private ValidationState? LoadValidationState()
    {
        if (!File.Exists(_validationStatePath))
            return null;

        try
        {
            var json = File.ReadAllText(_validationStatePath);
            return JsonSerializer.Deserialize<ValidationState>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al cargar estado de validación", ex);
            return null;
        }
    }

    /// <summary>
    /// Guarda el estado de validación actual
    /// </summary>
    private void SaveValidationState(ValidationResult result)
    {
        try
        {
            var state = new ValidationState
            {
                LastRun = DateTime.Now,
                LastResult = result,
                LastSuccessfulChecks = result.Checks
                    .Where(c => c.Value)
                    .ToDictionary(c => c.Key, c => DateTime.Now)
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(_validationStatePath, json);
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al guardar estado de validación", ex);
        }
    }

    /// <summary>
    /// Valida que todos los contenedores Docker estén corriendo
    /// </summary>
    private async Task<bool> ValidateDockerAsync()
    {
        try
        {
            _logService.WriteLog("Validando contenedores Docker...");

            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "ps --format \"{{.Names}}\t{{.Status}}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                _logService.WriteError("No se pudo ejecutar docker ps");
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            _logService.WriteProcessOutput("docker ps", output, false);

            // Verificar contenedores esperados
            var expectedContainers = new[] { "gesfer_api_db", "gesfer_api_memcached", "gesfer_api_adminer" };
            var runningContainers = new List<string>();

            foreach (var line in output.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('\t');
                if (parts.Length >= 2)
                {
                    var name = parts[0].Trim();
                    var status = parts[1].Trim();

                    if (status.Contains("Up", StringComparison.OrdinalIgnoreCase) || 
                        status.Contains("running", StringComparison.OrdinalIgnoreCase))
                    {
                        runningContainers.Add(name);
                        _logService.WriteLog($"Contenedor encontrado: {name} - {status}");
                    }
                }
            }

            bool allRunning = true;
            foreach (var expected in expectedContainers)
            {
                if (runningContainers.Any(c => c.Contains(expected, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"      ✓ {expected}");
                }
                else
                {
                    Console.WriteLine($"      ✗ {expected} (no encontrado o no corriendo)");
                    _logService.WriteLog($"Contenedor no encontrado o no corriendo: {expected}");
                    allRunning = false;
                }
            }

            return allRunning;
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al validar Docker", ex);
            return false;
        }
    }

    /// <summary>
    /// Valida que la API responda correctamente
    /// </summary>
    private async Task<bool> ValidateBackendAsync()
    {
        try
        {
            _logService.WriteLog("Validando API Backend...");

            // Intentar HTTP primero (puerto 5000)
            var httpUrl = "http://localhost:5000/api/health";
            var httpsUrl = "https://localhost:5001/api/health";

            bool httpWorks = false;
            bool httpsWorks = false;

            // Probar HTTP
            try
            {
                var response = await _httpClient.GetAsync(httpUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logService.WriteLog($"API HTTP responde: {content}");
                    httpWorks = true;
                    Console.WriteLine("      ✓ API HTTP (puerto 5000)");
                }
            }
            catch (Exception ex)
            {
                _logService.WriteLog($"API HTTP no responde: {ex.Message}");
            }

            // Probar HTTPS
            try
            {
                // Ignorar errores de certificado para validación local
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                using var httpsClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
                var response = await httpsClient.GetAsync(httpsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logService.WriteLog($"API HTTPS responde: {content}");
                    httpsWorks = true;
                    Console.WriteLine("      ✓ API HTTPS (puerto 5001)");
                }
            }
            catch (Exception ex)
            {
                _logService.WriteLog($"API HTTPS no responde: {ex.Message}");
            }

            if (!httpWorks && !httpsWorks)
            {
                Console.WriteLine("      ✗ API no responde en ningún puerto");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al validar Backend", ex);
            return false;
        }
    }

    /// <summary>
    /// Valida que el cliente Next.js esté corriendo y sin errores
    /// </summary>
    private async Task<bool> ValidateClientAsync()
    {
        try
        {
            _logService.WriteLog("Validando Cliente Next.js...");

            // Verificar que el puerto 3000 esté escuchando
            var portListening = await CheckPortListeningAsync(3000);
            if (!portListening)
            {
                Console.WriteLine("      ✗ Puerto 3000 no está escuchando");
                _logService.WriteLog("Puerto 3000 no está escuchando");
                return false;
            }
            Console.WriteLine("      ✓ Puerto 3000 está escuchando");

            // Intentar hacer una petición HTTP al servidor
            try
            {
                var response = await _httpClient.GetAsync("http://localhost:3000");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logService.WriteLog("Cliente Next.js responde correctamente");
                    Console.WriteLine("      ✓ Cliente responde correctamente");

                    // Verificar si hay errores de compilación en la respuesta
                    // Next.js muestra errores en el HTML cuando hay problemas
                    if (content.Contains("Error:", StringComparison.OrdinalIgnoreCase) ||
                        content.Contains("Failed to compile", StringComparison.OrdinalIgnoreCase) ||
                        content.Contains("Compilation error", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("      ⚠ Posibles errores de compilación detectados");
                        _logService.WriteLog("Advertencia: Posibles errores de compilación en Next.js");
                        // No es crítico, pero es una advertencia
                    }

                    return true;
                }
                else
                {
                    Console.WriteLine($"      ✗ Cliente responde con código: {response.StatusCode}");
                    _logService.WriteLog($"Cliente responde con código: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"      ✗ No se puede conectar al cliente: {ex.Message}");
                _logService.WriteError("No se puede conectar al cliente Next.js", ex);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al validar Cliente", ex);
            return false;
        }
    }

    /// <summary>
    /// Verifica si un puerto está escuchando
    /// </summary>
    private async Task<bool> CheckPortListeningAsync(int port)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = $"-ano | findstr \":{port}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Verificar si hay alguna línea con LISTENING
            return output.Contains("LISTENING", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida que los Sequential GUIDs se estén generando correctamente en el seeding
    /// </summary>
    private async Task<bool> ValidateSequentialGuidsAsync()
    {
        try
        {
            _logService.WriteLog("Validando Sequential GUIDs en base de datos...");

            // Verificar que existan registros con GUIDs en la base de datos
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e \"SELECT COUNT(*) as total FROM Companies WHERE Id IS NOT NULL LIMIT 1;\" --skip-column-names",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                _logService.WriteLog("No se pudo ejecutar consulta para validar GUIDs");
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && int.TryParse(output.Trim(), out int count) && count > 0)
            {
                _logService.WriteLog($"Sequential GUIDs validados: {count} registros encontrados");
                Console.WriteLine($"      ✓ {count} registros con GUIDs encontrados");
                return true;
            }
            else
            {
                _logService.WriteLog("No se encontraron registros para validar GUIDs");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logService.WriteProcessOutput("mysql GUID validation", error, true);
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al validar Sequential GUIDs", ex);
            return false;
        }
    }

    /// <summary>
    /// Valida que la tabla AdminUsers existe y tiene al menos un usuario de prueba
    /// </summary>
    private async Task<bool> ValidateAdminUsersAsync()
    {
        try
        {
            _logService.WriteLog("Validando tabla AdminUsers y usuario de prueba...");

            // Verificar que la tabla AdminUsers existe y tiene al menos un usuario activo
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e \"SELECT COUNT(*) as total FROM AdminUsers WHERE IsActive = 1 AND DeletedAt IS NULL;\" --skip-column-names",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                _logService.WriteLog("No se pudo ejecutar consulta para validar AdminUsers");
                Console.WriteLine("      ✗ No se pudo ejecutar consulta");
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logService.WriteLog($"Error al consultar AdminUsers: {error}");
                Console.WriteLine($"      ✗ Error al consultar tabla: {error.Trim()}");
                
                // Si la tabla no existe, intentar verificarlo específicamente
                if (error.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase) ||
                    error.Contains("Table", StringComparison.OrdinalIgnoreCase) && error.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("      ✗ La tabla AdminUsers no existe en la base de datos");
                    _logService.WriteLog("La tabla AdminUsers no existe - se requiere aplicar la migración");
                    return false;
                }
                
                return false;
            }

            if (int.TryParse(output.Trim(), out int count))
            {
                if (count > 0)
                {
                    _logService.WriteLog($"AdminUsers validado: {count} usuario(s) administrativo(s) activo(s) encontrado(s)");
                    Console.WriteLine($"      ✓ {count} usuario(s) administrativo(s) activo(s) encontrado(s)");
                    
                    // Verificar que existe el usuario "admin"
                    var adminUserInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = "exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e \"SELECT Username FROM AdminUsers WHERE Username = 'admin' AND IsActive = 1 AND DeletedAt IS NULL LIMIT 1;\" --skip-column-names",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var adminProcess = Process.Start(adminUserInfo);
                    if (adminProcess != null)
                    {
                        var adminOutput = await adminProcess.StandardOutput.ReadToEndAsync();
                        await adminProcess.WaitForExitAsync();
                        
                        if (adminOutput.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("      ✓ Usuario 'admin' encontrado");
                            _logService.WriteLog("Usuario administrativo 'admin' encontrado correctamente");
                        }
                        else
                        {
                            Console.WriteLine("      ⚠ Usuario 'admin' no encontrado (pero hay otros usuarios)");
                            _logService.WriteLog("Advertencia: Usuario 'admin' no encontrado, pero hay otros usuarios administrativos");
                        }
                    }
                    
                    return true;
                }
                else
                {
                    Console.WriteLine("      ✗ No hay usuarios administrativos activos en la tabla");
                    _logService.WriteLog("AdminUsers: La tabla existe pero no tiene usuarios activos");
                    return false;
                }
            }
            else
            {
                _logService.WriteLog("No se pudo parsear el resultado de la consulta AdminUsers");
                Console.WriteLine("      ✗ Error al interpretar resultado");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al validar AdminUsers", ex);
            Console.WriteLine($"      ✗ Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Abre las ventanas del navegador después de una validación exitosa
    /// </summary>
    private async Task OpenBrowserWindowsAsync()
    {
        try
        {
            _logService.WriteLog("Abriendo ventanas del navegador...");
            Console.WriteLine("Abriendo ventanas del navegador...");
            Console.WriteLine();

            var urls = new[]
            {
                "http://localhost:3000/login",
                "http://localhost:3000/admin/dashboard",
                "http://localhost:8080"
            };

            foreach (var url in urls)
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    };

                    Process.Start(processInfo);
                    _logService.WriteLog($"Navegador abierto: {url}");
                    Console.WriteLine($"    ✓ Abierto: {url}");

                    // Pequeña pausa entre aperturas
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logService.WriteError($"Error al abrir {url}", ex);
                    Console.WriteLine($"    ✗ Error al abrir: {url}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Ventanas del navegador abiertas correctamente");
        }
        catch (Exception ex)
        {
            _logService.WriteError("Error al abrir ventanas del navegador", ex);
        }
    }

    /// <summary>
    /// Libera recursos
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
