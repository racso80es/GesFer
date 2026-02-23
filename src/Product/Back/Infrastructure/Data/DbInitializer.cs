using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Inicializador de base de datos que aplica migraciones y carga datos iniciales.
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
    /// Inicializa la base de datos aplicando migraciones y cargando datos.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Nota: La lógica de "shouldInitialize" (isDevelopment || Testing) se mueve al caller (Program.cs)
        // O se puede mantener aquí si se inyectan las flags, pero es mejor que el orquestador decida si llamar o no.

        try
        {
            _logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            // Paso 1: Aplicar migraciones
            await _migrationService.ApplyMigrationsAsync();

            // Paso 2: Cargar datos iniciales
            await SeedDataAsync();

            // Paso 3: Verificar integridad
            await _integrityCheckService.EnsureIntegrityAsync();

            _logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        try
        {
            // 1. Maestros
            var masterResult = await _seeder.SeedMasterDataAsync();
            if (masterResult.Loaded && masterResult.Entities.Any())
                _logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", masterResult.Entities));

            // 2. Demo/Test Data
            if (_environment.EnvironmentName == "Testing")
            {
                await _seeder.SeedTestDataAsync();
                _logger.LogInformation("test-data.json cargado (modo Testing)");
            }
            else
            {
                var demoResult = await _seeder.SeedDemoDataAsync();
                if (demoResult.Loaded && demoResult.Entities.Any())
                    _logger.LogInformation("Seeds cargados: {Entities}", string.Join(", ", demoResult.Entities));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar datos iniciales desde JSON");
            throw;
        }
    }
}
