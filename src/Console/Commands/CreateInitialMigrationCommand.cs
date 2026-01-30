using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Commands;

public class CreateInitialMigrationCommand : ICommandHandler<CreateInitialMigrationInput, bool>
{
    private readonly LogService _logService;

    public CreateInitialMigrationCommand(LogService logService)
    {
        _logService = logService;
    }

    public async Task<CommandResult<bool>> HandleAsync(CreateInitialMigrationInput input)
    {
        var result = new CommandResult<bool>();
        result.Data = false;

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        // Ir 5 niveles arriba para llegar a la raíz del repositorio
        var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        var apiPath = Path.GetFullPath(Path.Combine(rootPath, "src", "Product", "Back", "Api"));
        var infrastructurePath = Path.GetFullPath(Path.Combine(rootPath, "src", "Product", "Back", "Infrastructure"));
        var migrationsPath = Path.Combine(infrastructurePath, "Migrations");

        _logService.WriteLog($"Verificando migraciones en: {migrationsPath}");

        // Verificar si existen migraciones
        if (Directory.Exists(migrationsPath))
        {
            var migrationFiles = Directory.GetFiles(migrationsPath, "*.cs");
            if (migrationFiles.Length > 0)
            {
                result.AddLog("    ✓ Migraciones existentes encontradas");
                _logService.WriteLog($"Migraciones existentes encontradas: {migrationFiles.Length} archivos");

                result.Success = true;
                result.Data = true; // Success implies "Verified or Created"
                result.Message = "Migraciones existentes.";
                return result;
            }
        }

        result.AddLog("    No se encontraron migraciones. Creando migración inicial...");
        _logService.WriteLog("No se encontraron migraciones. Creando migración inicial...");

        var projectPath = Path.Combine(infrastructurePath, "GesFer.Infrastructure.csproj");
        var startupProjectPath = Path.Combine(apiPath, "GesFer.Api.csproj");
        var command = $"ef migrations add InitialCreate --project \"{projectPath}\" --startup-project \"{startupProjectPath}\"";

        _logService.WriteLog($"Comando: dotnet {command}");
        _logService.WriteLog($"Directorio de trabajo: {apiPath}");

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = command,
                WorkingDirectory = apiPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                var errorMsg = "No se pudo iniciar dotnet ef";
                result.AddLog($"    ERROR: {errorMsg}");
                result.Errors.Add(errorMsg);
                _logService.WriteError(errorMsg);

                result.Success = false;
                result.Message = errorMsg;
                return result;
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
                result.AddLog("    ✓ Migración inicial creada");
                _logService.WriteLog("Migración inicial creada correctamente");

                result.Success = true;
                result.Data = true;
                result.Message = "Migración inicial creada.";
                return result;
            }
            else
            {
                result.AddLog("    ERROR: No se pudieron crear las migraciones");
                _logService.WriteError("No se pudieron crear las migraciones");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    result.AddLog("    Revisa el archivo de log para más detalles");
                }

                result.Success = false;
                result.Message = "Fallo al crear migración.";
                return result;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al crear migración: {ex.Message}";
            result.AddLog($"    ERROR: {ex.Message}");
            result.Errors.Add(errorMsg);
            _logService.WriteError(errorMsg, ex);

            result.Success = false;
            result.Message = errorMsg;
            return result;
        }
    }
}
