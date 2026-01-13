using System;
using System.Diagnostics;
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
    private readonly DockerService _dockerService;
    private readonly MigrationService _migrationService;
    private readonly SeedService _seedService;
    private readonly IntegrityValidationService _integrityValidationService;
    private readonly GoldenRulesComplianceService _goldenRulesService;
    private readonly LogService _logService;

    public MenuService(
        DockerService dockerService,
        MigrationService migrationService,
        SeedService seedService,
        IntegrityValidationService integrityValidationService,
        GoldenRulesComplianceService goldenRulesService,
        LogService logService)
    {
        _dockerService = dockerService;
        _migrationService = migrationService;
        _seedService = seedService;
        _integrityValidationService = integrityValidationService;
        _goldenRulesService = goldenRulesService;
        _logService = logService;
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
        Console.WriteLine("  2. Validación de integridad completa");
        Console.WriteLine("  3. Cumplimiento de Reglas de Oro (continuar desde último punto)");
        Console.WriteLine("  4. Gestionar contenedores Docker");
        Console.WriteLine("  5. Aplicar migraciones de BD");
        Console.WriteLine("  6. Ejecutar seeds de datos");
        Console.WriteLine("  7. Squash de migraciones (Resetear y crear migración inicial única)");
        Console.WriteLine("  8. Salir");
        Console.WriteLine();
        Console.Write("Opción: ");
    }

    /// <summary>
    /// Ejecuta la opción seleccionada
    /// </summary>
    public async Task<bool> ExecuteOptionAsync(int option)
    {
        try
        {
            switch (option)
            {
                case 1:
                    return await ExecuteFullInitializationAsync();
                case 2:
                    return await ExecuteIntegrityValidationAsync();
                case 3:
                    return await ExecuteGoldenRulesComplianceAsync();
                case 4:
                    return await ExecuteDockerMenuAsync();
                case 5:
                    return await ExecuteMigrationsMenuAsync();
                case 6:
                    return await ExecuteSeedsMenuAsync();
                case 7:
                    return await ExecuteMigrationSquashAsync();
                case 8:
                    return false; // Salir
                default:
                    Console.WriteLine("Opción no válida. Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
    }

    /// <summary>
    /// Ejecuta la inicialización completa
    /// </summary>
    private async Task<bool> ExecuteFullInitializationAsync()
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
        if (!await _dockerService.IsDockerRunningAsync())
        {
            Console.WriteLine("ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
        Console.WriteLine("    ✓ Docker está corriendo");
        Console.WriteLine();

        // 2. Verificar que la API compila
        Console.WriteLine("[2/9] Verificando compilación de la API...");
        var apiProjectPath = Path.Combine(_logService.GetRootPath(), "Api", "src", "Api", "GesFer.Api.csproj");
        
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
                        Console.ReadKey();
                        
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
        await _dockerService.RemoveContainersAsync();
        Console.WriteLine();

        // 4. Crear contenedores
        Console.WriteLine("[4/9] Creando contenedores Docker...");
        if (!await _dockerService.CreateContainersAsync())
        {
            Console.WriteLine("ERROR: No se pudieron crear los contenedores");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
        Console.WriteLine();

        // 5. Esperar MySQL
        Console.WriteLine("[5/9] Esperando a que MySQL esté listo...");
        if (!await _dockerService.WaitForMySqlReadyAsync())
        {
            Console.WriteLine("ERROR: MySQL no está listo");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
        Console.WriteLine();

        // 6. Verificar/Instalar dotnet-ef
        Console.WriteLine("[6/9] Verificando herramienta dotnet-ef...");
        if (!await _migrationService.IsEfToolInstalledAsync())
        {
            if (!await _migrationService.InstallEfToolAsync())
            {
                Console.WriteLine("ERROR: No se pudo instalar dotnet-ef");
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
                return true;
            }
        }
        else
        {
            Console.WriteLine("    ✓ Herramienta dotnet-ef encontrada");
        }
        Console.WriteLine();

        // 7. Crear migraciones si no existen
        Console.WriteLine("[7/9] Verificando migraciones...");
        await _migrationService.CreateInitialMigrationIfNeededAsync();
        Console.WriteLine();

        // 8. Aplicar migraciones y ejecutar seeds usando DbInitializer (sistema profesionalizado)
        Console.WriteLine("[8/9] Aplicando migraciones y cargando datos iniciales desde JSON...");
        if (!await ExecuteDatabaseInitializationAsync())
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: No se pudo inicializar la base de datos");
            Console.WriteLine();
            Console.WriteLine("Para más detalles, revisa el archivo de log:");
            Console.WriteLine($"  {_logService.GetLogFilePath()}");
            Console.WriteLine();
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
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
        Console.WriteLine("  Empresa: Empresa Demo");
        Console.WriteLine("  Usuario: admin");
        Console.WriteLine("  Contraseña: admin123");
        Console.WriteLine();
        Console.WriteLine("Servicios disponibles:");
        Console.WriteLine("  - MySQL: localhost:3306");
        Console.WriteLine("  - Memcached: localhost:11211");
        Console.WriteLine("  - Adminer: http://localhost:8080");
        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();

        return true;
    }

    /// <summary>
    /// Ejecuta la inicialización de base de datos usando DbInitializer (migraciones + seeding desde JSON)
    /// </summary>
    private async Task<bool> ExecuteDatabaseInitializationAsync()
    {
        try
        {
            // Usar el mismo sistema que la API para mantener consistencia
            // Esto aplica migraciones y carga datos desde JSON de forma idempotente
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            var apiPath = Path.Combine(rootPath, "Api", "src", "Api");

            // Configurar servicios igual que la API
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 0)),
                    mysqlOptions =>
                    {
                        mysqlOptions.EnableStringComparisonTranslations();
                        mysqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            services.AddScoped<JsonDataSeeder>();
            services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();

            var serviceProvider = services.BuildServiceProvider();
            using (serviceProvider as IDisposable)
            {
                // Usar DbInitializer para aplicar migraciones y seeding
                // Forzamos isDevelopment=true para que siempre ejecute en la consola
                Console.WriteLine();
                await DbInitializer.InitializeAsync(serviceProvider, isDevelopment: true);
                Console.WriteLine();
                
                _logService.WriteLog("Inicialización de base de datos completada usando DbInitializer");
                return true;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error al inicializar base de datos: {ex.Message}";
            Console.WriteLine($"    ⚠ {errorMsg}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
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
                    await _dockerService.RemoveContainersAsync();
                    break;
                case 2:
                    await _dockerService.CreateContainersAsync();
                    break;
                case 3:
                    await _dockerService.RemoveContainersAsync();
                    await _dockerService.CreateContainersAsync();
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
                    await _migrationService.CreateInitialMigrationIfNeededAsync();
                    break;
                case 2:
                    await _migrationService.ApplyMigrationsAsync();
                    break;
                case 3:
                    await _migrationService.CreateInitialMigrationIfNeededAsync();
                    await _migrationService.ApplyMigrationsAsync();
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
        return true;
    }

    /// <summary>
    /// Muestra el menú de seeds
    /// </summary>
    private async Task<bool> ExecuteSeedsMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Ejecución de Seeds");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  1. Ejecutar datos maestros");
        Console.WriteLine("  2. Ejecutar datos de muestra");
        Console.WriteLine("  3. Ejecutar datos de prueba");
        Console.WriteLine("  4. Ejecutar todos los seeds");
        Console.WriteLine("  5. Volver al menú principal");
        Console.WriteLine();
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    await _seedService.ExecuteMasterDataAsync();
                    break;
                case 2:
                    await _seedService.ExecuteSampleDataAsync();
                    break;
                case 3:
                    await _seedService.ExecuteTestDataAsync();
                    break;
                case 4:
                    await _seedService.ExecuteAllSeedsAsync();
                    break;
            }
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
        Console.ReadKey();
        
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

        var result = await _migrationService.SquashMigrationsAsync();

        Console.WriteLine();
        Console.WriteLine("========================================");
        if (result.Success)
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
            Console.WriteLine($"Error: {result.ErrorMessage}");
        }

        Console.WriteLine();
        Console.WriteLine("Mensajes del proceso:");
        foreach (var message in result.Messages)
        {
            Console.WriteLine($"  {message}");
        }

        Console.WriteLine();
        Console.WriteLine($"Log completo disponible en: {_logService.GetLogFilePath()}");
        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();

        return true;
    }
}
