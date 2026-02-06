using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;

namespace GesFer.ConsoleApp.Commands;

public class RunUnitTestsCommand : ICommandHandler<RunUnitTestsInput, bool>
{
    private readonly LogService _logService;
    private readonly string _rootPath;

    public RunUnitTestsCommand(LogService logService)
    {
        _logService = logService;
        _rootPath = _logService.GetRootPath();
    }

    public async Task<CommandResult<bool>> HandleAsync(RunUnitTestsInput input)
    {
        var result = new CommandResult<bool>();
        result.Success = true;
        result.Data = true;

        Console.WriteLine("Iniciando Tests Unitarios...");
        _logService.WriteLog("Iniciando Tests Unitarios...");

        // 1. Backend Unit Tests
        var backendProjects = new[]
        {
            "src/Shared/Back/tests/GesFer.Shared.Back.UnitTests/GesFer.Shared.Back.UnitTests.csproj",
            "src/Admin/Back/tests/GesFer.Admin.UnitTests/GesFer.Admin.UnitTests.csproj",
            "src/Product/Back/tests/GesFer.Product.UnitTests/GesFer.Product.UnitTests.csproj"
        };

        foreach (var project in backendProjects)
        {
            var projectPath = Path.Combine(_rootPath, project);
            if (!File.Exists(projectPath))
            {
                _logService.WriteError($"Proyecto no encontrado: {projectPath}");
                Console.WriteLine($"⚠ Proyecto no encontrado: {Path.GetFileName(project)}");
                continue;
            }

            var success = await ExecuteProcessAsync("dotnet", $"test \"{projectPath}\" --nologo", $"Backend: {Path.GetFileNameWithoutExtension(project)}");
            if (!success) result.Success = false;
        }

        // 2. Frontend Unit Tests
        var frontendProjects = new[]
        {
            ("src/Product/Front", "Product Front"),
            ("src/Admin/Front", "Admin Front")
        };

        foreach (var (folder, name) in frontendProjects)
        {
            var workingDir = Path.Combine(_rootPath, folder);
            if (!Directory.Exists(workingDir))
            {
                _logService.WriteError($"Directorio no encontrado: {workingDir}");
                Console.WriteLine($"⚠ Directorio no encontrado: {folder}");
                continue;
            }

            // Ejecutar npm run test
            var npmCmd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "npm.cmd" : "npm";
            var success = await ExecuteProcessAsync(npmCmd, "run test", $"Frontend: {name}", workingDir);
            if (!success) result.Success = false;
        }

        result.Data = result.Success;
        result.Message = result.Success ? "Todos los tests unitarios pasaron." : "Algunos tests unitarios fallaron.";
        return result;
    }

    private async Task<bool> ExecuteProcessAsync(string fileName, string arguments, string label, string? workingDir = null)
    {
        Console.Write($"  Running {label} ... ");
        _logService.WriteLog($"Ejecutando: {fileName} {arguments} en {workingDir ?? "Root"}");

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDir ?? _rootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("ERROR (No se pudo iniciar)");
                _logService.WriteError($"No se pudo iniciar el proceso: {fileName} {arguments}");
                return false;
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            _logService.WriteProcessOutput($"{fileName} {arguments}", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput($"{fileName} {arguments}", error, true);
            }

            if (process.ExitCode == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ PASS");
                Console.ResetColor();
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ FAIL");
                Console.ResetColor();
                // Opcional: Mostrar error en consola si falla
                // Console.WriteLine(error);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ ERROR DE EJECUCIÓN");
            Console.ResetColor();
            _logService.WriteError($"Excepción al ejecutar {label}", ex);
            return false;
        }
    }
}

public class RunIntegrationTestsCommand : ICommandHandler<RunIntegrationTestsInput, bool>
{
    private readonly LogService _logService;
    private readonly string _rootPath;

    public RunIntegrationTestsCommand(LogService logService)
    {
        _logService = logService;
        _rootPath = _logService.GetRootPath();
    }

