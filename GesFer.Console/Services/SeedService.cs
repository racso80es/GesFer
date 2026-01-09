using System.Diagnostics;
using System;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para ejecutar scripts SQL de seed
/// </summary>
public class SeedService
{
    private readonly string _rootPath;
    private readonly LogService _logService;

    public SeedService(LogService logService)
    {
        _logService = logService;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
    }

    /// <summary>
    /// Ejecuta un script SQL en el contenedor MySQL
    /// </summary>
    private async Task<bool> ExecuteSqlScriptAsync(string scriptPath, string scriptName)
    {
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine($"    ⚠ No se encontró el archivo {scriptName}");
            Console.WriteLine($"       Buscado en: {scriptPath}");
            _logService.WriteLog($"⚠ No se encontró el archivo {scriptName} en {scriptPath}");
            return false;
        }

        Console.WriteLine($"    Ejecutando {scriptName}...");
        _logService.WriteLog($"Ejecutando script SQL: {scriptName}");

        try
        {
            // Leer el contenido del script
            var scriptContent = await File.ReadAllTextAsync(scriptPath);
            _logService.WriteLog($"Tamaño del script: {scriptContent.Length} caracteres");

            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logService.WriteLog($"Comando: docker {processInfo.Arguments}");

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                var errorMsg = $"No se pudo iniciar docker para ejecutar {scriptName}";
                Console.WriteLine($"    ERROR: {errorMsg}");
                _logService.WriteError(errorMsg);
                return false;
            }

            // Escribir el script en la entrada estándar
            await process.StandardInput.WriteAsync(scriptContent);
            process.StandardInput.Close();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            _logService.WriteProcessOutput($"mysql exec {scriptName}", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput($"mysql exec {scriptName}", error, true);
            }
            _logService.WriteLog($"Código de salida: {process.ExitCode}");

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"    ✓ {scriptName} ejecutado correctamente");
                _logService.WriteLog($"{scriptName} ejecutado correctamente");
                return true;
            }
            else
            {
                Console.WriteLine($"    ⚠ Error al ejecutar {scriptName} (puede que algunos datos ya existan)");
                _logService.WriteLog($"⚠ Error al ejecutar {scriptName} (código: {process.ExitCode})");
                return true; // No es crítico si algunos datos ya existen
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al ejecutar {scriptName}: {ex.Message}";
            Console.WriteLine($"    ⚠ Error al ejecutar {scriptName} (puede que algunos datos ya existan): {ex.Message}");
            _logService.WriteError(errorMsg, ex);
            return true; // No es crítico
        }
    }

    /// <summary>
    /// Ejecuta el script de datos maestros
    /// </summary>
    public async Task<bool> ExecuteMasterDataAsync()
    {
        Console.WriteLine("Insertando datos maestros...");
        var scriptPath = Path.Combine(_rootPath, "Api", "scripts", "master-data.sql");
        return await ExecuteSqlScriptAsync(scriptPath, "master-data.sql");
    }

    /// <summary>
    /// Ejecuta el script de datos de muestra
    /// </summary>
    public async Task<bool> ExecuteSampleDataAsync()
    {
        Console.WriteLine("Insertando datos de muestra...");
        var scriptPath = Path.Combine(_rootPath, "Api", "scripts", "sample-data.sql");
        return await ExecuteSqlScriptAsync(scriptPath, "sample-data.sql");
    }

    /// <summary>
    /// Ejecuta el script de datos de prueba
    /// </summary>
    public async Task<bool> ExecuteTestDataAsync()
    {
        Console.WriteLine("Insertando datos de prueba...");
        var scriptPath = Path.Combine(_rootPath, "Api", "scripts", "test-data.sql");
        return await ExecuteSqlScriptAsync(scriptPath, "test-data.sql");
    }

    /// <summary>
    /// Ejecuta todos los scripts de seed en orden
    /// </summary>
    public async Task<bool> ExecuteAllSeedsAsync()
    {
        var masterResult = await ExecuteMasterDataAsync();
        await Task.Delay(500); // Pequeña pausa entre scripts

        var sampleResult = await ExecuteSampleDataAsync();
        await Task.Delay(500);

        var testResult = await ExecuteTestDataAsync();

        return masterResult && sampleResult && testResult;
    }
}
