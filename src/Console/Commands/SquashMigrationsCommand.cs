using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Commands;

public class SquashMigrationsCommand : ICommandHandler<SquashMigrationsInput, MigrationSquashResultData>
{
    private readonly LogService _logService;

    public SquashMigrationsCommand(LogService logService)
    {
        _logService = logService;
    }

    public async Task<CommandResult<MigrationSquashResultData>> HandleAsync(SquashMigrationsInput input)
    {
        var result = new CommandResult<MigrationSquashResultData>();
        result.Data = new MigrationSquashResultData();

        result.AddLog("Iniciando proceso de squash de migraciones...");
        _logService.WriteLog("========================================");
        _logService.WriteLog("Inicio de squash de migraciones");
        _logService.WriteLog("========================================");

        try
        {
            // Obtener rutas
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            var apiPath = Path.GetFullPath(Path.Combine(rootPath, "src", "Product", "Back", "src", "Api"));
            var infrastructurePath = Path.GetFullPath(Path.Combine(rootPath, "src", "Product", "Back", "src", "Infrastructure"));
            var migrationsPath = Path.Combine(infrastructurePath, "Migrations");

            result.AddLog($"Ruta de migraciones: {migrationsPath}");

            // Paso 1: Verificar que dotnet-ef esté instalado
            result.AddLog("Verificando herramienta dotnet-ef...");
            if (!await IsEfToolInstalledAsync())
            {
                result.AddLog("Instalando herramienta dotnet-ef...");
                if (!await InstallEfToolAsync(result))
                {
                    var errorMsg = "No se pudo instalar la herramienta dotnet-ef";
                    result.AddLog($"ERROR: {errorMsg}");
                    result.Errors.Add(errorMsg);
                    _logService.WriteError(errorMsg);

                    result.Success = false;
                    result.Message = errorMsg;
                    return result;
                }
            }
            result.AddLog("✓ Herramienta dotnet-ef disponible");

            // Paso 2: Eliminar carpeta Migrations completa
            result.AddLog("Eliminando carpeta de migraciones existentes...");
            if (Directory.Exists(migrationsPath))
            {
                try
                {
                    var filesBefore = Directory.GetFiles(migrationsPath, "*.*", SearchOption.AllDirectories);
                    result.AddLog($"Archivos encontrados en Migrations: {filesBefore.Length}");
                    result.Data.DeletedFilesCount = filesBefore.Length;

                    Directory.Delete(migrationsPath, recursive: true);
                    result.AddLog("✓ Carpeta Migrations eliminada correctamente");
                    _logService.WriteLog($"Carpeta Migrations eliminada: {filesBefore.Length} archivos");
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Error al eliminar carpeta Migrations: {ex.Message}";
                    result.AddLog($"ERROR: {errorMsg}");
                    result.Errors.Add(errorMsg);
                    _logService.WriteError(errorMsg, ex);

                    result.Success = false;
                    result.Message = errorMsg;
                    return result;
                }
            }
            else
            {
                result.AddLog("⚠ No se encontró carpeta Migrations (puede que ya esté eliminada)");
                result.Data.DeletedFilesCount = 0;
            }

            // Paso 3: Generar nueva migración inicial
            result.AddLog("Generando nueva migración inicial única...");
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
                    result.AddLog($"ERROR: {errorMsg}");
                    result.Errors.Add(errorMsg);
                    _logService.WriteError(errorMsg);

                    result.Success = false;
                    result.Message = errorMsg;
                    return result;
                }

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                var output = await outputTask;
                var error = await errorTask;

                _logService.WriteProcessOutput("dotnet ef migrations add", output, false);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logService.WriteProcessOutput("dotnet ef migrations add", error, true);
                }

                _logService.WriteLog($"Código de salida: {process.ExitCode}");

                if (process.ExitCode != 0)
                {
                    var errorMsg = "Error al generar la migración inicial";
                    result.AddLog($"ERROR: {errorMsg}");
                    result.AddLog($"Salida: {output}");
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        result.AddLog($"Errores: {error}");
                    }
                    result.Errors.Add(errorMsg);
                    _logService.WriteError(errorMsg);

