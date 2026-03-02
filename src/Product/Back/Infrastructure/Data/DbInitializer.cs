using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Infrastructure.Services;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Inicializador de base de datos que coordina la aplicación de migraciones y carga datos iniciales desde archivos JSON.
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
        var context = services.GetRequiredService<ProductDbContext>();

        var migrationService = services.GetRequiredService<IMigrationService>();
        var integrityService = services.GetRequiredService<IIntegrityCheckService>();

        try
        {
            logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            await migrationService.ApplyMigrationsAsync();

            await SeedDataFromJsonAsync(context, services, logger);

            context.ChangeTracker.Clear();

            await integrityService.EnsureAdminUserAndSmokeTestAsync();

            logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
        }
    }

    /// <summary>
    /// Carga solo datos maestros (master-data.json). Orden de seeds: 1 - Maestros, 2 - Admin, 3 - Product.
    /// </summary>
    public static async Task SeedMasterDataAsync(ProductDbContext context, IServiceProvider services, ILogger logger)
    {
        var seeder = services.GetRequiredService<JsonDataSeeder>();
        var result = await seeder.SeedMasterDataAsync();
        if (result.Loaded && result.Entities.Any())
            logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", result.Entities));
    }

    /// <summary>
    /// Carga solo datos de producto/demo (demo-data.json o test-data.json en Testing). Orden de seeds: 1 - Maestros, 2 - Admin, 3 - Product.
    /// </summary>
    public static async Task SeedDemoDataAsync(ProductDbContext context, IServiceProvider services, ILogger logger)
    {
        var seeder = services.GetRequiredService<JsonDataSeeder>();
        var environment = services.GetRequiredService<IHostEnvironment>();
        if (environment.EnvironmentName == "Testing")
        {
            await seeder.SeedTestDataAsync();
            logger.LogInformation("test-data.json cargado (modo Testing)");
        }
        else
        {
            var result = await seeder.SeedDemoDataAsync();
            if (result.Loaded && result.Entities.Any())
                logger.LogInformation("Seeds cargados: {Entities}", string.Join(", ", result.Entities));
        }
    }

    /// <summary>
    /// Carga datos iniciales desde archivos JSON de forma idempotente (orden interno: master → demo/test).
    /// </summary>
    private static async Task SeedDataFromJsonAsync(
        ProductDbContext context,
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            await SeedMasterDataAsync(context, services, logger);
            await SeedDemoDataAsync(context, services, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al cargar datos iniciales desde JSON");
            throw;
        }
    }

    /// <summary>
    /// Garantiza usuario admin y ejecuta smoke test de integridad. (Obsoleto: usar IIntegrityCheckService).
    /// Mantenido por compatibilidad temporal si es llamado explícitamente.
    /// </summary>
    public static async Task EnsureAdminUserAndSmokeTestAsync(ProductDbContext context, IServiceProvider services, ILogger logger)
    {
        var integrityService = services.GetRequiredService<IIntegrityCheckService>();
        await integrityService.EnsureAdminUserAndSmokeTestAsync();
    }
}
