using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Extensions;

/// <summary>
/// Extensiones para la base de datos
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Aplica las migraciones y ejecuta el seeding de datos maestros únicamente
    /// </summary>
    public static async Task MigrateAndSeedAsync(this ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        
        try
        {
            // Aplicar migraciones pendientes
            logger.LogInformation("Aplicando migraciones pendientes...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas correctamente");

            // Ejecutar seeding SOLO de datos maestros (durante migraciones)
            logger.LogInformation("Ejecutando seeding de datos maestros...");
            var seeder = serviceProvider.GetRequiredService<JsonDataSeeder>();
            await seeder.SeedMasterDataAsync();

            logger.LogInformation("Seeding de datos maestros completado correctamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al aplicar migraciones o ejecutar seeding");
            throw;
        }
    }

    /// <summary>
    /// Ejecuta solo el seeding de datos (sin migraciones)
    /// </summary>
    public static async Task SeedDataAsync(this ApplicationDbContext context, IServiceProvider serviceProvider, bool includeTestData = false)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var seeder = serviceProvider.GetRequiredService<JsonDataSeeder>();

        try
        {
            logger.LogInformation("Ejecutando seeding de datos maestros...");
            await seeder.SeedMasterDataAsync();

            logger.LogInformation("Ejecutando seeding de datos de demostración...");
            await seeder.SeedDemoDataAsync();

            if (includeTestData)
            {
                logger.LogInformation("Ejecutando seeding de datos de prueba...");
                await seeder.SeedTestDataAsync();
            }

            logger.LogInformation("Seeding completado correctamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al ejecutar seeding");
            throw;
        }
    }
}
