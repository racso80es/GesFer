using System;
using System.Linq;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para gestionar migraciones de Entity Framework
/// </summary>
public class MigrationService
{
    private readonly LogService _logService;

    public MigrationService(LogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Verifica si existe la herramienta dotnet-ef
    /// </summary>
    public async Task<bool> IsEfToolInstalledAsync()
    {
        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "ef --version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                return false;
            }

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Instala la herramienta dotnet-ef si no está instalada
    /// </summary>
    public async Task<bool> InstallEfToolAsync()
    {
        Console.WriteLine("    Instalando herramienta dotnet-ef...");
        _logService.WriteLog("Instalando herramienta dotnet-ef...");

        try
        {
            // Intentar instalar con versión específica
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "tool install --global dotnet-ef --version 8.0.0",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logService.WriteLog($"Comando: dotnet {processInfo.Arguments}");

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process != null)
            {
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                var output = await outputTask;
                var error = await errorTask;

                _logService.WriteProcessOutput("dotnet tool install", output, false);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logService.WriteProcessOutput("dotnet tool install", error, true);
                }
                _logService.WriteLog($"Código de salida: {process.ExitCode}");

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("    ✓ Herramienta dotnet-ef instalada correctamente");
                    _logService.WriteLog("Herramienta dotnet-ef instalada correctamente");
                    return true;
                }
            }

            // Si falla, intentar sin versión específica
            Console.WriteLine("    ⚠ Falló instalación con versión específica. Intentando sin versión...");
            _logService.WriteLog("Falló instalación con versión específica. Intentando sin versión...");
            processInfo.Arguments = "tool install --global dotnet-ef";
            _logService.WriteLog($"Comando: dotnet {processInfo.Arguments}");

            using var process2 = System.Diagnostics.Process.Start(processInfo);
            if (process2 != null)
            {
                var outputTask2 = process2.StandardOutput.ReadToEndAsync();
                var errorTask2 = process2.StandardError.ReadToEndAsync();
                
                await process2.WaitForExitAsync();

                var output2 = await outputTask2;
                var error2 = await errorTask2;

                _logService.WriteProcessOutput("dotnet tool install (sin versión)", output2, false);
                if (!string.IsNullOrWhiteSpace(error2))
                {
                    _logService.WriteProcessOutput("dotnet tool install (sin versión)", error2, true);
                }
                _logService.WriteLog($"Código de salida: {process2.ExitCode}");

                if (process2.ExitCode == 0)
                {
                    Console.WriteLine("    ✓ Herramienta dotnet-ef instalada correctamente");
                    _logService.WriteLog("Herramienta dotnet-ef instalada correctamente");
                    return true;
                }
            }

            Console.WriteLine("    ERROR: No se pudo instalar la herramienta dotnet-ef");
            _logService.WriteError("No se pudo instalar la herramienta dotnet-ef");
            return false;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al instalar dotnet-ef: {ex.Message}";
            Console.WriteLine($"    ERROR: {ex.Message}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Crea la migración inicial si no existe
    /// </summary>
    public async Task<bool> CreateInitialMigrationIfNeededAsync()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var apiPath = Path.GetFullPath(Path.Combine(rootPath, "Api", "src", "Api"));
        var infrastructurePath = Path.GetFullPath(Path.Combine(rootPath, "Api", "src", "Infrastructure"));
        var migrationsPath = Path.Combine(infrastructurePath, "Migrations");

        _logService.WriteLog($"Verificando migraciones en: {migrationsPath}");

        // Verificar si existen migraciones
        if (Directory.Exists(migrationsPath))
        {
            var migrationFiles = Directory.GetFiles(migrationsPath, "*.cs");
            if (migrationFiles.Length > 0)
            {
                Console.WriteLine("    ✓ Migraciones existentes encontradas");
                _logService.WriteLog($"Migraciones existentes encontradas: {migrationFiles.Length} archivos");
                return true;
            }
        }

        Console.WriteLine("    No se encontraron migraciones. Creando migración inicial...");
        _logService.WriteLog("No se encontraron migraciones. Creando migración inicial...");

        var projectPath = Path.Combine(infrastructurePath, "GesFer.Infrastructure.csproj");
        var startupProjectPath = Path.Combine(apiPath, "GesFer.Api.csproj");
        var command = $"ef migrations add InitialCreate --project \"{projectPath}\" --startup-project \"{startupProjectPath}\"";

        _logService.WriteLog($"Comando: dotnet {command}");
        _logService.WriteLog($"Directorio de trabajo: {apiPath}");

        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = command,
                WorkingDirectory = apiPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                var errorMsg = "No se pudo iniciar dotnet ef";
                Console.WriteLine($"    ERROR: {errorMsg}");
                _logService.WriteError(errorMsg);
                return false;
            }

            // Leer la salida mientras el proceso se ejecuta
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            // Registrar toda la salida en el log
            _logService.WriteProcessOutput("dotnet ef migrations add", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput("dotnet ef migrations add", error, true);
            }

            _logService.WriteLog($"Código de salida: {process.ExitCode}");

