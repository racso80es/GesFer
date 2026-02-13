using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Commands;

public class StartLocalEnvironmentInput
{
    public bool StartProductApi { get; set; }
    public bool StartAdminApi { get; set; }
    public bool StartProductFront { get; set; }
    public bool StartAdminFront { get; set; }

    public bool IsStartAll => !StartProductApi && !StartAdminApi && !StartProductFront && !StartAdminFront;
}

public class StartLocalEnvironmentCommand : ICommandHandler<StartLocalEnvironmentInput>
{
    private readonly LogService _logService;
    private readonly string _rootPath;
    private readonly List<Process> _runningProcesses = new();
    private static readonly ConcurrentDictionary<string, object> _fileLocks = new();

    public StartLocalEnvironmentCommand(LogService logService)
    {
        _logService = logService;
        _rootPath = logService.GetRootPath();
    }

    public async Task<CommandResult> HandleAsync(StartLocalEnvironmentInput input)
    {
        Console.WriteLine("Iniciando entorno local...");
        _logService.WriteLog("Iniciando entorno local - Opción 2/3");

        var startAll = input.IsStartAll;
        if (startAll)
        {
            input.StartProductApi = true;
            input.StartAdminApi = true;
            input.StartProductFront = true;
            input.StartAdminFront = true;
        }

        try
        {
            // 1. Obtener configuración y puertos dinámicamente
            var productApiConfig = GetDotNetProjectConfig("src/Product/Back/Api/Properties/launchSettings.json", "http");
            var adminApiConfig = GetDotNetProjectConfig("src/Admin/Back/Api/Properties/launchSettings.json", "http");
            var productFrontPort = GetNpmProjectPort("src/Product/Front/package.json", 3000);
            var adminFrontPort = GetNpmProjectPort("src/Admin/Front/package.json", 3001);

            // 2. Compilar Backends
            if (input.StartProductApi)
            {
                if (!await BuildDotNetProjectAsync("src/Product/Back/Api/GesFer.Api.csproj", "Product API"))
                    return CommandResult.Fail("Fallo en la preparación del entorno (Compilación Product API).");
            }

            if (input.StartAdminApi)
            {
                if (!await BuildDotNetProjectAsync("src/Admin/Back/Api/GesFer.Admin.Api.csproj", "Admin API"))
                    return CommandResult.Fail("Fallo en la preparación del entorno (Compilación Admin API).");
            }

            // 3. Preparar Frontends (npm install)
            if (input.StartProductFront)
            {
                await PrepareNpmProjectAsync("src/Product/Front", "Product Front");
            }
            if (input.StartAdminFront)
            {
                await PrepareNpmProjectAsync("src/Admin/Front", "Admin Front");
            }

            // Crear directorio de logs si no existe
            var logsDir = Path.Combine(_rootPath, "logs", "services");
            Directory.CreateDirectory(logsDir);

            // 4. Liberar puertos
            Console.WriteLine("Liberando puertos...");
            if (input.StartProductApi) FreePort(5000); // Product API
            if (input.StartAdminApi) FreePort(5010); // Admin API (según launchSettings)
            if (input.StartProductFront) FreePort(productFrontPort);
            if (input.StartAdminFront) FreePort(adminFrontPort);

            // 5. Levantar Servicios
            Console.WriteLine("Levantando servicios en segundo plano...");

            // Product API
            if (input.StartProductApi)
                StartDotNetProcess("src/Product/Back/Api/GesFer.Api.csproj", "ProductApi", logsDir);

            // Admin API
            if (input.StartAdminApi)
                StartDotNetProcess("src/Admin/Back/Api/GesFer.Admin.Api.csproj", "AdminApi", logsDir);

            // Product Front
            if (input.StartProductFront)
                StartNpmProcess("src/Product/Front", "ProductFront", logsDir);

            // Admin Front
            if (input.StartAdminFront)
                StartNpmProcess("src/Admin/Front", "AdminFront", logsDir);

            // 6. Mostrar Información
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("   Entorno Local en Ejecución");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("Servicios:");
            Console.ForegroundColor = ConsoleColor.Green;
            if (input.StartProductApi) Console.WriteLine($"  ➜ Back Product:  {productApiConfig.Url ?? "http://localhost:5000"}");
            if (input.StartAdminApi) Console.WriteLine($"  ➜ Back Admin:    {adminApiConfig.Url ?? "http://localhost:5010"}");
            if (input.StartProductFront) Console.WriteLine($"  ➜ Front Product: http://localhost:{productFrontPort}");
            if (input.StartAdminFront) Console.WriteLine($"  ➜ Front Admin:   http://localhost:{adminFrontPort}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Logs disponibles en: {logsDir}");
            Console.WriteLine();
            Console.WriteLine("Presione 'q' para detener todos los servicios y salir.");

            // Loop de espera
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                    {
                        break;
                    }
                }
                await Task.Delay(100);
            }

            StopAllProcesses();
            return CommandResult.Ok("Entorno detenido correctamente");
        }
        catch (Exception ex)
        {
            StopAllProcesses();
            _logService.WriteError("Error al levantar entorno local", ex);
            return CommandResult.Fail($"Error: {ex.Message}");
        }
    }

