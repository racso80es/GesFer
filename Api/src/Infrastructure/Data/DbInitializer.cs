using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Inicializador de base de datos que aplica migraciones y carga datos iniciales desde archivos JSON.
/// Este proceso es completamente idempotente y seguro de ejecutar múltiples veces.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Inicializa la base de datos aplicando migraciones pendientes y cargando datos iniciales desde JSON.
    /// Se ejecuta en modo Development o Testing.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios</param>
    /// <param name="isDevelopment">Indica si estamos en modo Development</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, bool isDevelopment)
    {
        // Ejecutar en modo Development o Testing
        // En Testing, también ejecutamos migraciones para tests E2E
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
        var shouldInitialize = isDevelopment || environment.EnvironmentName == "Testing";
        
        if (!shouldInitialize)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DbInitializer");
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            // Paso 1: Aplicar migraciones pendientes
            await ApplyMigrationsAsync(context, logger);

            // Paso 2: Cargar datos iniciales desde JSON
            await SeedDataFromJsonAsync(context, services, logger);

            logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
        }
    }

    /// <summary>
    /// Aplica las migraciones pendientes de forma segura e idempotente.
    /// </summary>
    private static async Task ApplyMigrationsAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Verificando migraciones pendientes...");

            // Guarda de seguridad: Verificar que el proveedor sea relacional antes de aplicar migraciones
            // Esto evita errores si por error se inyecta un proveedor no relacional (ej: In-Memory)
            if (!context.Database.IsRelational())
            {
                logger.LogWarning("Saltando migraciones: El proveedor no es relacional.");
                return;
            }

            // Verificar conexión a la base de datos
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogWarning("No se puede conectar a la base de datos. Las migraciones intentarán crear la base de datos si es necesario.");
                // No usar EnsureCreated, dejar que MigrateAsync maneje la creación de la base de datos
            }

            // Obtener migraciones pendientes
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var pendingMigrationsList = pendingMigrations.ToList();

            if (pendingMigrationsList.Any())
            {
                var migrationsList = string.Join(", ", pendingMigrationsList);
                logger.LogInformation("Se encontraron {Count} migraciones pendientes: {Migrations}",
                    pendingMigrationsList.Count,
                    migrationsList);
                
                try
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migraciones aplicadas correctamente");
                    Console.WriteLine($"    Migraciones aplicadas: {string.Join(", ", pendingMigrationsList)}");
                }
                catch (Exception migrateEx)
                {
                    // Verificar si el error es porque las tablas ya existen
                    // Esto puede ocurrir si EnsureDeletedAsync no funcionó correctamente
                    // pero las migraciones ya están aplicadas
                    if (migrateEx.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                        (migrateEx.InnerException?.Message?.Contains("already exists", StringComparison.OrdinalIgnoreCase) == true))
                    {
                        logger.LogWarning(migrateEx, 
                            "Las tablas ya existen. Verificando si las migraciones están aplicadas...");
                        
                        // Verificar si las migraciones ya están aplicadas
                        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                        var appliedMigrationsList = appliedMigrations.ToList();
                        
                        if (appliedMigrationsList.Any())
                        {
                            logger.LogInformation("Las migraciones ya están aplicadas. La base de datos está actualizada.");
                            Console.WriteLine($"    Migraciones ya aplicadas: {string.Join(", ", appliedMigrationsList)}");
                        }
                        else
                        {
                            // Las tablas existen pero las migraciones no están registradas
                            // Esto es un estado inconsistente, intentar eliminar y recrear
                            logger.LogWarning("Estado inconsistente detectado: tablas existen pero migraciones no registradas. Intentando corregir...");
                            try
                            {
                                await context.Database.EnsureDeletedAsync();
                                await context.Database.MigrateAsync();
                                logger.LogInformation("Base de datos recreada y migraciones aplicadas correctamente");
                            }
                            catch (Exception fixEx)
                            {
                                logger.LogError(fixEx, "No se pudo corregir el estado inconsistente");
                                throw new InvalidOperationException(
                                    $"Error al aplicar migraciones: {migrateEx.Message}. " +
                                    $"Verifique la configuración de la base de datos y las migraciones. " +
                                    $"Una vez corregido el problema, puede reintentar ejecutando la aplicación nuevamente.", 
                                    migrateEx);
                            }
                        }
                    }
                    else
                    {
                        logger.LogError(migrateEx, 
                            "Error al aplicar migraciones. Tipo: {ExceptionType}, Mensaje: {Message}", 
                            migrateEx.GetType().Name, 
                            migrateEx.Message);
                        throw new InvalidOperationException(
                            $"Error al aplicar migraciones: {migrateEx.Message}. " +
                            $"Verifique la configuración de la base de datos y las migraciones. " +
                            $"Una vez corregido el problema, puede reintentar ejecutando la aplicación nuevamente.", 
                            migrateEx);
                    }
                }
            }
            else
            {
                logger.LogInformation("No hay migraciones pendientes. La base de datos está actualizada.");
                Console.WriteLine("    Migraciones: ninguna pendiente");
            }
        }
        catch (InvalidOperationException)
        {
            // Re-lanzar InvalidOperationException sin envolver
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al aplicar migraciones. Tipo: {ExceptionType}", ex.GetType().Name);
            throw new InvalidOperationException(
                $"Error inesperado al aplicar migraciones: {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Carga datos iniciales desde archivos JSON de forma idempotente.
    /// </summary>
    private static async Task SeedDataFromJsonAsync(
        ApplicationDbContext context,
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            var seeder = services.GetRequiredService<JsonDataSeeder>();
            var environment = services.GetRequiredService<IHostEnvironment>();
            var isTesting = environment.EnvironmentName == "Testing";

            if (isTesting)
            {
                // En modo Testing, cargar solo test-data.json
                logger.LogInformation("Modo Testing detectado: cargando test-data.json");
                await seeder.SeedTestDataAsync();
                logger.LogInformation("test-data.json cargado correctamente");
            }
            else
            {
                // En modo Development, cargar master-data.json y demo-data.json
                // Cargar datos maestros y obtener resumen de entidades
                var masterDataResult = await seeder.SeedMasterDataAsync();
                
                // Cargar datos de demostración y obtener resumen de entidades
                var demoDataResult = await seeder.SeedDemoDataAsync();

                // Mostrar resumen conciso de entidades cargadas
                if (masterDataResult.Loaded || demoDataResult.Loaded)
                {
                    var entities = new List<string>();
                    
                    if (masterDataResult.Loaded && masterDataResult.Entities.Any())
                    {
                        entities.AddRange(masterDataResult.Entities);
                    }
                    
                    if (demoDataResult.Loaded && demoDataResult.Entities.Any())
                    {
                        entities.AddRange(demoDataResult.Entities);
                    }

                    if (entities.Any())
                    {
                        Console.WriteLine($"    Seeds cargados: {string.Join(", ", entities)}");
                    }
                }

                // Registrar en log para debugging
                if (masterDataResult.Loaded && demoDataResult.Loaded)
                {
                    logger.LogInformation("Todos los datos iniciales han sido cargados correctamente");
                }
                else
                {
                    logger.LogWarning("Algunos datos iniciales no se pudieron cargar. master-data.json: {MasterLoaded}, demo-data.json: {DemoLoaded}",
                        masterDataResult.Loaded, demoDataResult.Loaded);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al cargar datos iniciales desde JSON");
            throw;
        }
    }

}
