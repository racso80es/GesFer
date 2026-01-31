using System;
using System.Diagnostics;
using GesFer.ConsoleApp.Commands;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para mostrar y gestionar el menú interactivo
/// </summary>
public class MenuService
{
    private readonly CheckDockerCommand _checkDockerCommand;
    private readonly RemoveContainersCommand _removeContainersCommand;
    private readonly CreateContainersCommand _createContainersCommand;
    private readonly WaitMySqlReadyCommand _waitMySqlReadyCommand;
    private readonly ApplyMigrationsCommand _applyMigrationsCommand;
    private readonly CreateInitialMigrationCommand _createInitialMigrationCommand;
    private readonly SquashMigrationsCommand _squashMigrationsCommand;
    private readonly EnsureEfToolCommand _ensureEfToolCommand;
    private readonly SeedCommand _seedCommand;
    private readonly InitializeDatabaseCommand _initializeDatabaseCommand;
    private readonly IntegrityValidationService _integrityValidationService;
    private readonly GoldenRulesComplianceService _goldenRulesService;
    private readonly LogService _logService;

    public MenuService(
        CheckDockerCommand checkDockerCommand,
        RemoveContainersCommand removeContainersCommand,
        CreateContainersCommand createContainersCommand,
        WaitMySqlReadyCommand waitMySqlReadyCommand,
        ApplyMigrationsCommand applyMigrationsCommand,
        CreateInitialMigrationCommand createInitialMigrationCommand,
        SquashMigrationsCommand squashMigrationsCommand,
        EnsureEfToolCommand ensureEfToolCommand,
        SeedCommand seedCommand,
        InitializeDatabaseCommand initializeDatabaseCommand,
        IntegrityValidationService integrityValidationService,
        GoldenRulesComplianceService goldenRulesService,
        LogService logService)
    {
        _checkDockerCommand = checkDockerCommand;
        _removeContainersCommand = removeContainersCommand;
        _createContainersCommand = createContainersCommand;
        _waitMySqlReadyCommand = waitMySqlReadyCommand;
        _applyMigrationsCommand = applyMigrationsCommand;
        _createInitialMigrationCommand = createInitialMigrationCommand;
        _squashMigrationsCommand = squashMigrationsCommand;
        _ensureEfToolCommand = ensureEfToolCommand;
        _seedCommand = seedCommand;
        _initializeDatabaseCommand = initializeDatabaseCommand;
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
    /// Muestra el menú principal
    /// </summary>
    public void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("        GesFer - Consola de Gestión");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Seleccione una opción:");
        Console.WriteLine();
        Console.WriteLine("  1. Inicialización completa");
        Console.WriteLine("  2. Inicialización de base de datos");
        Console.WriteLine("  3. Validación de integridad completa");
        Console.WriteLine("  4. Cumplimiento de Reglas de Oro (continuar desde último punto)");
        Console.WriteLine("  5. Gestionar contenedores Docker");
        Console.WriteLine("  6. Aplicar migraciones de BD");
        Console.WriteLine("  7. Ejecutar seeds de datos");
        Console.WriteLine("  8. Squash de migraciones (Resetear y crear migración inicial única)");
        Console.WriteLine("  9. Salir");
        Console.WriteLine();
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
                    return await ExecuteDatabaseInitializationStep8Async(waitForInput);
                case 3:
                    return await ExecuteIntegrityValidationAsync();
                case 4:
                    return await ExecuteGoldenRulesComplianceAsync();
                case 5:
                    return await ExecuteDockerMenuAsync();
                case 6:
                    return await ExecuteMigrationsMenuAsync();
                case 7:
                    return await ExecuteSeedsMenuAsync();
                case 8:
                    return await ExecuteMigrationSquashAsync();
                case 9:
                    return false; // Salir
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
        Console.WriteLine("[1/9] Verificando Docker...");
        var dockerCheck = await _checkDockerCommand.HandleAsync(new CheckDockerInput());
        if (!dockerCheck.Success || !dockerCheck.Data)
        {
            Console.WriteLine("ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }
        Console.WriteLine("    ✓ Docker está corriendo");
        Console.WriteLine();

        // 2. Verificar que la API compila
        Console.WriteLine("[2/9] Verificando compilación de la API...");
        // Ruta corregida: _logService.GetRootPath() es la raíz del repo, luego src/Product/Back/Api
        var apiProjectPath = Path.Combine(_logService.GetRootPath(), "src", "Product", "Back", "Api", "GesFer.Api.csproj");
        
        if (!File.Exists(apiProjectPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("    ⚠ Advertencia: No se encontró el proyecto de la API en:");
            Console.WriteLine($"      {apiProjectPath}");
            Console.ResetColor();
            Console.WriteLine("    Continuando sin verificar compilación...");
            Console.WriteLine();
        }
        else
        {
            try
            {
                var buildProcess = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build \"{apiProjectPath}\" --no-incremental",
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
                }
                else
                {
                    var output = await buildProcessInstance.StandardOutput.ReadToEndAsync();
                    var error = await buildProcessInstance.StandardError.ReadToEndAsync();
                    await buildProcessInstance.WaitForExitAsync();

                    if (buildProcessInstance.ExitCode != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("    ❌ ERROR: La API no compila. Abortando inicialización.");
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
                        Console.WriteLine($"Ruta del proyecto: {apiProjectPath}");
                        Console.WriteLine();
                        Console.WriteLine("Presione cualquier tecla para continuar...");
                        SafeReadKey();
                        
                        _logService.WriteError($"La API no compila. ExitCode: {buildProcessInstance.ExitCode}");
                        _logService.WriteLog($"Salida de compilación: {output}");
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            _logService.WriteLog($"Errores de compilación: {error}");
                        }
                        
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("    ✓ API compila correctamente");
                        _logService.WriteLog("API compilada correctamente");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    ⚠ Advertencia: Error al verificar compilación: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("    Continuando sin verificar compilación...");
                _logService.WriteError("Error al verificar compilación de la API", ex);
            }
            Console.WriteLine();
        }

        // 3. Eliminar contenedores
        Console.WriteLine("[3/9] Limpiando contenedores existentes...");
        var rmResult = await _removeContainersCommand.HandleAsync(new RemoveContainersInput());
        if (rmResult?.Logs != null)
        {
            foreach(var l in rmResult.Logs) Console.WriteLine(l);
        }
        Console.WriteLine();

        // 4. Crear contenedores
        Console.WriteLine("[4/9] Creando contenedores Docker...");
        var createResult = await _createContainersCommand.HandleAsync(new CreateContainersInput());
        foreach(var l in createResult?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);

        if (createResult == null || !createResult.Success)
        {
            Console.WriteLine("ERROR: No se pudieron crear los contenedores");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }
        Console.WriteLine();

        // 5. Esperar MySQL
        Console.WriteLine("[5/9] Esperando a que MySQL esté listo...");
        var waitResult = await _waitMySqlReadyCommand.HandleAsync(new WaitMySqlInput());
        foreach(var l in waitResult?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(l);

        if (waitResult == null || !waitResult.Success)
        {
            Console.WriteLine("ERROR: MySQL no está listo");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            SafeReadKey();
            return true;
        }
        Console.WriteLine();

        // 6. Verificar/Instalar dotnet-ef
        Console.WriteLine("[6/9] Verificando herramienta dotnet-ef...");
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

        // 7. Crear migraciones si no existen
        Console.WriteLine("[7/9] Verificando migraciones...");
        var initMigResult = await _createInitialMigrationCommand.HandleAsync(new CreateInitialMigrationInput());
        foreach (var log in initMigResult?.Logs ?? Enumerable.Empty<string>()) Console.WriteLine(log);

        if (initMigResult == null || !initMigResult.Success)
        {
             // Logged in command, but we should probably stop if strict?
        }
        Console.WriteLine();

        // 8. Aplicar migraciones y ejecutar seeds usando DatabaseInitializationService (Command)
        Console.WriteLine("[8/9] Aplicando migraciones y cargando datos iniciales desde JSON...");
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
}
