using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Commands;

public class StartLocalEnvironmentInput { }

public class StartLocalEnvironmentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class StartLocalEnvironmentCommand
{
    private readonly LogService _logService;
    private readonly string _rootPath;
    private readonly List<Process> _runningProcesses = new();

    public StartLocalEnvironmentCommand(LogService logService)
    {
        _logService = logService;
        _rootPath = logService.GetRootPath();
    }

    public async Task<StartLocalEnvironmentResult> HandleAsync(StartLocalEnvironmentInput input)
    {
        Console.WriteLine("Iniciando entorno local...");
        _logService.WriteLog("Iniciando entorno local - Opción 2");

        try
        {
            // 1. Obtener configuración y puertos dinámicamente
            var productApiConfig = GetDotNetProjectConfig("src/Product/Back/Api/Properties/launchSettings.json", "http");
            var adminApiConfig = GetDotNetProjectConfig("src/Admin/Back/Api/Properties/launchSettings.json", "http");
            var productFrontPort = GetNpmProjectPort("src/Product/Front/package.json", 3000);
            var adminFrontPort = GetNpmProjectPort("src/Admin/Front/package.json", 3001);

            // 2. Compilar Backends
            if (!await BuildDotNetProjectAsync("src/Product/Back/Api/GesFer.Api.csproj", "Product API")) return Fail();
            if (!await BuildDotNetProjectAsync("src/Admin/Back/Api/GesFer.Admin.Api.csproj", "Admin API")) return Fail();

            // 3. Preparar Frontends (npm install)
            await PrepareNpmProjectAsync("src/Product/Front", "Product Front");
            await PrepareNpmProjectAsync("src/Admin/Front", "Admin Front");

            // Crear directorio de logs si no existe
            var logsDir = Path.Combine(_rootPath, "logs", "services");
            Directory.CreateDirectory(logsDir);

            // 4. Levantar Servicios
            Console.WriteLine("Levantando servicios en segundo plano...");

            // Product API
            StartDotNetProcess("src/Product/Back/Api/GesFer.Api.csproj", "ProductApi", logsDir);

            // Admin API
            StartDotNetProcess("src/Admin/Back/Api/GesFer.Admin.Api.csproj", "AdminApi", logsDir);

            // Product Front
            StartNpmProcess("src/Product/Front", "ProductFront", logsDir);

            // Admin Front
            StartNpmProcess("src/Admin/Front", "AdminFront", logsDir);

            // 5. Mostrar Información
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("   Entorno Local en Ejecución");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("Servicios:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ➜ Back Product:  {productApiConfig.Url ?? "http://localhost:5000"}");
            Console.WriteLine($"  ➜ Back Admin:    {adminApiConfig.Url ?? "http://localhost:5049"}");
            Console.WriteLine($"  ➜ Front Product: http://localhost:{productFrontPort}");
            Console.WriteLine($"  ➜ Front Admin:   http://localhost:{adminFrontPort}");
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
            return new StartLocalEnvironmentResult { Success = true, Message = "Entorno detenido correctamente" };
        }
        catch (Exception ex)
        {
            StopAllProcesses();
            _logService.WriteError("Error al levantar entorno local", ex);
            return new StartLocalEnvironmentResult { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    private StartLocalEnvironmentResult Fail()
    {
        return new StartLocalEnvironmentResult { Success = false, Message = "Fallo en la preparación del entorno." };
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

        process.OutputDataReceived += (s, e) => { if (e.Data != null) File.AppendAllText(logFile, $"{DateTime.Now}: {e.Data}\n"); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) File.AppendAllText(logFile, $"ERROR {DateTime.Now}: {e.Data}\n"); };

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

        process.OutputDataReceived += (s, e) => { if (e.Data != null) File.AppendAllText(logFile, $"{DateTime.Now}: {e.Data}\n"); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) File.AppendAllText(logFile, $"ERROR {DateTime.Now}: {e.Data}\n"); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        _runningProcesses.Add(process);
    }
}