    private void StopAllProcesses()
    {
        Console.WriteLine();
        Console.WriteLine("Deteniendo servicios...");
        foreach (var process in _runningProcesses)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true); // Kill tree
                }
            }
            catch { }
        }
        _runningProcesses.Clear();
        Console.WriteLine("Servicios detenidos.");
    }

    // --- Helpers de Configuración ---

    private (string? Url, int Port) GetDotNetProjectConfig(string relativePath, string profileName)
    {
        try
        {
            var fullPath = Path.Combine(_rootPath, relativePath);
            if (!File.Exists(fullPath)) return (null, 0);

            var jsonContent = File.ReadAllText(fullPath);
            using var doc = JsonDocument.Parse(jsonContent);

            if (doc.RootElement.TryGetProperty("profiles", out var profiles) &&
                profiles.TryGetProperty(profileName, out var profile) &&
                profile.TryGetProperty("applicationUrl", out var appUrl))
            {
                var appUrlStr = appUrl.GetString();
                if (appUrlStr != null)
                {
                    var urls = appUrlStr.Split(';');
                    var httpUrl = urls.FirstOrDefault(u => u.StartsWith("http://"));
                    return (httpUrl, 0);
                }
            }
        }
        catch (Exception ex)
        {
            _logService.WriteError($"Error leyendo launchSettings en {relativePath}", ex);
        }
        return (null, 0);
    }

    private int GetNpmProjectPort(string relativePath, int defaultPort)
    {
        try
        {
            var fullPath = Path.Combine(_rootPath, relativePath);
            if (!File.Exists(fullPath)) return defaultPort;

            var jsonContent = File.ReadAllText(fullPath);
            using var doc = JsonDocument.Parse(jsonContent);

            if (doc.RootElement.TryGetProperty("scripts", out var scripts) &&
                scripts.TryGetProperty("dev", out var devScript))
            {
                var scriptContent = devScript.GetString();
                if (scriptContent != null)
                {
                    var parts = scriptContent.Split(' ');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i] == "-p" && i + 1 < parts.Length)
                        {
                            if (int.TryParse(parts[i+1], out int port))
                            {
                                return port;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logService.WriteError($"Error leyendo package.json en {relativePath}", ex);
        }
        return defaultPort;
    }

    // --- Helpers de Proceso ---

    private async Task<bool> BuildDotNetProjectAsync(string projectRelPath, string name)
    {
        Console.WriteLine($"Compilando {name}...");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{Path.Combine(_rootPath, projectRelPath)}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error compilando {name}.");
            Console.WriteLine(await process.StandardError.ReadToEndAsync());
            Console.ResetColor();
            return false;
        }
        Console.WriteLine($"✓ {name} compilado.");
        return true;
    }

    private async Task PrepareNpmProjectAsync(string projectRelPath, string name)
    {
        Console.WriteLine($"Preparando {name} (npm install)...");
        var path = Path.Combine(_rootPath, projectRelPath);

        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var fileName = isWindows ? "cmd" : "npm";
        var args = isWindows ? $"/c cd \"{path}\" && npm install" : $"install --prefix \"{path}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0) Console.WriteLine($"✓ {name} dependencias listas.");
        else Console.WriteLine($"⚠ Advertencia: {name} npm install retornó {process.ExitCode}");
    }

    private void StartDotNetProcess(string projectRelPath, string name, string logsDir)
    {
        var logFile = Path.Combine(logsDir, $"{name}.log");
        var projectPath = Path.Combine(_rootPath, projectRelPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\" --no-build",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (s, e) => { if (e.Data != null) WriteLogSafe(logFile, $"{DateTime.Now}: {e.Data}\n"); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) WriteLogSafe(logFile, $"ERROR {DateTime.Now}: {e.Data}\n"); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        _runningProcesses.Add(process);
    }

    private void StartNpmProcess(string projectRelPath, string name, string logsDir)
    {
        var logFile = Path.Combine(logsDir, $"{name}.log");
        var path = Path.Combine(_rootPath, projectRelPath);

        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var fileName = isWindows ? "cmd" : "npm";
        var args = isWindows ? $"/c cd \"{path}\" && npm run dev" : $"run dev --prefix \"{path}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (s, e) => { if (e.Data != null) WriteLogSafe(logFile, $"{DateTime.Now}: {e.Data}\n"); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) WriteLogSafe(logFile, $"ERROR {DateTime.Now}: {e.Data}\n"); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        _runningProcesses.Add(process);
    }

    private void WriteLogSafe(string filepath, string content)
    {
        var lockObj = _fileLocks.GetOrAdd(filepath, _ => new object());
        lock (lockObj)
        {
            try
            {
                File.AppendAllText(filepath, content);
            }
            catch
            {
                // Ignore errors to avoid crashing the main process
            }
        }
    }

    private void FreePort(int port)
    {
        if (port <= 0) return;
        try
        {
            var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            if (isWindows)
            {
                // Windows: netstat para encontrar PID y taskkill para matar
                // Usamos powershell para ser más precisos que cmd y findstr
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = $"-Command \"Get-NetTCPConnection -LocalPort {port} -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess | ForEach-Object {{ Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            else
            {
                // Linux/Mac: lsof + kill
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "bash",
                        Arguments = $"-c \"lsof -t -i:{port} | xargs -r kill -9\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
                process.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            _logService.WriteError($"Error liberando puerto {port}", ex);
        }
    }
}
