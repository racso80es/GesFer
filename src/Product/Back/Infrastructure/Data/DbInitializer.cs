using GesFer.Infrastructure.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using GesFer.Infrastructure.Data;

using GesFer.Shared.Back.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Inicializador de base de datos que orquesta las migraciones, la carga de datos iniciales desde archivos JSON
/// y la verificación de integridad. Este proceso es completamente idempotente y seguro de ejecutar múltiples veces.
/// </summary>
public class DbInitializer
{
    private readonly IMigrationService _migrationService;
    private readonly JsonDataSeeder _seeder;
    private readonly IIntegrityCheckService _integrityChecker;
    private readonly IHostEnvironment _environment;
    private readonly ProductDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        IMigrationService migrationService,
        JsonDataSeeder seeder,
        IIntegrityCheckService integrityChecker,
        IHostEnvironment environment,
        ProductDbContext context,
        ILogger<DbInitializer> logger)
    {
        _migrationService = migrationService;
        _seeder = seeder;
        _integrityChecker = integrityChecker;
        _environment = environment;
        _context = context;
        _logger = logger;
    }

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
        var logger = loggerFactory.CreateLogger<DbInitializer>();

        var context = services.GetRequiredService<ProductDbContext>();
        var migrationService = services.GetRequiredService<IMigrationService>();
        var seeder = services.GetRequiredService<JsonDataSeeder>();
        var integrityChecker = services.GetRequiredService<IIntegrityCheckService>();

        var initializer = new DbInitializer(
            migrationService,
            seeder,
            integrityChecker,
            environment,
            context,
            logger
        );

        await initializer.InitializeInternalAsync();
    }

    private async Task InitializeInternalAsync()
    {
        try
        {
            _logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            // Paso 1: Aplicar migraciones pendientes
            await _migrationService.ApplyMigrationsAsync();

            // Paso 2: Cargar datos iniciales desde JSON
            await SeedDataFromJsonAsync();

            // CRÍTICO: Evitar conflictos de tracking (Seeder puede haber dejado entidades en ChangeTracker)
            _context.ChangeTracker.Clear();

            // Paso 3: Garantizar usuario admin y smoke test
            await _integrityChecker.EnsureIntegrityAsync();

            _logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
        }
    }

    private async Task SeedDataFromJsonAsync()
    {
        try
        {
            var resultMaster = await _seeder.SeedMasterDataAsync();
            if (resultMaster.Loaded && resultMaster.Entities.Any())
                _logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", resultMaster.Entities));

            if (_environment.EnvironmentName == "Testing")
            {
                await _seeder.SeedTestDataAsync();
                _logger.LogInformation("test-data.json cargado (modo Testing)");
            }
            else
            {
                var resultDemo = await _seeder.SeedDemoDataAsync();
                if (resultDemo.Loaded && resultDemo.Entities.Any())
                    _logger.LogInformation("Seeds demo cargados: {Entities}", string.Join(", ", resultDemo.Entities));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar datos iniciales desde JSON");
            throw;
        }
    }

    /// <summary>
    /// Legacy static method para compatibilidad con código existente (InitializeDatabaseCommand)
    /// </summary>
    public static async Task SeedMasterDataAsync(ProductDbContext context, IServiceProvider services, ILogger logger)
    {
        var seeder = services.GetRequiredService<JsonDataSeeder>();
        var result = await seeder.SeedMasterDataAsync();
        if (result.Loaded && result.Entities.Any())
            logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", result.Entities));
    }

    /// <summary>
    /// Legacy static method para compatibilidad con código existente (InitializeDatabaseCommand)
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
    /// Legacy static method para compatibilidad con código existente (InitializeDatabaseCommand)
    /// </summary>
    public static async Task EnsureAdminUserAndSmokeTestAsync(ProductDbContext context, IServiceProvider services, ILogger logger)
    {
        var integrityChecker = services.GetRequiredService<IIntegrityCheckService>();
        await integrityChecker.EnsureIntegrityAsync();
    }
}
