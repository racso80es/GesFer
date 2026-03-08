using System;
using System.Linq;
using System.Threading.Tasks;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Infrastructure.Services;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Provee funcionalidad para inicializar y cargar datos en la base de datos de la aplicación.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Orquesta la inicialización de la base de datos (migraciones, seeding y smoke tests).
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, bool isDevelopment)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

        var shouldInitialize = isDevelopment || environment.EnvironmentName == "Testing";
        if (!shouldInitialize)
        {
            logger.LogInformation("Saltando inicialización y seeding en entorno de producción.");
            return;
        }

        try
        {
            var context = serviceProvider.GetRequiredService<ProductDbContext>();
            var migrationService = serviceProvider.GetRequiredService<IMigrationService>();
            var integrityCheckService = serviceProvider.GetRequiredService<IIntegrityCheckService>();

            logger.LogInformation("=== Iniciando proceso de inicialización de base de datos ===");
            logger.LogInformation("Entorno: {Environment}, IsDevelopment: {IsDevelopment}", environment.EnvironmentName, isDevelopment);

            // 1. Aplicar Migraciones
            await migrationService.ApplyMigrationsAsync();

            // 2. Seeding de Datos (Idempotente)
            logger.LogInformation("Iniciando carga de datos de prueba...");
            await SeedDataFromJsonAsync(context, serviceProvider, logger);

            // 3. Smoke Test y Verificación del Admin
            logger.LogInformation("Verificando usuario 'admin'...");
            await integrityCheckService.VerifyAsync();

            logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
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
    /// Carga solo datos maestros (master-data.json).
    /// </summary>
    public static async Task SeedMasterDataAsync(ProductDbContext context, IServiceProvider services, ILogger logger)
    {
        var seeder = services.GetRequiredService<JsonDataSeeder>();
        var result = await seeder.SeedMasterDataAsync();
        if (result.Loaded && result.Entities.Any())
            logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", result.Entities));
    }

    /// <summary>
    /// Carga solo datos de producto/demo (demo-data.json o test-data.json en Testing).
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
}
