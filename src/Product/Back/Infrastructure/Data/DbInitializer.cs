using GesFer.Infrastructure.Services;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Inicializador de base de datos que aplica migraciones y carga datos iniciales desde archivos JSON.
/// Este proceso es completamente idempotente y seguro de ejecutar múltiples veces.
/// </summary>
public class DbInitializer
{
    private readonly IMigrationService _migrationService;
    private readonly IIntegrityCheckService _integrityChecker;
    private readonly JsonDataSeeder _seeder;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        IMigrationService migrationService,
        IIntegrityCheckService integrityChecker,
        JsonDataSeeder seeder,
        IHostEnvironment environment,
        ILogger<DbInitializer> logger)
    {
        _migrationService = migrationService;
        _integrityChecker = integrityChecker;
        _seeder = seeder;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa la base de datos aplicando migraciones pendientes y cargando datos iniciales desde JSON.
    /// Se ejecuta en modo Development o Testing.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Ejecutar en modo Development o Testing
        // En Testing, también ejecutamos migraciones para tests E2E
        var shouldInitialize = _environment.IsDevelopment() || _environment.EnvironmentName == "Testing";

        if (!shouldInitialize)
        {
            return;
        }

        try
        {
            _logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            // Paso 1: Aplicar migraciones pendientes
            await _migrationService.ApplyMigrationsAsync(cancellationToken);

            // Paso 2: Cargar datos iniciales desde JSON
            await SeedDataFromJsonAsync();

            // Paso 3: Verificar integridad y Smoke Tests
            await _integrityChecker.EnsureIntegrityAsync(cancellationToken);

            _logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
        }
    }

    /// <summary>
    /// Carga datos iniciales desde archivos JSON de forma idempotente (orden interno: master → demo/test).
    /// </summary>
    private async Task SeedDataFromJsonAsync()
    {
        try
        {
            await SeedMasterDataAsync();
            await SeedDemoDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar datos iniciales desde JSON");
            throw;
        }
    }

    /// <summary>
    /// Carga solo datos maestros (master-data.json). Orden de seeds: 1 - Maestros, 2 - Admin, 3 - Product.
    /// </summary>
    public async Task SeedMasterDataAsync()
    {
        var result = await _seeder.SeedMasterDataAsync();
        if (result.Loaded && result.Entities.Any())
            _logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", result.Entities));
    }

    /// <summary>
    /// Carga solo datos de producto/demo (demo-data.json o test-data.json en Testing). Orden de seeds: 1 - Maestros, 2 - Admin, 3 - Product.
    /// </summary>
    public async Task SeedDemoDataAsync()
    {
        if (_environment.EnvironmentName == "Testing")
        {
            await _seeder.SeedTestDataAsync();
            _logger.LogInformation("test-data.json cargado (modo Testing)");
        }
        else
        {
            var result = await _seeder.SeedDemoDataAsync();
            if (result.Loaded && result.Entities.Any())
                _logger.LogInformation("Seeds cargados: {Entities}", string.Join(", ", result.Entities));
        }
    }
}
