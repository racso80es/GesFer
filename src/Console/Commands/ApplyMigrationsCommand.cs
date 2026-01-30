using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Commands;

public class ApplyMigrationsCommand : ICommandHandler<ApplyMigrationsInput, bool>
{
    private readonly LogService _logService;

    public ApplyMigrationsCommand(LogService logService)
    {
        _logService = logService;
    }

    public async Task<CommandResult<bool>> HandleAsync(ApplyMigrationsInput input)
    {
        var result = new CommandResult<bool>();
        result.Data = false;

        result.AddLog("    Aplicando migraciones a la base de datos...");
        _logService.WriteLog("Aplicando migraciones a la base de datos...");

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var apiPath = Path.GetFullPath(Path.Combine(rootPath, "src", "Product", "Back", "src", "Api"));
        var infrastructurePath = Path.GetFullPath(Path.Combine(rootPath, "src", "Product", "Back", "src", "Infrastructure"));

        var projectPath = Path.Combine(infrastructurePath, "GesFer.Infrastructure.csproj");
        var startupProjectPath = Path.Combine(apiPath, "GesFer.Api.csproj");
        var command = $"ef database update --project \"{projectPath}\" --startup-project \"{startupProjectPath}\"";

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
            _logService.WriteProcessOutput("dotnet ef database update", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput("dotnet ef database update", error, true);
            }

            _logService.WriteLog($"Código de salida: {process.ExitCode}");

            if (process.ExitCode == 0)
            {
                result.AddLog("    ✓ Migraciones aplicadas correctamente");
                _logService.WriteLog("Migraciones aplicadas correctamente");

                result.Success = true;
                result.Data = true;
                result.Message = "Migraciones aplicadas.";
                return result;
            }
            else
            {
                result.AddLog("    ERROR: No se pudieron aplicar las migraciones");
                _logService.WriteError("No se pudieron aplicar las migraciones");

                // Si el build falló, intentar obtener más información ejecutando dotnet build
                if (output.Contains("Build failed") || output.Contains("Build started"))
                {
                    result.AddLog("    El build del proyecto falló. Ejecutando dotnet build para ver los errores...");
                    _logService.WriteLog("El build falló, ejecutando dotnet build para obtener errores detallados...");

                    var buildProcessInfo = new ProcessStartInfo
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
                        using var buildProcess = Process.Start(buildProcessInfo);
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

                            // Mostrar errores de compilación
                            if (!string.IsNullOrWhiteSpace(buildError))
                            {
                                result.AddLog("");
                                result.AddLog("    Errores de compilación:");
                                var errorLines = buildError.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(l => l.Contains("error", StringComparison.OrdinalIgnoreCase))
                                    .Take(15)
                                    .ToList();

                                foreach (var line in errorLines)
                                {
                                    result.AddLog($"    {line}");
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
                                        result.AddLog($"    {line}");
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

                result.AddLog("");
                result.AddLog("    Revisa el archivo de log para más detalles:");
                result.AddLog($"    {_logService.GetLogFilePath()}");

                if (!string.IsNullOrWhiteSpace(error))
                {
                    result.AddLog("");
                    result.AddLog("    Detalles del error:");
                    var errorLines = error.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in errorLines.Take(10))
                    {
                        result.AddLog($"    {line}");
                    }
                    if (errorLines.Length > 10)
                    {
                        result.AddLog($"    ... y {errorLines.Length - 10} líneas más (ver log completo)");
                    }
                }

                result.Success = false;
                result.Message = "Fallo al aplicar migraciones.";
                return result;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al aplicar migraciones: {ex.Message}";
            result.AddLog($"    ERROR: {ex.Message}");
            result.Errors.Add(errorMsg);
            _logService.WriteError(errorMsg, ex);

            result.Success = false;
            result.Message = errorMsg;
            return result;
        }
    }
}
