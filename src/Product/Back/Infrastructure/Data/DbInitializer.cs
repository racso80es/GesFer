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
        // Ejecutar en modo Development o Testing
        // En Testing, también ejecutamos migraciones para tests E2E
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

        try
        {
            logger.LogInformation("=== Iniciando inicialización de base de datos ===");

            // Paso 1: Aplicar migraciones pendientes
            await ApplyMigrationsAsync(context, logger);

            // Paso 2: Cargar datos iniciales desde JSON
            await SeedDataFromJsonAsync(context, services, logger);

            // CRÍTICO: Evitar conflictos de tracking (Seeder puede haber dejado entidades en ChangeTracker)
            // - Si 'admin' ya fue creado por JSON, lo leeremos desde DB sin duplicar instancias.
            // - Si el seeder dejó una instancia Added/Unchanged en memoria, se elimina para evitar conflicto.
            context.ChangeTracker.Clear();

            // CRÍTICO: Garantizar usuario admin de forma idempotente y atómica (especialmente en Testing)
            await EnsureAdminUserAsync(context, services, logger);

            // SMOKE TEST: Verificación de Integridad de Acceso
            var adminUser = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == "admin");

            if (adminUser == null)
            {
                logger.LogError("🔥 FALLO CRÍTICO: Usuario 'admin' existe pero no se pudo cargar.");
                throw new Exception("Usuario 'admin' no encontrado.");
            }
            if (adminUser.CompanyId == Guid.Empty || adminUser.CompanyId == default)
            {
                logger.LogError("🔥 FALLO CRÍTICO: El usuario 'admin' no tiene CompanyId vinculado.");
                throw new Exception("Usuario 'admin' sin CompanyId.");
            }

            // Empresa: verificar vía Admin API (Product no conoce BD de empresa)
            var adminClient = services.GetService<IAdminApiClient>();
            string? companyName = null;
            if (adminClient != null)
            {
                var company = await adminClient.GetCompanyAsync(adminUser.CompanyId);
                if (company == null)
                {
                    logger.LogError("🔥 FALLO CRÍTICO: La empresa {CompanyId} del admin no existe en Admin API.", adminUser.CompanyId);
                    throw new Exception("Empresa del admin no encontrada en Admin API.");
                }
                companyName = company.Name;
            }

            var companyInfo = $" (Empresa: {companyName ?? adminUser.CompanyId.ToString()}, CompanyId: {adminUser.CompanyId})";
            logger.LogInformation("✅ Smoke Test Superado: Usuario 'admin' verificado correctamente{CompanyInfo}", companyInfo);

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

    /// <summary>
    /// Garantiza usuario admin y ejecuta smoke test de integridad. Llamar tras cargar seeds en orden (master → admin → product).
    /// </summary>
    public static async Task EnsureAdminUserAndSmokeTestAsync(ApplicationDbContext context, IServiceProvider services, ILogger logger)
    {
        context.ChangeTracker.Clear();
        await EnsureAdminUserAsync(context, services, logger);

        var adminUser = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == "admin");

        if (adminUser == null)
        {
            var errorMessage = "🔥 FALLO CRÍTICO: Usuario 'admin' existe pero no se pudo cargar. Estado inconsistente detectado.";
            logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
        if (adminUser.CompanyId == Guid.Empty || adminUser.CompanyId == default(Guid))
        {
            var errorMessage = $"🔥 FALLO CRÍTICO DE INTEGRIDAD REFERENCIAL: El usuario 'admin' no tiene CompanyId vinculado (CompanyId: {adminUser.CompanyId}). El sistema sería inaccesible. Revise la vinculación en demo-data.json.";
            logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
        string? companyName = null;
        var adminClient = services.GetService<IAdminApiClient>();
        if (adminClient != null)
        {
            var company = await adminClient.GetCompanyAsync(adminUser.CompanyId);
            if (company == null)
            {
                var errorMessage = $"🔥 FALLO CRÍTICO DE INTEGRIDAD REFERENCIAL: El usuario 'admin' tiene CompanyId ({adminUser.CompanyId}) pero la empresa no existe en Admin API. Revise la vinculación en demo-data.json.";
                logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
            companyName = company.Name;
        }
        const string EXPECTED_ADMIN_COMPANY_NAME = "Empresa Admin";
        const string EXPECTED_ADMIN_COMPANY_ID = "550e8400-e29b-41d4-a716-446655440000";
        if (companyName != null && companyName != EXPECTED_ADMIN_COMPANY_NAME)
        {
            logger.LogWarning("⚠️ ADVERTENCIA: El usuario 'admin' está vinculado a '{Name}' en lugar de '{Expected}'.", companyName, EXPECTED_ADMIN_COMPANY_NAME);
        }
        if (adminUser.CompanyId.ToString() != EXPECTED_ADMIN_COMPANY_ID)
        {
            logger.LogWarning("⚠️ ADVERTENCIA: El usuario 'admin' tiene CompanyId '{Id}' en lugar del esperado '{Expected}'.", adminUser.CompanyId, EXPECTED_ADMIN_COMPANY_ID);
        }
        var companyInfo = $" (Empresa: {companyName ?? adminUser.CompanyId.ToString()}, CompanyId: {adminUser.CompanyId})";
        logger.LogInformation("✅ Smoke Test Superado: Usuario 'admin' verificado correctamente{CompanyInfo}", companyInfo);
    }

    /// <summary>
    /// Aplica las migraciones pendientes de forma segura e idempotente.
    /// </summary>
    private static async Task ApplyMigrationsAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Verificando migraciones pendientes...");

            // Guarda de seguridad: Verificar que el proveedor sea relacional antes de aplicar migraciones
            // Esto evita errores si por error se inyecta un proveedor no relacional (ej: In-Memory)
            if (!context.Database.IsRelational())
            {
                logger.LogWarning("Saltando migraciones: El proveedor no es relacional.");
                return;
            }

            // Verificar conexión a la base de datos
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogWarning("No se puede conectar a la base de datos. Las migraciones intentarán crear la base de datos si es necesario.");
                // No usar EnsureCreated, dejar que MigrateAsync maneje la creación de la base de datos
            }

            // Obtener migraciones pendientes
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var pendingMigrationsList = pendingMigrations.ToList();

            if (pendingMigrationsList.Any())
            {
                var migrationsList = string.Join(", ", pendingMigrationsList);
                logger.LogInformation("Se encontraron {Count} migraciones pendientes: {Migrations}",
                    pendingMigrationsList.Count,
                    migrationsList);

                try
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migraciones aplicadas correctamente. Migraciones: {Migrations}", string.Join(", ", pendingMigrationsList));
                }
                catch (Exception migrateEx)
                {
                    // Verificar si el error es porque las tablas ya existen
                    // Esto puede ocurrir si EnsureDeletedAsync no funcionó correctamente
                    // pero las migraciones ya están aplicadas
                    if (migrateEx.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                        (migrateEx.InnerException?.Message?.Contains("already exists", StringComparison.OrdinalIgnoreCase) == true))
                    {
                        logger.LogWarning(migrateEx,
                            "Las tablas ya existen. Verificando si las migraciones están aplicadas...");

                        // Verificar si las migraciones ya están aplicadas
                        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                        var appliedMigrationsList = appliedMigrations.ToList();

                        if (appliedMigrationsList.Any())
                        {
                            logger.LogInformation("Las migraciones ya están aplicadas. La base de datos está actualizada. Migraciones: {Migrations}", string.Join(", ", appliedMigrationsList));
                        }
                        else
                        {
                            // Las tablas existen pero las migraciones no están registradas
                            // Esto es un estado inconsistente, intentar eliminar y recrear
                            logger.LogWarning("Estado inconsistente detectado: tablas existen pero migraciones no registradas. Intentando corregir...");
                            try
                            {
                                await context.Database.EnsureDeletedAsync();
                                await context.Database.MigrateAsync();
                                logger.LogInformation("Base de datos recreada y migraciones aplicadas correctamente");
                            }
                            catch (Exception fixEx)
                            {
                                logger.LogError(fixEx, "No se pudo corregir el estado inconsistente");
                                throw new InvalidOperationException(
                                    $"Error al aplicar migraciones: {migrateEx.Message}. " +
                                    $"Verifique la configuración de la base de datos y las migraciones. " +
                                    $"Una vez corregido el problema, puede reintentar ejecutando la aplicación nuevamente.",
                                    migrateEx);
                            }
                        }
                    }
                    else
                    {
                        logger.LogError(migrateEx,
                            "Error al aplicar migraciones. Tipo: {ExceptionType}, Mensaje: {Message}",
                            migrateEx.GetType().Name,
                            migrateEx.Message);
                        throw new InvalidOperationException(
                            $"Error al aplicar migraciones: {migrateEx.Message}. " +
                            $"Verifique la configuración de la base de datos y las migraciones. " +
                            $"Una vez corregido el problema, puede reintentar ejecutando la aplicación nuevamente.",
                            migrateEx);
                    }
                }
            }
            else
            {
                logger.LogInformation("No hay migraciones pendientes. La base de datos está actualizada.");
            }
        }
        catch (InvalidOperationException)
        {
            // Re-lanzar InvalidOperationException sin envolver
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al aplicar migraciones. Tipo: {ExceptionType}", ex.GetType().Name);
            throw new InvalidOperationException(
                $"Error inesperado al aplicar migraciones: {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Carga datos iniciales desde archivos JSON de forma idempotente (orden interno: master → demo/test).
    /// Para orden completo (master → admin → product) usar desde consola los métodos públicos en secuencia.
    /// </summary>
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

    /// <summary>
    /// Garantiza que el usuario 'admin' exista tras el seeding.
    /// Debe ser idempotente (no falla si ya existe) y atómico (una sola transacción cuando sea posible).
    /// </summary>
    private static async Task EnsureAdminUserAsync(ApplicationDbContext context, IServiceProvider services, ILogger logger)
    {
        // Reglas (solo seeds crean empresas y usuarios):
        // - Usar IgnoreQueryFilters para detectar admin aunque esté soft-deleted.
        // - Si existe: solo reparar (reactivar, rellenar password si vacío, CompanyId si inválido). No crear Company ni User.
        // - Si no existe: fallar con mensaje claro; el admin debe definirse en demo-data.json o test-data.json.

        var sanitizer = services.GetRequiredService<ISensitiveDataSanitizer>();
        var environment = services.GetRequiredService<IHostEnvironment>();
        var isTesting = environment.EnvironmentName == "Testing";
        const string TestAdminPassword = "admin123"; // Solo para integración tests (AuthControllerTests, MyCompanyControllerTests)

        const string AdminUsername = "admin";
        // const string AdminPassword = "admin123"; // REMOVED SECURITY RISK
        // const string FixedAdminHash = ...; // REMOVED SECURITY RISK

        // ID de empresa demo por si el admin viene de seeds con CompanyId vacío (solo reparación)
        var defaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111115");

        async Task EnsureCoreAsync()
        {
            // Guard: si el seeder ya añadió el admin al contexto (Added/Unchanged) pero aún no está en DB,
            // no debemos intentar crear otra instancia y provocar conflicto de tracking.
            var localAdmin = context.Users.Local.FirstOrDefault(u => u.Username == AdminUsername);
            if (localAdmin != null)
            {
                // Normalizar campos mínimos sin crear nueva instancia
                if (localAdmin.DeletedAt != null)
                {
                    localAdmin.DeletedAt = null;
                    localAdmin.IsActive = true;
                }
                if (string.IsNullOrWhiteSpace(localAdmin.PasswordHash))
                {
                    var newPwd = isTesting ? TestAdminPassword : sanitizer.GenerateRandomPassword();
                    localAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd);
                    logger.LogWarning("[ENSURE ADMIN] 🔐 Set password for existing local admin: {Kind}", isTesting ? "admin123 (Testing)" : "random");
                }
                if (localAdmin.CompanyId == Guid.Empty || localAdmin.CompanyId == default(Guid))
                {
                    localAdmin.CompanyId = defaultCompanyId;
                }
                await context.SaveChangesAsync();
                return;
            }

            var admin = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == AdminUsername);

            if (admin != null)
            {
                if (admin.DeletedAt != null)
                {
                    admin.DeletedAt = null;
                    admin.IsActive = true;
                }
                if (string.IsNullOrWhiteSpace(admin.PasswordHash))
                {
                    var newPwd = isTesting ? TestAdminPassword : sanitizer.GenerateRandomPassword();
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd);
                    logger.LogWarning("[ENSURE ADMIN] 🔐 Set password for existing admin (was empty): {Kind}", isTesting ? "admin123 (Testing)" : "random");
                }
                if (admin.CompanyId == Guid.Empty || admin.CompanyId == default(Guid))
                {
                    admin.CompanyId = defaultCompanyId;
                }

                // Empresa: validar vía Admin API (Product no conoce BD)
                var adminClient = services.GetService<IAdminApiClient>();
                if (adminClient != null)
                {
                    var company = await adminClient.GetCompanyAsync(admin.CompanyId);
                    if (company == null)
                    {
                        logger.LogError("El usuario 'admin' tiene CompanyId {CompanyId} pero la empresa no existe en Admin API.", admin.CompanyId);
                        throw new InvalidOperationException("El usuario 'admin' está referenciando una empresa inexistente. Ejecute seeds de Admin.");
                    }
                }

                await context.SaveChangesAsync();
                return;
            }

            // Admin no existe: no crear Company ni User en código. Solo seeds deben crearlos.
            logger.LogError("El usuario 'admin' no existe en la BD. Debe estar definido en los seeds (demo-data.json o test-data.json).");
            throw new InvalidOperationException(
                "El usuario 'admin' debe estar definido en los seeds (demo-data.json o test-data.json). Toda la carga masiva de empresas y usuarios se realiza mediante seeds.");
        }

        // Pomelo MySQL suele habilitar estrategia de reintentos que requiere transacciones dentro de ExecutionStrategy.
        if (context.Database.IsRelational())
        {
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await context.Database.BeginTransactionAsync();
                await EnsureCoreAsync();
                await tx.CommitAsync();
            });
            return;
        }

        await EnsureCoreAsync();
    }

}
