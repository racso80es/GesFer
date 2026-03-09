using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Infrastructure.Services;
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
        var migrationService = services.GetRequiredService<IMigrationService>();
        var integrityChecker = services.GetRequiredService<IIntegrityCheckService>();
        var seeder = services.GetRequiredService<JsonDataSeeder>();

        try
        {
            logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            await migrationService.ApplyMigrationsAsync();

            await seeder.SeedMasterDataAsync();

            if (environment.EnvironmentName == "Testing")
            {
                await seeder.SeedTestDataAsync();
            }
            else
            {
                await seeder.SeedDemoDataAsync();
            }

            await integrityChecker.EnsureAdminUserAndSmokeTestAsync();

            logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
        }
    }
}