                    result.Success = false;
                    result.Message = errorMsg;
                    return result;
                }

                result.AddLog("✓ Migración inicial generada correctamente");
                _logService.WriteLog("Migración inicial generada correctamente");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Excepción al generar migración: {ex.Message}";
                result.AddLog($"ERROR: {errorMsg}");
                result.Errors.Add(errorMsg);
                _logService.WriteError(errorMsg, ex);

                result.Success = false;
                result.Message = errorMsg;
                return result;
            }

            // Paso 4: Verificar que la migración se creó correctamente
            result.AddLog("Verificando migración generada...");
            if (Directory.Exists(migrationsPath))
            {
                var migrationFiles = Directory.GetFiles(migrationsPath, "*.cs");
                result.Data.CreatedFilesCount = migrationFiles.Length;
                result.AddLog($"Archivos de migración creados: {migrationFiles.Length}");

                foreach (var file in migrationFiles)
                {
                    var fileName = Path.GetFileName(file);
                    result.AddLog($"  - {fileName}");
                    result.Data.CreatedFiles.Add(fileName);
                }

                // Verificar que la migración incluya las tablas críticas (Logs y AdminUsers)
                var migrationFile = migrationFiles.FirstOrDefault(f => f.Contains("InitialCreate") && !f.Contains("Designer") && !f.Contains("Snapshot"));
                if (migrationFile != null && File.Exists(migrationFile))
                {
                    var migrationContent = await File.ReadAllTextAsync(migrationFile);
                    var hasLogsTable = migrationContent.Contains("CreateTable", StringComparison.OrdinalIgnoreCase) &&
                                       migrationContent.Contains("Logs", StringComparison.OrdinalIgnoreCase);
                    var hasAdminUsersTable = migrationContent.Contains("CreateTable", StringComparison.OrdinalIgnoreCase) &&
                                            migrationContent.Contains("AdminUsers", StringComparison.OrdinalIgnoreCase);

                    if (hasLogsTable)
                    {
                        result.AddLog("✓ Tabla 'Logs' encontrada en la migración");
                        result.Data.TablesFound.Add("Logs");
                    }
                    else
                    {
                        result.AddLog("⚠ Tabla 'Logs' no encontrada en la migración");
                    }

                    if (hasAdminUsersTable)
                    {
                        result.AddLog("✓ Tabla 'AdminUsers' encontrada en la migración");
                        result.Data.TablesFound.Add("AdminUsers");
                    }
                    else
                        result.AddLog("⚠ Tabla 'AdminUsers' no encontrada en la migración");

                    // Contar todas las tablas creadas
                    var createTableMatches = Regex.Matches(
                        migrationContent,
                        @"CreateTable\s*\(\s*name:\s*""([^""]+)""",
                        RegexOptions.IgnoreCase
                    );

                    result.Data.TotalTablesInMigration = createTableMatches.Count;
                    result.AddLog($"Total de tablas en la migración: {result.Data.TotalTablesInMigration}");

                    foreach (System.Text.RegularExpressions.Match match in createTableMatches)
                    {
                        if (match.Groups.Count > 1)
                        {
                            var tableName = match.Groups[1].Value;
                            if (!result.Data.TablesFound.Contains(tableName))
                            {
                                result.Data.TablesFound.Add(tableName);
                            }
                        }
                    }
                }
            }
            else
            {
                var errorMsg = "La carpeta Migrations no se creó después de generar la migración";
                result.AddLog($"ERROR: {errorMsg}");
                result.Errors.Add(errorMsg);
                _logService.WriteError(errorMsg);

                result.Success = false;
                result.Message = errorMsg;
                return result;
            }

            result.Success = true;
            result.Message = "Squash de migraciones completado exitosamente";
            result.AddLog("========================================");
            result.AddLog("Squash de migraciones completado exitosamente");
            result.AddLog("========================================");
            _logService.WriteLog("Squash de migraciones completado exitosamente");

            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción durante el squash de migraciones: {ex.Message}";
            result.AddLog($"ERROR: {errorMsg}");
            result.Errors.Add(errorMsg);
            _logService.WriteError(errorMsg, ex);

            result.Success = false;
            result.Message = errorMsg;
            return result;
        }
    }

    private async Task<bool> IsEfToolInstalledAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "ef --version",
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

    private async Task<bool> InstallEfToolAsync(CommandResult<MigrationSquashResultData> result)
    {
        result.AddLog("    Instalando herramienta dotnet-ef...");
        _logService.WriteLog("Instalando herramienta dotnet-ef...");

        try
        {
            // Intentar instalar con versión específica
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "tool install --global dotnet-ef --version 8.0.0",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logService.WriteLog($"Comando: dotnet {processInfo.Arguments}");

            using var process = Process.Start(processInfo);
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
                    result.AddLog("    ✓ Herramienta dotnet-ef instalada correctamente");
                    _logService.WriteLog("Herramienta dotnet-ef instalada correctamente");
                    return true;
                }
            }

            // Si falla, intentar sin versión específica
            result.AddLog("    ⚠ Falló instalación con versión específica. Intentando sin versión...");
            _logService.WriteLog("Falló instalación con versión específica. Intentando sin versión...");
            processInfo.Arguments = "tool install --global dotnet-ef";
            _logService.WriteLog($"Comando: dotnet {processInfo.Arguments}");

            using var process2 = Process.Start(processInfo);
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
                    result.AddLog("    ✓ Herramienta dotnet-ef instalada correctamente");
                    _logService.WriteLog("Herramienta dotnet-ef instalada correctamente");
                    return true;
                }
            }

            result.AddLog("    ERROR: No se pudo instalar la herramienta dotnet-ef");
            _logService.WriteError("No se pudo instalar la herramienta dotnet-ef");
            return false;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al instalar dotnet-ef: {ex.Message}";
            result.AddLog($"    ERROR: {ex.Message}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }
}
