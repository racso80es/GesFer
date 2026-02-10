using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para mostrar y gestionar el menú interactivo
/// </summary>
public class MenuService
{
    private readonly CheckDockerCommand _checkDockerCommand;
    private readonly CheckDockerComposeCommand _checkDockerComposeCommand;
    private readonly RemoveContainersCommand _removeContainersCommand;
    private readonly CreateContainersCommand _createContainersCommand;
    private readonly WaitMySqlReadyCommand _waitMySqlReadyCommand;
    private readonly ApplyMigrationsCommand _applyMigrationsCommand;
    private readonly CreateInitialMigrationCommand _createInitialMigrationCommand;
    private readonly SquashMigrationsCommand _squashMigrationsCommand;
    private readonly EnsureEfToolCommand _ensureEfToolCommand;
    private readonly SeedCommand _seedCommand;
    private readonly InitializeDatabaseCommand _initializeDatabaseCommand;
    private readonly StartLocalEnvironmentCommand _startLocalEnvironmentCommand;
    private readonly RunUnitTestsCommand _runUnitTestsCommand;
    private readonly RunIntegrationTestsCommand _runIntegrationTestsCommand;
    private readonly RunE2ETestsCommand _runE2ETestsCommand;
    private readonly IntegrityValidationService _integrityValidationService;
    private readonly GoldenRulesComplianceService _goldenRulesService;
    private readonly LogService _logService;

    public MenuService(
        CheckDockerCommand checkDockerCommand,
        CheckDockerComposeCommand checkDockerComposeCommand,
        RemoveContainersCommand removeContainersCommand,
        CreateContainersCommand createContainersCommand,
        WaitMySqlReadyCommand waitMySqlReadyCommand,
        ApplyMigrationsCommand applyMigrationsCommand,
        CreateInitialMigrationCommand createInitialMigrationCommand,
        SquashMigrationsCommand squashMigrationsCommand,
        EnsureEfToolCommand ensureEfToolCommand,
        SeedCommand seedCommand,
        InitializeDatabaseCommand initializeDatabaseCommand,
        StartLocalEnvironmentCommand startLocalEnvironmentCommand,
        RunUnitTestsCommand runUnitTestsCommand,
        RunIntegrationTestsCommand runIntegrationTestsCommand,
        RunE2ETestsCommand runE2ETestsCommand,
        IntegrityValidationService integrityValidationService,
        GoldenRulesComplianceService goldenRulesService,
        LogService logService)
    {
        _checkDockerCommand = checkDockerCommand;
        _checkDockerComposeCommand = checkDockerComposeCommand;
        _removeContainersCommand = removeContainersCommand;
        _createContainersCommand = createContainersCommand;
        _waitMySqlReadyCommand = waitMySqlReadyCommand;
        _applyMigrationsCommand = applyMigrationsCommand;
        _createInitialMigrationCommand = createInitialMigrationCommand;
        _squashMigrationsCommand = squashMigrationsCommand;
        _ensureEfToolCommand = ensureEfToolCommand;
        _seedCommand = seedCommand;
        _initializeDatabaseCommand = initializeDatabaseCommand;
        _startLocalEnvironmentCommand = startLocalEnvironmentCommand;
        _runUnitTestsCommand = runUnitTestsCommand;
        _runIntegrationTestsCommand = runIntegrationTestsCommand;
        _runE2ETestsCommand = runE2ETestsCommand;
        _integrityValidationService = integrityValidationService;
        _goldenRulesService = goldenRulesService;
        _logService = logService;
    }

