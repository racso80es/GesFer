using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Services;
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
    private readonly JsonDataSeeder _seeder;
    private readonly IIntegrityCheckService _integrityCheckService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        IMigrationService migrationService,
        JsonDataSeeder seeder,
        IIntegrityCheckService integrityCheckService,
        IHostEnvironment environment,
        ILogger<DbInitializer> logger)
    {
        _migrationService = migrationService;
        _seeder = seeder;
        _integrityCheckService = integrityCheckService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa la base de datos aplicando migraciones pendientes y cargando datos iniciales desde JSON.
    /// Se ejecuta en modo Development o Testing.
    /// </summary>
    public async Task InitializeAsync()
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
            await _migrationService.ApplyMigrationsAsync();

            // Paso 2: Cargar datos iniciales desde JSON
            await SeedMasterDataAsync();
            await SeedDemoDataAsync();

            // CRÍTICO: Garantizar usuario admin de forma idempotente y atómica (especialmente en Testing)
            // SMOKE TEST: Verificación de Integridad de Acceso
            await _integrityCheckService.EnsureIntegrityAsync();

            _logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
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