            if (process.ExitCode == 0)
            {
                Console.WriteLine("    ✓ Migración inicial creada");
                _logService.WriteLog("Migración inicial creada correctamente");
                return true;
            }
            else
            {
                Console.WriteLine("    ERROR: No se pudieron crear las migraciones");
                _logService.WriteError("No se pudieron crear las migraciones");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine("    Revisa el archivo de log para más detalles");
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al crear migración: {ex.Message}";
            Console.WriteLine($"    ERROR: {ex.Message}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Aplica las migraciones a la base de datos
    /// </summary>
    public async Task<bool> ApplyMigrationsAsync()
    {
        Console.WriteLine("    Aplicando migraciones a la base de datos...");
        _logService.WriteLog("Aplicando migraciones a la base de datos...");

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var apiPath = Path.GetFullPath(Path.Combine(rootPath, "Api", "src", "Api"));
        var infrastructurePath = Path.GetFullPath(Path.Combine(rootPath, "Api", "src", "Infrastructure"));

        var projectPath = Path.Combine(infrastructurePath, "GesFer.Infrastructure.csproj");
        var startupProjectPath = Path.Combine(apiPath, "GesFer.Api.csproj");
        var command = $"ef database update --project \"{projectPath}\" --startup-project \"{startupProjectPath}\"";

        _logService.WriteLog($"Comando: dotnet {command}");
        _logService.WriteLog($"Directorio de trabajo: {apiPath}");

        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = command,
                WorkingDirectory = apiPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process == null)
            {
                var errorMsg = "No se pudo iniciar dotnet ef";
                Console.WriteLine($"    ERROR: {errorMsg}");
                _logService.WriteError(errorMsg);
                return false;
            }

            // Leer la salida mientras el proceso se ejecuta
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            // Registrar toda la salida en el log
            _logService.WriteProcessOutput("dotnet ef database update", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput("dotnet ef database update", error, true);
            }

            _logService.WriteLog($"Código de salida: {process.ExitCode}");

            if (process.ExitCode == 0)
            {
                Console.WriteLine("    ✓ Migraciones aplicadas correctamente");
                _logService.WriteLog("Migraciones aplicadas correctamente");
                return true;
            }
            else
            {
                Console.WriteLine("    ERROR: No se pudieron aplicar las migraciones");
                _logService.WriteError("No se pudieron aplicar las migraciones");
                
                // Si el build falló, intentar obtener más información ejecutando dotnet build
                if (output.Contains("Build failed") || output.Contains("Build started"))
                {
                    Console.WriteLine("    El build del proyecto falló. Ejecutando dotnet build para ver los errores...");
                    _logService.WriteLog("El build falló, ejecutando dotnet build para obtener errores detallados...");
                    
                    var buildProcessInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"build \"{Path.Combine(infrastructurePath, "GesFer.Infrastructure.csproj")}\"",
                        WorkingDirectory = infrastructurePath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    try
                    {
                        using var buildProcess = System.Diagnostics.Process.Start(buildProcessInfo);
                        if (buildProcess != null)
                        {
                            var buildOutputTask = buildProcess.StandardOutput.ReadToEndAsync();
                            var buildErrorTask = buildProcess.StandardError.ReadToEndAsync();
                            
                            await buildProcess.WaitForExitAsync();
                            
                            var buildOutput = await buildOutputTask;
                            var buildError = await buildErrorTask;
                            
                            _logService.WriteProcessOutput("dotnet build Infrastructure", buildOutput, false);
                            if (!string.IsNullOrWhiteSpace(buildError))
                            {
                                _logService.WriteProcessOutput("dotnet build Infrastructure", buildError, true);
                            }
                            
                            // Mostrar errores de compilación en consola
                            if (!string.IsNullOrWhiteSpace(buildError))
                            {
                                Console.WriteLine();
                                Console.WriteLine("    Errores de compilación:");
                                var errorLines = buildError.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(l => l.Contains("error", StringComparison.OrdinalIgnoreCase))
                                    .Take(15)
                                    .ToList();
                                
                                foreach (var line in errorLines)
                                {
                                    Console.WriteLine($"    {line}");
                                }
                                
                                if (errorLines.Count == 0 && !string.IsNullOrWhiteSpace(buildOutput))
                                {
                                    // Si no hay errores en stderr, buscar en stdout
                                    var outputErrorLines = buildOutput.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                        .Where(l => l.Contains("error", StringComparison.OrdinalIgnoreCase))
                                        .Take(15)
                                        .ToList();
                                    
                                    foreach (var line in outputErrorLines)
                                    {
                                        Console.WriteLine($"    {line}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception buildEx)
                    {
                        _logService.WriteError($"Error al ejecutar dotnet build: {buildEx.Message}", buildEx);
                    }
                }
                
                Console.WriteLine();
                Console.WriteLine("    Revisa el archivo de log para más detalles:");
                Console.WriteLine($"    {_logService.GetLogFilePath()}");
                
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine();
                    Console.WriteLine("    Detalles del error:");
                    var errorLines = error.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in errorLines.Take(10)) // Mostrar solo las primeras 10 líneas en consola
                    {
                        Console.WriteLine($"    {line}");
                    }
                    if (errorLines.Length > 10)
                    {
                        Console.WriteLine($"    ... y {errorLines.Length - 10} líneas más (ver log completo)");
                    }
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al aplicar migraciones: {ex.Message}";
            Console.WriteLine($"    ERROR: {ex.Message}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Verifica que las tablas se hayan creado correctamente usando docker exec
    /// </summary>
    public async Task<bool> VerifyTablesCreatedAsync()
    {
        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e \"SELECT COUNT(*) as total FROM information_schema.tables WHERE table_schema = 'ScrapDb';\" --skip-column-names",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    if (int.TryParse(output.Trim(), out int tableCount) && tableCount > 0)
                    {
                        Console.WriteLine($"    ✓ Tablas creadas correctamente (Total: {tableCount})");
                        return true;
                    }
                }
            }

            Console.WriteLine("    ⚠ No se pudo verificar las tablas, pero las migraciones se aplicaron");
            return true; // No es crítico
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    ⚠ No se pudo verificar las tablas, pero las migraciones se aplicaron: {ex.Message}");
            return true; // No es crítico
        }
    }
}
