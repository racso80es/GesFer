using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Services;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// Inicializador de base de datos que aplica migraciones y carga datos iniciales.
/// Refactorizado para usar Inyección de Dependencias y separar responsabilidades.
/// </summary>
public class DbInitializer
{
    private readonly IMigrationService _migrationService;
    private readonly IIntegrityCheckService _integrityCheckService;
    private readonly JsonDataSeeder _seeder;
    private readonly ILogger<DbInitializer> _logger;
    private readonly IHostEnvironment _environment;
    private readonly ApplicationDbContext _context;

    public DbInitializer(
        IMigrationService migrationService,
        IIntegrityCheckService integrityCheckService,
        JsonDataSeeder seeder,
        ILogger<DbInitializer> logger,
        IHostEnvironment environment,
        ApplicationDbContext context)
    {
        _migrationService = migrationService;
        _integrityCheckService = integrityCheckService;
        _seeder = seeder;
        _logger = logger;
        _environment = environment;
        _context = context;
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
            await SeedDataFromJsonAsync();

            // CRÍTICO: Evitar conflictos de tracking (Seeder puede haber dejado entidades en ChangeTracker)
            _context.ChangeTracker.Clear();

            // CRÍTICO: Garantizar usuario admin de forma idempotente y atómica (especialmente en Testing)
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
    /// Carga solo datos maestros (master-data.json).
    /// </summary>
    public async Task SeedMasterDataAsync()
    {
        var result = await _seeder.SeedMasterDataAsync();
        if (result.Loaded && result.Entities.Any())
            _logger.LogInformation("Seeds maestros cargados: {Entities}", string.Join(", ", result.Entities));
    }

    /// <summary>
    /// Carga solo datos de producto/demo (demo-data.json o test-data.json en Testing).
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
}
