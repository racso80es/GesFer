using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    /// Solo se ejecuta si isDevelopment es true.
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios</param>
    /// <param name="isDevelopment">Indica si estamos en modo Development</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, bool isDevelopment)
    {
        // Solo ejecutar en modo Development
        if (!isDevelopment)
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

            // Verificar conexión a la base de datos
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogWarning("No se puede conectar a la base de datos. Intentando crear la base de datos...");
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Base de datos creada exitosamente");
                }
                catch (Exception createEx)
                {
                    logger.LogError(createEx, "Error al crear la base de datos");
                    throw new InvalidOperationException("No se pudo conectar ni crear la base de datos", createEx);
                }
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al cargar datos iniciales desde JSON");
            throw;
        }
    }

}
