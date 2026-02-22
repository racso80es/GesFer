using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Inicializador de base de datos orquestador.
/// Delega responsabilidades a servicios específicos (Migración, Seeding, Integridad).
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
    /// Inicializa la base de datos completa (Migraciones -> Seeds -> Integridad).
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            await ApplyMigrationsAsync();
            await SeedMasterDataAsync();
            await SeedDemoDataAsync();
            await EnsureIntegrityAsync();

            _logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
        }
    }

    public async Task ApplyMigrationsAsync()
    {
        await _migrationService.ApplyMigrationsAsync();
    }

    public async Task SeedMasterDataAsync()
    {
        var result = await _seeder.SeedMasterDataAsync();
        if (result.Loaded && result.Entities.Any())
            _logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", result.Entities));
    }

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

    public async Task EnsureIntegrityAsync()
    {
        await _integrityCheckService.EnsureIntegrityAsync();
    }
}
