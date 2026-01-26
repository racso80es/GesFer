using System.Diagnostics;
using System;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para gestionar contenedores Docker
/// </summary>
public class DockerService
{
    private readonly string _apiPath;
    private readonly LogService _logService;

    public DockerService(LogService logService)
    {
        _logService = logService;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        _apiPath = Path.GetFullPath(Path.Combine(rootPath, "Api"));
    }

    /// <summary>
    /// Verifica si Docker está corriendo
    /// </summary>
    public async Task<bool> IsDockerRunningAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
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

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Elimina contenedores Docker existentes
    /// </summary>
    public async Task<bool> RemoveContainersAsync()
    {
        Console.WriteLine("Limpiando contenedores existentes...");
        _logService.WriteLog("Limpiando contenedores existentes...");

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = "down -v",
                WorkingDirectory = _apiPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logService.WriteLog($"Comando: docker-compose {processInfo.Arguments}");
            _logService.WriteLog($"Directorio de trabajo: {_apiPath}");

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                var errorMsg = "No se pudo iniciar docker-compose";
                Console.WriteLine($"    ERROR: {errorMsg}");
                _logService.WriteError(errorMsg);
                return false;
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            _logService.WriteProcessOutput("docker-compose down", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput("docker-compose down", error, true);
            }
            _logService.WriteLog($"Código de salida: {process.ExitCode}");

            if (process.ExitCode == 0)
            {
                Console.WriteLine("    ✓ Contenedores eliminados");
                _logService.WriteLog("Contenedores eliminados correctamente");
                return true;
            }
            else
            {
                Console.WriteLine("    ⚠ No se pudieron detener contenedores (puede que no existan)");
                _logService.WriteLog("No se pudieron detener contenedores (puede que no existan)");
                return true; // No es un error crítico si no hay contenedores
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al eliminar contenedores: {ex.Message}";
            Console.WriteLine($"    ERROR: {ex.Message}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Crea contenedores Docker
    /// </summary>
    public async Task<bool> CreateContainersAsync()
    {
        Console.WriteLine("Creando contenedores Docker...");
        _logService.WriteLog("Creando contenedores Docker...");

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = "up -d",
                WorkingDirectory = _apiPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logService.WriteLog($"Comando: docker-compose {processInfo.Arguments}");
            _logService.WriteLog($"Directorio de trabajo: {_apiPath}");

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                var errorMsg = "No se pudo iniciar docker-compose";
                Console.WriteLine($"    ERROR: {errorMsg}");
                _logService.WriteError(errorMsg);
                return false;
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            _logService.WriteProcessOutput("docker-compose up", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput("docker-compose up", error, true);
            }
            _logService.WriteLog($"Código de salida: {process.ExitCode}");

            if (process.ExitCode == 0)
            {
                Console.WriteLine("    ✓ Contenedores creados");
                _logService.WriteLog("Contenedores creados correctamente");
                return true;
            }
            else
            {
                Console.WriteLine("    ERROR: No se pudieron crear los contenedores");
                _logService.WriteError("No se pudieron crear los contenedores");
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al crear contenedores: {ex.Message}";
            Console.WriteLine($"    ERROR: {ex.Message}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Espera a que MySQL esté listo
    /// </summary>
    public async Task<bool> WaitForMySqlReadyAsync(int maxAttempts = 30, int delaySeconds = 2)
    {
        Console.WriteLine("Esperando a que MySQL esté listo...");
        _logService.WriteLog($"Esperando a que MySQL esté listo (máximo {maxAttempts} intentos)...");

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    var output = await outputTask;
                    var error = await errorTask;

                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine("    ✓ MySQL está listo");
                        _logService.WriteLog($"MySQL está listo después de {attempt} intentos");
                        // Esperar un poco más para asegurar que MySQL esté completamente listo
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        return true;
                    }
                    else
                    {
                        _logService.WriteLog($"Intento {attempt}/{maxAttempts} fallido (código: {process.ExitCode})");
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            _logService.WriteProcessOutput($"mysqladmin ping (intento {attempt})", error, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.WriteLog($"Excepción en intento {attempt}: {ex.Message}");
                // Continuar intentando
            }

            if (attempt < maxAttempts)
            {
                Console.WriteLine($"    Intento {attempt}/{maxAttempts}...");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

        var errorMsg = $"MySQL no está listo después de {maxAttempts} intentos";
        Console.WriteLine($"    ERROR: {errorMsg}");
        _logService.WriteError(errorMsg);
        return false;
    }
}
