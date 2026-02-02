using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Commands;

public class CheckDockerCommand : ICommandHandler<CheckDockerInput, bool>
{
    private readonly LogService _logService;

    public CheckDockerCommand(LogService logService)
    {
        _logService = logService;
    }

    public async Task<CommandResult<bool>> HandleAsync(CheckDockerInput input)
    {
        var result = new CommandResult<bool>();
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
                result.Success = false;
                result.Data = false;
                result.Message = "Could not start docker process";
                return result;
            }

            await process.WaitForExitAsync();

            result.Success = process.ExitCode == 0;
            result.Data = result.Success;
            result.Message = result.Success ? "Docker is running" : "Docker is not running";
            return result;
        }
        catch
        {
            result.Success = false;
            result.Data = false;
            result.Message = "Exception checking docker";
            return result;
        }
    }
}

public class RemoveContainersCommand : ICommandHandler<RemoveContainersInput, bool>
{
    private readonly LogService _logService;
    private readonly string _apiPath;

    public RemoveContainersCommand(LogService logService)
    {
        _logService = logService;
        _apiPath = _logService.GetRootPath();
    }

    public async Task<CommandResult<bool>> HandleAsync(RemoveContainersInput input)
    {
        var result = new CommandResult<bool>();
        result.Data = false;

        result.AddLog("Limpiando contenedores existentes...");
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
                result.AddLog($"    ERROR: {errorMsg}");
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

            _logService.WriteProcessOutput("docker-compose down", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput("docker-compose down", error, true);
            }
            _logService.WriteLog($"Código de salida: {process.ExitCode}");

            if (process.ExitCode == 0)
            {
                result.AddLog("    ✓ Contenedores eliminados");
                _logService.WriteLog("Contenedores eliminados correctamente");
                result.Success = true;
                result.Data = true;
                result.Message = "Contenedores eliminados.";
            }
            else
            {
                result.AddLog("    ⚠ No se pudieron detener contenedores (puede que no existan)");
                _logService.WriteLog("No se pudieron detener contenedores (puede que no existan)");
                // Not critical error
                result.Success = true;
                result.Data = true;
                result.Message = "Contenedores no encontrados o ya detenidos.";
            }
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al eliminar contenedores: {ex.Message}";
            result.AddLog($"    ERROR: {ex.Message}");
            result.Errors.Add(errorMsg);
            _logService.WriteError(errorMsg, ex);

            result.Success = false;
            result.Message = errorMsg;
            return result;
        }
    }
}

public class CreateContainersCommand : ICommandHandler<CreateContainersInput, bool>
{
    private readonly LogService _logService;
    private readonly string _apiPath;

    public CreateContainersCommand(LogService logService)
    {
        _logService = logService;
        _apiPath = _logService.GetRootPath();
    }

    public async Task<CommandResult<bool>> HandleAsync(CreateContainersInput input)
    {
        var result = new CommandResult<bool>();
        result.Data = false;

        result.AddLog("Creando contenedores Docker...");
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
                result.AddLog($"    ERROR: {errorMsg}");
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

            _logService.WriteProcessOutput("docker-compose up", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput("docker-compose up", error, true);
            }
            _logService.WriteLog($"Código de salida: {process.ExitCode}");

            if (process.ExitCode == 0)
            {
                result.AddLog("    ✓ Contenedores creados");
                _logService.WriteLog("Contenedores creados correctamente");
                result.Success = true;
                result.Data = true;
                result.Message = "Contenedores creados.";
            }
            else
            {
                result.AddLog("    ERROR: No se pudieron crear los contenedores");
                _logService.WriteError("No se pudieron crear los contenedores");
                result.Success = false;
                result.Message = "Fallo al crear contenedores.";
            }
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al crear contenedores: {ex.Message}";
            result.AddLog($"    ERROR: {ex.Message}");
            result.Errors.Add(errorMsg);
            _logService.WriteError(errorMsg, ex);

            result.Success = false;
            result.Message = errorMsg;
            return result;
        }
    }
}

public class WaitMySqlReadyCommand : ICommandHandler<WaitMySqlInput, bool>
{
    private readonly LogService _logService;

    public WaitMySqlReadyCommand(LogService logService)
    {
        _logService = logService;
    }

    public async Task<CommandResult<bool>> HandleAsync(WaitMySqlInput input)
    {
        var result = new CommandResult<bool>();
        result.Data = false;

        int maxAttempts = 30;
        int delaySeconds = 2;

        result.AddLog("Esperando a que MySQL esté listo...");
        _logService.WriteLog($"Esperando a que MySQL esté listo (máximo {maxAttempts} intentos)...");

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "exec gesfer_db mysqladmin ping -h localhost -u root -prootpassword",
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
                        result.AddLog("    ✓ MySQL está listo");
                        _logService.WriteLog($"MySQL está listo después de {attempt} intentos");
                        // Esperar un poco más para asegurar que MySQL esté completamente listo
                        await Task.Delay(TimeSpan.FromSeconds(5));

                        result.Success = true;
                        result.Data = true;
                        result.Message = "MySQL ready.";
                        return result;
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
            }

            if (attempt < maxAttempts)
            {
                result.AddLog($"    Intento {attempt}/{maxAttempts}...");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

        var errorMsg = $"MySQL no está listo después de {maxAttempts} intentos";
        result.AddLog($"    ERROR: {errorMsg}");
        result.Errors.Add(errorMsg);
        _logService.WriteError(errorMsg);

        result.Success = false;
        result.Message = errorMsg;
        return result;
    }
}
