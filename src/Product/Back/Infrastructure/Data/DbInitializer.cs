using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Inicializador de base de datos que coordina migraciones y carga datos iniciales.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Inicializa la base de datos aplicando migraciones pendientes y cargando datos iniciales desde JSON.
    /// Se ejecuta en modo Development o Testing.
    /// </summary>
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
        var context = services.GetRequiredService<ApplicationDbContext>();

        var migrationService = services.GetRequiredService<IMigrationService>();
        var integrityCheckService = services.GetRequiredService<IIntegrityCheckService>();

        try
        {
            logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            await migrationService.ApplyMigrationsAsync(context);

            await SeedDataFromJsonAsync(context, services, logger);

            context.ChangeTracker.Clear();

            await integrityCheckService.EnsureAdminUserAsync(context, services);

            // Smoke Test simplificado en el Initializer (la lógica fuerte está en el Service)
            await integrityCheckService.EnsureAdminUserAndSmokeTestAsync(context, services);

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
    public static async Task SeedMasterDataAsync(ApplicationDbContext context, IServiceProvider services, ILogger logger)
    {
        var seeder = services.GetRequiredService<JsonDataSeeder>();
        var result = await seeder.SeedMasterDataAsync();
        if (result.Loaded && result.Entities.Any())
            logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", result.Entities));
    }

    /// <summary>
    /// Carga solo datos de producto/demo (demo-data.json o test-data.json en Testing). Orden de seeds: 1 - Maestros, 2 - Admin, 3 - Product.
    /// </summary>
    public static async Task SeedDemoDataAsync(ApplicationDbContext context, IServiceProvider services, ILogger logger)
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

    private static async Task SeedDataFromJsonAsync(
        ApplicationDbContext context,
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
}