    /// <summary>
    /// Helper para leer tecla de forma segura en entornos no interactivos
    /// </summary>
    private void SafeReadKey()
    {
        if (!Console.IsInputRedirected)
        {
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Devuelve el texto del menú principal (para tests que verifican contenido sin usar consola).
    /// </summary>
    public static string GetMainMenuTextForTesting()
    {
        return string.Join(Environment.NewLine,
            "========================================",
            "        GesFer - Consola de Gestión",
            "========================================",
            "",
            "Seleccione una opción:",
            "",
            "  1. Inicialización completa",
            "  2. Levantar entorno local (Back/Front) [Shortcut]",
            "  3. Acciones Atómicas (Docker, Seeds, Servicios)",
            "  4. Validación de integridad completa",
            "  5. Cumplimiento de Reglas de Oro",
            "  6. Gestionar contenedores Docker",
            "  7. Aplicar migraciones de BD",
            "  9. Squash de migraciones",
            "  10. Salir",
            "  11. Ejecutar tests",
            "");
    }

    /// <summary>
    /// Muestra el menú principal
    /// </summary>
    public void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine(GetMainMenuTextForTesting());
        Console.Write("Opción: ");
    }

    /// <summary>
    /// Ejecuta la opción seleccionada
    /// </summary>
    public async Task<bool> ExecuteOptionAsync(int option, bool waitForInput = true)
    {
        try
        {
            switch (option)
            {
                case 1:
                    return await ExecuteFullInitializationAsync(waitForInput);
                case 2:
                    // Shortcut: Levantar todo
                    var result = await _startLocalEnvironmentCommand.HandleAsync(new StartLocalEnvironmentInput());
                    return result.Success;
                case 3:
                    // Nueva Acción 3: Acciones Atómicas
                    return await ExecuteAtomicActionsMenuAsync();
                case 4:
                    return await ExecuteIntegrityValidationAsync();
                case 5:
                    return await ExecuteGoldenRulesComplianceAsync();
                case 6:
                    return await ExecuteDockerMenuAsync();
                case 7:
                    return await ExecuteMigrationsMenuAsync();
                // case 8: Eliminado
                case 9:
                    return await ExecuteMigrationSquashAsync();
                case 10:
                    return false;
                case 11:
                    return await ExecuteTestsMenuAsync();
                default:
                    Console.WriteLine("Opción no válida. Presione cualquier tecla para continuar...");
                    SafeReadKey();
                    return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }
    }

    /// <summary>
    /// Nuevo menú de Acciones Atómicas (Acción 3)
    /// </summary>
    private async Task<bool> ExecuteAtomicActionsMenuAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("   Acciones Atómicas");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("  1. Inicializar Docker (Recrear + Esperar MySQL)");
            Console.WriteLine("  2. Restaurar Datos Seed (Granular)");
            Console.WriteLine("  3. Levantar Servicios (Granular)");
            Console.WriteLine("  4. Inicialización Completa BD (Migraciones + Seeds)");
            Console.WriteLine("  5. Volver al menú principal");
            Console.WriteLine();
            Console.Write("Opción: ");

            if (!int.TryParse(Console.ReadLine(), out int option))
            {
                Console.WriteLine("Opción no válida. Presione cualquier tecla para continuar...");
                SafeReadKey();
                continue;
            }

            switch (option)
            {
                case 1:
                    await ExecuteDockerInitializationAsync();
                    break;
                case 2:
                    await ExecuteSeedsMenuAsync();
                    break;
                case 3:
                    await ExecuteStartServicesMenuAsync();
                    break;
                case 4:
                    await ExecuteDatabaseInitializationStep8Async(waitForInput: true);
                    break;
                case 5:
                    return true;
                default:
                    Console.WriteLine("Opción no válida.");
                    SafeReadKey();
                    break;
            }
        }
    }

    /// <summary>
    /// Secuencia compartida: Remove → Create → Wait MySQL. Usada por Acción 3.1 y por Inicialización Completa.
    /// </summary>
    private async Task<bool> RunDockerInitSequenceAsync(string msgRemove, string msgCreate, string msgWait)
    {
        Console.WriteLine();
        Console.WriteLine(msgRemove);
        var rmResult = await _removeContainersCommand.HandleAsync(new RemoveContainersInput());
        foreach (var l in rmResult?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);

        Console.WriteLine();
        Console.WriteLine(msgCreate);
        var createResult = await _createContainersCommand.HandleAsync(new CreateContainersInput());
        foreach (var l in createResult?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
        if (createResult == null || !createResult.Success)
        {
            Console.WriteLine("ERROR: No se pudieron crear los contenedores");
            return false;
        }

        Console.WriteLine();
        Console.WriteLine(msgWait);
        var waitResult = await _waitMySqlReadyCommand.HandleAsync(new WaitMySqlInput());
        foreach (var l in waitResult?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
        if (waitResult == null || !waitResult.Success)
        {
            Console.WriteLine("ERROR: MySQL no está listo");
            return false;
        }
        Console.WriteLine();
        return true;
    }

    /// <summary>
    /// Acción 3.1: Inicializar Docker (reutiliza RunDockerInitSequenceAsync)
    /// </summary>
    private async Task ExecuteDockerInitializationAsync()
    {
        var ok = await RunDockerInitSequenceAsync(
            "[Docker] Limpiando contenedores existentes...",
            "[Docker] Creando contenedores...",
            "[Docker] Esperando a que MySQL esté listo...");
        if (ok)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Docker inicializado correctamente.");
            Console.ResetColor();
        }
        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        SafeReadKey();
    }

    /// <summary>
    /// Acción 3.3: Menú para levantar servicios granularmente
    /// </summary>
    private async Task ExecuteStartServicesMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Levantar Servicios (Granular)");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  1. Iniciar TODO (Back + Front)");
        Console.WriteLine("  2. Solo Product API (Back)");
        Console.WriteLine("  3. Solo Admin API (Back)");
        Console.WriteLine("  4. Solo Product Front");
        Console.WriteLine("  5. Solo Admin Front");
        Console.WriteLine("  6. Volver");
        Console.WriteLine();
        Console.Write("Opción: ");

