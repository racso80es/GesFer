using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Commands;

public class EnsureEfToolCommand : ICommandHandler<EnsureEfToolInput, bool>
{
    private readonly LogService _logService;

    public EnsureEfToolCommand(LogService logService)
    {
        _logService = logService;
    }

    public async Task<CommandResult<bool>> HandleAsync(EnsureEfToolInput input)
    {
        var result = new CommandResult<bool>();
        result.Data = false;

        result.AddLog("Verificando herramienta dotnet-ef...");
        if (await IsEfToolInstalledAsync())
        {
            result.AddLog("    ✓ Herramienta dotnet-ef encontrada");
            result.Success = true;
            result.Data = true;
            result.Message = "Herramienta dotnet-ef encontrada.";
            return result;
        }

        result.AddLog("Instalando herramienta dotnet-ef...");
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

                    result.Success = true;
                    result.Data = true;
                    result.Message = "Herramienta dotnet-ef instalada.";
                    return result;
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

                    result.Success = true;
                    result.Data = true;
                    result.Message = "Herramienta dotnet-ef instalada.";
                    return result;
                }
            }

            var errorMsg = "No se pudo instalar la herramienta dotnet-ef";
            result.AddLog($"    ERROR: {errorMsg}");
            result.Errors.Add(errorMsg);
            _logService.WriteError(errorMsg);

            result.Success = false;
            result.Message = errorMsg;
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Excepción al instalar dotnet-ef: {ex.Message}";
            result.AddLog($"    ERROR: {ex.Message}");
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
}