    public async Task<CommandResult<bool>> HandleAsync(RunIntegrationTestsInput input)
    {
        var result = new CommandResult<bool>();
        result.Success = true;
        result.Data = true;

        Console.WriteLine("Iniciando Tests de Integración (Docker)...");
        _logService.WriteLog("Iniciando Tests de Integración...");

        // 1. Backend Integration (backend-test service)
        var backendSuccess = await ExecuteDockerComposeAsync("run --rm backend-test", "Backend Integration");
        if (!backendSuccess) result.Success = false;

        // 2. Frontend Integrity (frontend-test service with specific command)
        // package.json script: "test:integrity": "jest __tests__/integration --verbose"
        // docker-compose.test.yml defines frontend-test service. We override command.
        var frontendSuccess = await ExecuteDockerComposeAsync("run --rm frontend-test npm run test:integrity", "Frontend Integration");
        if (!frontendSuccess) result.Success = false;

        result.Data = result.Success;
        result.Message = result.Success ? "Todos los tests de integración pasaron." : "Algunos tests de integración fallaron.";
        return result;
    }

    private async Task<bool> ExecuteDockerComposeAsync(string args, string label)
    {
        Console.Write($"  Running {label} ... ");
        _logService.WriteLog($"Ejecutando Docker Compose: {args}");

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = $"-f docker-compose.test.yml {args}",
                WorkingDirectory = _rootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("ERROR (No se pudo iniciar)");
                return false;
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            _logService.WriteProcessOutput($"docker-compose {args}", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput($"docker-compose {args}", error, true);
            }

            if (process.ExitCode == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ PASS");
                Console.ResetColor();
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ FAIL");
                Console.ResetColor();
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ ERROR");
            Console.ResetColor();
            _logService.WriteError($"Excepción en {label}", ex);
            return false;
        }
    }
}

public class RunE2ETestsCommand : ICommandHandler<RunE2ETestsInput, bool>
{
    private readonly LogService _logService;
    private readonly string _rootPath;

    public RunE2ETestsCommand(LogService logService)
    {
        _logService = logService;
        _rootPath = _logService.GetRootPath();
    }

    public async Task<CommandResult<bool>> HandleAsync(RunE2ETestsInput input)
    {
        var result = new CommandResult<bool>();
        result.Success = true;
        result.Data = true;

        Console.WriteLine("Iniciando Tests E2E (Playwright en Docker)...");
        _logService.WriteLog("Iniciando Tests E2E...");

        // Verificar si la app está corriendo (opcional, pero recomendado)
        // Por simplicidad, asumimos que el usuario es responsable o que el test fallará rápido.
        // Podríamos usar CheckDockerCommand o similar, pero aquí nos enfocamos en lanzar el test.

        var success = await ExecuteDockerComposeAsync("run --rm playwright-test", "Playwright E2E");
        if (!success) result.Success = false;

        result.Data = result.Success;
        result.Message = result.Success ? "Tests E2E pasaron." : "Tests E2E fallaron.";
        return result;
    }

    private async Task<bool> ExecuteDockerComposeAsync(string args, string label)
    {
        Console.Write($"  Running {label} ... ");
        _logService.WriteLog($"Ejecutando Docker Compose: {args}");

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = $"-f docker-compose.test.yml {args}",
                WorkingDirectory = _rootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("ERROR (No se pudo iniciar)");
                return false;
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            _logService.WriteProcessOutput($"docker-compose {args}", output, false);
            if (!string.IsNullOrWhiteSpace(error))
            {
                _logService.WriteProcessOutput($"docker-compose {args}", error, true);
            }

            if (process.ExitCode == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ PASS");
                Console.ResetColor();
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ FAIL");
                Console.ResetColor();
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ ERROR");
            Console.ResetColor();
            _logService.WriteError($"Excepción en {label}", ex);
            return false;
        }
    }
}