        if (!int.TryParse(Console.ReadLine(), out int option)) return;

        var input = new StartLocalEnvironmentInput();

        switch (option)
        {
            case 1:
                // IsStartAll = true por defecto si todo es false
                break;
            case 2:
                input.StartProductApi = true;
                break;
            case 3:
                input.StartAdminApi = true;
                break;
            case 4:
                input.StartProductFront = true;
                break;
            case 5:
                input.StartAdminFront = true;
                break;
            case 6:
                return;
            default:
                Console.WriteLine("Opción no válida.");
                SafeReadKey();
                return;
        }

        await _startLocalEnvironmentCommand.HandleAsync(input);
        // HandleAsync tiene su propio loop de espera 'q' para salir; al volver aquí los servicios ya se detuvieron.
        Console.WriteLine();
        Console.WriteLine("Servicios detenidos. Volviendo al menú de acciones atómicas.");
        Console.WriteLine("Presione cualquier tecla para continuar...");
        SafeReadKey();
    }

    /// <summary>
    /// Ejecuta la inicialización completa
    /// </summary>
    private async Task<bool> ExecuteFullInitializationAsync(bool waitForInput = true)
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Inicialización Completa GesFer");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine($"Log: {_logService.GetLogFilePath()}");
        Console.WriteLine();
        _logService.WriteLog("========================================");
        _logService.WriteLog("Inicio de inicialización completa");
        _logService.WriteLog("========================================");

        // 1. Verificar Docker
        Console.WriteLine("[1/12] Verificando Docker...");
        var dockerCheck = await _checkDockerCommand.HandleAsync(new CheckDockerInput());
        if (!dockerCheck.Success || !dockerCheck.Data)
        {
            Console.WriteLine("ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }
        Console.WriteLine("    ✓ Docker está corriendo");

        var dockerComposeCheck = await _checkDockerComposeCommand.HandleAsync(new CheckDockerComposeInput());
        if (!dockerComposeCheck.Success || !dockerComposeCheck.Data)
        {
            Console.WriteLine("ERROR: docker-compose no se encuentra instalado o no está en el PATH.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }
        Console.WriteLine("    ✓ docker-compose está disponible");
        Console.WriteLine();

        // 2. Verificar que la API Product compila
        Console.WriteLine("[2/12] Verificando compilación de la API Product...");
        // Ruta corregida: _logService.GetRootPath() es la raíz del repo, luego src/Product/Back/Api
        var apiProjectPath = Path.Combine(_logService.GetRootPath(), "src", "Product", "Back", "Api", "GesFer.Api.csproj");
        
        if (!await CheckDotNetProjectCompilationAsync(apiProjectPath, "API Product"))
        {
            return true;
        }

        // 3. Verificar que la API Admin compila
        Console.WriteLine("[3/12] Verificando compilación de la API Admin...");
        var adminApiProjectPath = Path.Combine(_logService.GetRootPath(), "src", "Admin", "Back", "Api", "GesFer.Admin.Api.csproj");

        if (!await CheckDotNetProjectCompilationAsync(adminApiProjectPath, "API Admin"))
        {
            return true;
        }

        // 4 y 5. Verificar compilación de ambos frontends en paralelo (evita duplicar tiempo)
        var productFrontPath = Path.Combine(_logService.GetRootPath(), "src", "Product", "Front");
        var adminFrontPath = Path.Combine(_logService.GetRootPath(), "src", "Admin", "Front");

        Console.WriteLine("[4/12] y [5/12] Verificando compilación de Frontend Product y Admin en paralelo...");
        Console.WriteLine("    (solo se ejecuta 'npm install' si falta node_modules; luego 'npm run build')");
        var productTask = RunNpmCompilationCheckAsync(productFrontPath, "Frontend Product");
        var adminTask = RunNpmCompilationCheckAsync(adminFrontPath, "Frontend Admin");
        await Task.WhenAll(productTask, adminTask);
        var (productOk, productError) = await productTask;
        var (adminOk, adminError) = await adminTask;

        // Resultados en orden para lectura clara
        Console.WriteLine("[4/12] Frontend Product: " + (productOk ? "✓ compila correctamente" : "❌ ERROR"));
        if (!productOk && !string.IsNullOrEmpty(productError))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var line in productError.Split('\n').TakeLast(15)) Console.WriteLine("    " + line);
            Console.ResetColor();
        }
        Console.WriteLine("[5/12] Frontend Admin: " + (adminOk ? "✓ compila correctamente" : "❌ ERROR"));
        if (!adminOk && !string.IsNullOrEmpty(adminError))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var line in adminError.Split('\n').TakeLast(15)) Console.WriteLine("    " + line);
            Console.ResetColor();
        }
        Console.WriteLine();

        if (!productOk || !adminOk)
        {
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }

        // 6–8. Secuencia Docker (compartida con Acción 3.1)
        var dockerOk = await RunDockerInitSequenceAsync(
            "[6/12] Limpiando contenedores existentes...",
            "[7/12] Creando contenedores Docker...",
            "[8/12] Esperando a que MySQL esté listo...");
        if (!dockerOk)
        {
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }

        // 9. Verificar/Instalar dotnet-ef
        Console.WriteLine("[9/12] Verificando herramienta dotnet-ef...");
        var efToolResult = await _ensureEfToolCommand.HandleAsync(new EnsureEfToolInput());
        foreach (var log in efToolResult?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(log);

        if (efToolResult == null || !efToolResult.Success || !efToolResult.Data)
        {
            Console.WriteLine("ERROR: No se pudo verificar/instalar dotnet-ef");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }
        Console.WriteLine();

        // 10. Crear migraciones si no existen
        Console.WriteLine("[10/12] Verificando migraciones...");
        var initMigResult = await _createInitialMigrationCommand.HandleAsync(new CreateInitialMigrationInput());
        foreach (var log in initMigResult?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(log);

        if (initMigResult == null || !initMigResult.Success)
        {
             // Logged in command, but we should probably stop if strict?
        }
        Console.WriteLine();

        // 11. Aplicar migraciones y ejecutar seeds usando DatabaseInitializationService (Command)
        Console.WriteLine("[11/12] Aplicando migraciones y cargando datos iniciales desde JSON...");
        var initCmdResult = await _initializeDatabaseCommand.HandleAsync(new InitializeDatabaseInput());

        if (initCmdResult == null || initCmdResult.Data == null)
        {
             Console.WriteLine("ERROR: Fallo crítico al inicializar base de datos (resultado nulo).");
             return true;
        }

        var step8Result = initCmdResult.Data;
        foreach(var l in initCmdResult.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);

        // Mostrar información del proceso de forma visual
        Console.WriteLine();
        if (step8Result.Information != null && step8Result.Information.Any())
        {
            foreach (var info in step8Result.Information)
            {
                if (info.Contains("✓"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  {info}");
                    Console.ResetColor();
                }
                else if (info.Contains("⚠"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  {info}");
                    Console.ResetColor();
                }
                else if (info.Contains("Paso") || info.Contains("Iniciando") || info.Contains("==="))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  {info}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {info}");
                }
            }
            Console.WriteLine();
        }
        
        if (step8Result.Status != "ok")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: No se pudo inicializar la base de datos");
            Console.ResetColor();
            Console.WriteLine();
            
            // Mostrar errores
            if (step8Result.Errors.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Errores encontrados:");
                foreach (var error in step8Result.Errors)
                {
                    Console.WriteLine($"  ❌ {error}");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
            
            // Mostrar mensaje resumen
            if (!string.IsNullOrEmpty(step8Result.Message))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Mensaje: {step8Result.Message}");
                Console.ResetColor();
                Console.WriteLine();
            }
            
            Console.WriteLine("Para más detalles, revisa el archivo de log:");
            Console.WriteLine($"  {_logService.GetLogFilePath()}");
            Console.WriteLine();
            if (waitForInput && !Console.IsInputRedirected)
            {
                Console.WriteLine("Presione cualquier tecla para continuar...");
                try
                {
                    Console.ReadKey();
                }
                catch (InvalidOperationException)
                {
                    // Si no hay consola interactiva, continuar sin esperar
                }
            }
            return true;
        }
        
        // Mostrar mensaje de éxito
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {step8Result.Message}");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("========================================");
        Console.WriteLine("   Inicialización completada");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Datos iniciales insertados desde JSON:");
        Console.WriteLine("  ✓ Datos maestros (idiomas, permisos, grupos)");
        Console.WriteLine("  ✓ Datos de muestra (empresa, usuarios, clientes, proveedores)");
        Console.WriteLine("  ✓ Usuario administrativo (admin/admin123)");
        Console.WriteLine();
        Console.WriteLine("Credenciales de acceso:");
        Console.WriteLine("  Empresa: Empresa Admin");
        Console.WriteLine("  Usuario: admin");
        Console.WriteLine("  Contraseña: admin123");
        Console.WriteLine();
        Console.WriteLine("Servicios disponibles:");
        Console.WriteLine("  - MySQL: localhost:3306");
        Console.WriteLine("  - Memcached: localhost:11211");
        Console.WriteLine("  - Adminer: http://localhost:8080");
        Console.WriteLine();
        if (waitForInput && !Console.IsInputRedirected)
        {
            Console.WriteLine("Presione cualquier tecla para continuar...");
            try
            {
                Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                // Si no hay consola interactiva, continuar sin esperar
            }
        }

        return true;
    }

    private async Task<bool> CheckDotNetProjectCompilationAsync(string projectPath, string projectName)
    {
        if (!File.Exists(projectPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    ⚠ Advertencia: No se encontró el proyecto {projectName} en:");
            Console.WriteLine($"      {projectPath}");
            Console.ResetColor();
            Console.WriteLine("    Continuando sin verificar compilación...");
            Console.WriteLine();
            return true;
        }

        try
        {
            var buildProcess = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\" --no-incremental",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var buildProcessInstance = Process.Start(buildProcess);
            if (buildProcessInstance == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("    ⚠ Advertencia: No se pudo iniciar el proceso de compilación");
                Console.ResetColor();
                Console.WriteLine("    Continuando sin verificar compilación...");
                Console.WriteLine();
                return true;
            }

            var output = await buildProcessInstance.StandardOutput.ReadToEndAsync();
            var error = await buildProcessInstance.StandardError.ReadToEndAsync();
            await buildProcessInstance.WaitForExitAsync();

            if (buildProcessInstance.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"    ❌ ERROR: {projectName} no compila. Abortando inicialización.");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Errores de compilación:");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine(error);
                }
                if (!string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine(output);
                }
                Console.WriteLine();
                Console.WriteLine("Por favor, corrige los errores de compilación antes de continuar.");
                Console.WriteLine($"Ruta del proyecto: {projectPath}");
                Console.WriteLine();
                Console.WriteLine("Presione cualquier tecla para continuar...");
                SafeReadKey();

                _logService.WriteError($"{projectName} no compila. ExitCode: {buildProcessInstance.ExitCode}");
                _logService.WriteLog($"Salida de compilación: {output}");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logService.WriteLog($"Errores de compilación: {error}");
                }

                return false;
            }
            else
            {
                Console.WriteLine($"    ✓ {projectName} compila correctamente");
                _logService.WriteLog($"{projectName} compilada correctamente");
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    ⚠ Advertencia: Error al verificar compilación de {projectName}: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("    Continuando sin verificar compilación...");
            _logService.WriteError($"Error al verificar compilación de {projectName}", ex);
        }
        Console.WriteLine();
        return true;
    }

    /// <summary>
    /// Ejecuta verificación de compilación npm (install solo si falta node_modules, luego build).
    /// Devuelve (éxito, mensajeDeError). No escribe en consola para permitir ejecución en paralelo.
    /// </summary>
    private async Task<(bool Success, string? ErrorDetail)> RunNpmCompilationCheckAsync(string projectPath, string projectName)
    {
        if (!Directory.Exists(projectPath))
        {
            _logService.WriteLog($"Omisión: no existe directorio de {projectName} en {projectPath}");
            return (true, null);
        }

        var nodeModulesPath = Path.Combine(projectPath, "node_modules");
        var needsInstall = !Directory.Exists(nodeModulesPath);
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        var fileName = isWindows ? "cmd" : "bash";
        var installAndBuild = needsInstall
            ? (isWindows ? $"npm install && npm run build" : "npm install && npm run build")
            : "npm run build";
        var arguments = isWindows
            ? $"/c cd \"{projectPath}\" && {installAndBuild}"
            : $"-c \"cd '{projectPath}' && {installAndBuild}\"";

        try
        {
            var buildProcess = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var buildProcessInstance = Process.Start(buildProcess);
            if (buildProcessInstance == null)
            {
                _logService.WriteError($"No se pudo iniciar proceso npm para {projectName}", null!);
                return (true, null);
            }

            var output = await buildProcessInstance.StandardOutput.ReadToEndAsync();
            var error = await buildProcessInstance.StandardError.ReadToEndAsync();
            await buildProcessInstance.WaitForExitAsync();

            if (buildProcessInstance.ExitCode != 0)
            {
                _logService.WriteError($"{projectName} no compila. ExitCode: {buildProcessInstance.ExitCode}");
                _logService.WriteLog($"Salida: {output}\nERRORES: {error}");
                var errorDetail = string.IsNullOrWhiteSpace(error) ? output : error;
                return (false, errorDetail);
            }
            _logService.WriteLog($"{projectName} compilada correctamente");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logService.WriteError($"Error al verificar compilación de {projectName}", ex);
            return (true, null);
        }
    }

    private async Task<bool> CheckNpmProjectCompilationAsync(string projectPath, string projectName)
    {
        if (!Directory.Exists(projectPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    ⚠ Advertencia: No se encontró el directorio de {projectName} en:");
            Console.WriteLine($"      {projectPath}");
            Console.ResetColor();
            Console.WriteLine("    Continuando sin verificar compilación...");
            Console.WriteLine();
            return true;
        }

        Console.WriteLine("    Iniciando instalación de dependencias y build (puede tardar unos minutos)...");

        var (success, errorDetail) = await RunNpmCompilationCheckAsync(projectPath, projectName);

        if (!success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    ❌ ERROR: {projectName} no compila. Abortando inicialización.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Errores de compilación:");
            foreach (var line in (errorDetail ?? "").Split('\n').TakeLast(20))
                Console.WriteLine(line);
            Console.WriteLine();
            Console.WriteLine("Por favor, corrige los errores de compilación antes de continuar.");
            Console.WriteLine($"Ruta del proyecto: {projectPath}");
            Console.WriteLine();
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return false;
        }

        Console.WriteLine($"    ✓ {projectName} compila correctamente");
        Console.WriteLine();
        return true;
    }

    /// <summary>
    /// Ejecuta la inicialización de base de datos (Punto 8) de forma aislada
    /// </summary>
    private async Task<bool> ExecuteDatabaseInitializationStep8Async(bool waitForInput = true)
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Si no hay consola interactiva, continuar sin limpiar
        }
        
        Console.WriteLine("========================================");
        Console.WriteLine("   Inicialización de Base de Datos");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine($"Log: {_logService.GetLogFilePath()}");
        Console.WriteLine();
        
        var initCmdResult = await _initializeDatabaseCommand.HandleAsync(new InitializeDatabaseInput());

        if (initCmdResult == null || initCmdResult.Data == null)
        {
             Console.WriteLine("ERROR: El comando devolvió null.");
             return false;
        }

        var result = initCmdResult.Data;
        foreach(var l in initCmdResult.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
        
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.ForegroundColor = result.Status == "ok" ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"   Resultado: {result.Status.ToUpper()}");
        Console.ResetColor();
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Mostrar información del proceso de forma visual
        if (result.Information.Any())
        {
            Console.WriteLine("Información del proceso:");
            Console.WriteLine();
            foreach (var info in result.Information)
            {
                if (info.Contains("✓"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  {info}");
                    Console.ResetColor();
                }
                else if (info.Contains("⚠"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  {info}");
                    Console.ResetColor();
                }
                else if (info.Contains("Paso") || info.Contains("Iniciando") || info.Contains("==="))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  {info}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {info}");
                }
            }
            Console.WriteLine();
        }

        // Mostrar errores si los hay
        if (result.Errors.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Errores encontrados:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  ❌ {error}");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        // Mostrar mensaje resumen
        if (!string.IsNullOrEmpty(result.Message))
        {
            Console.ForegroundColor = result.Status == "ok" ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"Mensaje: {result.Message}");
            Console.ResetColor();
            Console.WriteLine();
        }

        if (result.Status == "ok")
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Proceso completado exitosamente");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Proceso falló");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Para más detalles, revisa el archivo de log:");
            Console.WriteLine($"  {_logService.GetLogFilePath()}");
        }
        
        Console.WriteLine();
        
        // Solo esperar entrada si se ejecuta en modo interactivo
        if (waitForInput)
        {
            Console.WriteLine("Presione cualquier tecla para continuar...");
            try
            {
                SafeReadKey();
            }
            catch (InvalidOperationException)
            {
                // Si no hay consola interactiva, continuar sin esperar
            }
        }
        
        return result.Status == "ok";
    }

    /// <summary>
    /// Muestra el menú de Docker
    /// </summary>
    private async Task<bool> ExecuteDockerMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Gestión de Contenedores Docker");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  1. Eliminar contenedores");
        Console.WriteLine("  2. Crear contenedores");
        Console.WriteLine("  3. Eliminar y crear (reiniciar)");
        Console.WriteLine("  4. Volver al menú principal");
        Console.WriteLine();
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    var r1 = await _removeContainersCommand.HandleAsync(new RemoveContainersInput());
                    foreach(var l in r1?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
                    break;
                case 2:
                    var r2 = await _createContainersCommand.HandleAsync(new CreateContainersInput());
                    foreach(var l in r2?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
                    break;
                case 3:
                    var r3a = await _removeContainersCommand.HandleAsync(new RemoveContainersInput());
                    foreach(var l in r3a?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
                    var r3b = await _createContainersCommand.HandleAsync(new CreateContainersInput());
                    foreach(var l in r3b?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
        return true;
    }

    /// <summary>
    /// Muestra el menú de migraciones
    /// </summary>
    private async Task<bool> ExecuteMigrationsMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Gestión de Migraciones");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  1. Crear migración inicial (si no existe)");
        Console.WriteLine("  2. Aplicar migraciones");
        Console.WriteLine("  3. Crear y aplicar migraciones");
        Console.WriteLine("  4. Volver al menú principal");
        Console.WriteLine();
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    var r1 = await _createInitialMigrationCommand.HandleAsync(new CreateInitialMigrationInput());
                    foreach(var l in r1?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
                    break;
                case 2:
                    var r2 = await _applyMigrationsCommand.HandleAsync(new ApplyMigrationsInput());
                    foreach(var l in r2?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
                    break;
                case 3:
                    var r3a = await _createInitialMigrationCommand.HandleAsync(new CreateInitialMigrationInput());
                    foreach(var l in r3a?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
                    var r3b = await _applyMigrationsCommand.HandleAsync(new ApplyMigrationsInput());
                    foreach(var l in r3b?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
        return true;
    }

    /// <summary>
    /// Muestra el menú de seeds con selector de ámbito
    /// </summary>
    private async Task<bool> ExecuteSeedsMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Ejecución de Seeds");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Seleccione el ámbito:");
        Console.WriteLine("  1. Shared (datos compartidos)");
        Console.WriteLine("  2. Admin (datos administrativos)");
        Console.WriteLine("  3. Product (datos de producto)");
        Console.WriteLine("  4. All (todos los ámbitos)");
        Console.WriteLine("  5. Volver al menú principal");
        Console.WriteLine();
        Console.Write("Ámbito: ");

        if (!int.TryParse(Console.ReadLine(), out int scopeOption) || scopeOption < 1 || scopeOption > 5)
        {
            Console.WriteLine("Opción no válida.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }

        if (scopeOption == 5)
        {
            return true;
        }

        var scope = scopeOption switch
        {
            1 => SeedScope.Shared,
            2 => SeedScope.Admin,
            3 => SeedScope.Product,
            4 => SeedScope.All,
            _ => SeedScope.All
        };

        Console.WriteLine();
        Console.WriteLine("Seleccione el nivel:");
        Console.WriteLine("  1. Master (datos maestros)");
        Console.WriteLine("  2. Demo (datos de demostración)");
        Console.WriteLine("  3. Test (datos de prueba)");
        Console.WriteLine();
        Console.Write("Nivel: ");

        if (!int.TryParse(Console.ReadLine(), out int levelOption) || levelOption < 1 || levelOption > 3)
        {
            Console.WriteLine("Opción no válida.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }

        var level = levelOption switch
        {
            1 => SeedLevel.Master,
            2 => SeedLevel.Demo,
            3 => SeedLevel.Test,
            _ => SeedLevel.Master
        };

        Console.WriteLine();

        var input = new SeedCommandInput { Scope = scope, Level = level };
        var result = await _seedCommand.HandleAsync(input);

        // Imprimir logs devueltos por el comando
        foreach (var log in result.Logs)
        {
            Console.WriteLine(log);
        }

        if (result.Success)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(result.Message);
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {result.Message}");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
        return true;
    }

    /// <summary>
    /// Ejecuta la validación de integridad completa
    /// </summary>
    private async Task<bool> ExecuteIntegrityValidationAsync()
    {
        var result = await _integrityValidationService.ValidateEcosystemAsync();
        
        Console.WriteLine();
        if (result.IsValid)
        {
            Console.WriteLine("Presione cualquier tecla para continuar...");
        }
        else
        {
            Console.WriteLine("Revisa los errores anteriores antes de continuar.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
        }
        SafeReadKey();
        
        return true;
    }

    /// <summary>
    /// Ejecuta el cumplimiento de reglas de oro
    /// </summary>
    private async Task<bool> ExecuteGoldenRulesComplianceAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Cumplimiento de Reglas de Oro");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Este proceso verificará:");
        Console.WriteLine("  • Sincronización de Seeds con entidades");
        Console.WriteLine("  • Sincronización de Tests con entidades");
        Console.WriteLine("  • Detección de cambios en entidades");
        Console.WriteLine();
        Console.WriteLine("El proceso puede continuar desde donde se quedó.");
        Console.WriteLine();
        Console.WriteLine("¿Desea forzar verificación completa? (s/N): ");
        var forceInput = Console.ReadLine();
        var forceFull = forceInput?.Trim().ToLower() == "s" || forceInput?.Trim().ToLower() == "sí";

        var result = await _goldenRulesService.EnforceGoldenRulesAsync(forceFull);
        
        Console.WriteLine();
        if (result.Success)
        {
            if (result.HasWarnings)
            {
                Console.WriteLine("Proceso completado con advertencias. Revisa las entidades que requieren atención.");
            }
            else
            {
                Console.WriteLine("✓ Proceso completado exitosamente.");
            }
        }
        else
        {
            Console.WriteLine($"✗ Error durante el proceso: {result.Error}");
        }
        
        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
        
        return true;
    }

    /// <summary>
    /// Ejecuta el squash de migraciones
    /// </summary>
    private async Task<bool> ExecuteMigrationSquashAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Squash de Migraciones");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Este proceso realizará:");
        Console.WriteLine("  • Eliminación de todas las migraciones existentes");
        Console.WriteLine("  • Generación de una nueva migración inicial única");
        Console.WriteLine("  • Verificación de que incluya las tablas Logs y AdminUsers");
        Console.WriteLine();
        Console.WriteLine("⚠ ADVERTENCIA: Este proceso eliminará todas las migraciones existentes.");
        Console.WriteLine("   Asegúrate de tener un backup si es necesario.");
        Console.WriteLine();
        Console.Write("¿Desea continuar? (s/N): ");
        var confirm = Console.ReadLine();
        
        if (confirm?.Trim().ToLower() != "s" && confirm?.Trim().ToLower() != "sí")
        {
            Console.WriteLine();
            Console.WriteLine("Operación cancelada.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }

        Console.WriteLine();
        Console.WriteLine("Ejecutando squash de migraciones...");
        Console.WriteLine();

        var cmdResult = await _squashMigrationsCommand.HandleAsync(new SquashMigrationsInput());

        if (cmdResult == null)
        {
            Console.WriteLine("ERROR: El comando devolvió null.");
            return true;
        }

        var result = cmdResult.Data; // Accessing data

        // Print Logs
        foreach (var log in cmdResult.Logs ?? Enumerable.Empty<string>())
        {
            Console.WriteLine(log);
        }

        Console.WriteLine();
        Console.WriteLine("========================================");
        if (cmdResult.Success && result != null)
        {
            Console.WriteLine("   ✓ Squash de migraciones completado");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("Resumen:");
            Console.WriteLine($"  • Archivos eliminados: {result.DeletedFilesCount}");
            Console.WriteLine($"  • Archivos creados: {result.CreatedFilesCount}");
            Console.WriteLine($"  • Total de tablas en migración: {result.TotalTablesInMigration}");
            Console.WriteLine();
            
            if (result.CreatedFiles.Count > 0)
            {
                Console.WriteLine("Archivos de migración creados:");
                foreach (var file in result.CreatedFiles)
                {
                    Console.WriteLine($"  - {file}");
                }
                Console.WriteLine();
            }

            if (result.TablesFound.Count > 0)
            {
                Console.WriteLine("Tablas encontradas en la migración:");
                var criticalTables = new[] { "Logs", "AdminUsers" };
                foreach (var table in result.TablesFound.OrderBy(t => t))
                {
                    var isCritical = criticalTables.Contains(table);
                    var marker = isCritical ? "✓" : "  ";
                    Console.WriteLine($"  {marker} {table}");
                }
                Console.WriteLine();
            }

            // Verificar tablas críticas
            var hasLogs = result.TablesFound.Contains("Logs");
            var hasAdminUsers = result.TablesFound.Contains("AdminUsers");
            
            if (hasLogs && hasAdminUsers)
            {
                Console.WriteLine("✓ Tablas críticas (Logs y AdminUsers) incluidas correctamente");
            }
            else
            {
                Console.WriteLine("⚠ ADVERTENCIA: Algunas tablas críticas no se encontraron:");
                if (!hasLogs) Console.WriteLine("  - Logs");
                if (!hasAdminUsers) Console.WriteLine("  - AdminUsers");
            }
        }
        else
        {
            Console.WriteLine("   ✗ Squash de migraciones falló");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine($"Error: {cmdResult.Message}");
        }

        // Original code printed result.Messages. My Command puts them in Logs.
        // So printing Logs (above) covers it.

        Console.WriteLine();
        Console.WriteLine($"Log completo disponible en: {_logService.GetLogFilePath()}");
        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();

        return true;
    }

    /// <summary>
    /// Muestra el menú de ejecución de tests
    /// </summary>
    private async Task<bool> ExecuteTestsMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Ejecución de Tests");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  1. Tests Unitarios (Local)");
        Console.WriteLine("  2. Tests de Integridad (Docker)");
        Console.WriteLine("  3. Tests E2E (Playwright - Docker)");
        Console.WriteLine("  4. Todos los Tests");
        Console.WriteLine("  5. Volver al menú principal");
        Console.WriteLine();
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    await _runUnitTestsCommand.HandleAsync(new RunUnitTestsInput());
                    break;
                case 2:
                    await _runIntegrationTestsCommand.HandleAsync(new RunIntegrationTestsInput());
                    break;
                case 3:
                    await _runE2ETestsCommand.HandleAsync(new RunE2ETestsInput());
                    break;
                case 4:
                    await _runUnitTestsCommand.HandleAsync(new RunUnitTestsInput());
                    await _runIntegrationTestsCommand.HandleAsync(new RunIntegrationTestsInput());
                    await _runE2ETestsCommand.HandleAsync(new RunE2ETestsInput());
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        SafeReadKey();
        return true;
    }
}
