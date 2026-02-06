using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
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

            // SMOKE TEST: Verificación de Integridad de Acceso (ignorar filtros por si estaba soft-deleted)
            var adminUser = await context.Users
                .IgnoreQueryFilters()
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Username == "admin");
            
            // KAIZEN: Verificación adicional de integridad referencial
            if (adminUser == null)
            {
                var errorMessage = "🔥 FALLO CRÍTICO: Usuario 'admin' existe pero no se pudo cargar. Estado inconsistente detectado.";
                logger.LogError(errorMessage);
                Console.WriteLine($"    ❌ {errorMessage}");
                throw new Exception(errorMessage);
            }
            
            // KAIZEN: Verificar que el admin tenga CompanyId vinculado
            if (adminUser.CompanyId == Guid.Empty || adminUser.CompanyId == default(Guid))
            {
                var errorMessage = $"🔥 FALLO CRÍTICO DE INTEGRIDAD REFERENCIAL: El usuario 'admin' no tiene CompanyId vinculado (CompanyId: {adminUser.CompanyId}). El sistema sería inaccesible. Revise la vinculación en demo-data.json.";
                logger.LogError(errorMessage);
                Console.WriteLine($"    ❌ {errorMessage}");
                throw new Exception(errorMessage);
            }
            
            // KAIZEN: Verificar que la empresa vinculada existe
            if (adminUser.Company == null)
            {
                var errorMessage = $"🔥 FALLO CRÍTICO DE INTEGRIDAD REFERENCIAL: El usuario 'admin' tiene CompanyId ({adminUser.CompanyId}) pero la empresa no existe en la base de datos. Revise la creación de empresas en demo-data.json.";
                logger.LogError(errorMessage);
                Console.WriteLine($"    ❌ {errorMessage}");
                throw new Exception(errorMessage);
            }
            
            // KAIZEN: Verificar que la empresa vinculada es "Empresa Admin" con el GUID correcto
            const string EXPECTED_ADMIN_COMPANY_NAME = "Empresa Admin";
            const string EXPECTED_ADMIN_COMPANY_ID = "550e8400-e29b-41d4-a716-446655440000";
            
            if (adminUser.Company.Name != EXPECTED_ADMIN_COMPANY_NAME)
            {
                var warningMessage = $"⚠️ ADVERTENCIA: El usuario 'admin' está vinculado a '{adminUser.Company.Name}' en lugar de '{EXPECTED_ADMIN_COMPANY_NAME}'. Esto puede causar problemas de autenticación.";
                logger.LogWarning(warningMessage);
                Console.WriteLine($"    ⚠ {warningMessage}");
            }
            
            if (adminUser.CompanyId.ToString() != EXPECTED_ADMIN_COMPANY_ID)
            {
                var warningMessage = $"⚠️ ADVERTENCIA: El usuario 'admin' tiene CompanyId '{adminUser.CompanyId}' en lugar del esperado '{EXPECTED_ADMIN_COMPANY_ID}'. Verifique la sincronización en demo-data.json.";
                logger.LogWarning(warningMessage);
                Console.WriteLine($"    ⚠ {warningMessage}");
            }
            
            var companyInfo = $" (Empresa: {adminUser.Company.Name}, CompanyId: {adminUser.CompanyId})";
            logger.LogInformation("✅ Smoke Test Superado: Usuario 'admin' verificado correctamente{CompanyInfo}", companyInfo);
            Console.WriteLine($"    ✅ Smoke Test Superado: Usuario 'admin' verificado{companyInfo}");

            logger.LogInformation("=== Inicialización de base de datos completada exitosamente ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico durante la inicialización de la base de datos");
            throw;
        }
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
                    logger.LogInformation("Migraciones aplicadas correctamente");
                    Console.WriteLine($"    Migraciones aplicadas: {string.Join(", ", pendingMigrationsList)}");
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
                            logger.LogInformation("Las migraciones ya están aplicadas. La base de datos está actualizada.");
                            Console.WriteLine($"    Migraciones ya aplicadas: {string.Join(", ", appliedMigrationsList)}");
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
                Console.WriteLine("    Migraciones: ninguna pendiente");
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
    /// Carga datos iniciales desde archivos JSON de forma idempotente.
    /// </summary>
    private static async Task SeedDataFromJsonAsync(
        ApplicationDbContext context,
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            var seeder = services.GetRequiredService<JsonDataSeeder>();
            var environment = services.GetRequiredService<IHostEnvironment>();
            var isTesting = environment.EnvironmentName == "Testing";

            if (isTesting)
            {
                // En modo Testing, cargar solo test-data.json
                logger.LogInformation("Modo Testing detectado: cargando test-data.json");
                await seeder.SeedTestDataAsync();
                logger.LogInformation("test-data.json cargado correctamente");
            }
            else
            {
                // En modo Development, cargar master-data.json y demo-data.json
                // Cargar datos maestros y obtener resumen de entidades
                var masterDataResult = await seeder.SeedMasterDataAsync();
                
                // Cargar datos de demostración y obtener resumen de entidades
                var demoDataResult = await seeder.SeedDemoDataAsync();

                // Mostrar resumen conciso de entidades cargadas
                if (masterDataResult.Loaded || demoDataResult.Loaded)
                {
                    var entities = new List<string>();
                    
                    if (masterDataResult.Loaded && masterDataResult.Entities.Any())
                    {
                        entities.AddRange(masterDataResult.Entities);
                    }
                    
                    if (demoDataResult.Loaded && demoDataResult.Entities.Any())
                    {
                        entities.AddRange(demoDataResult.Entities);
                    }

                    if (entities.Any())
                    {
                        Console.WriteLine($"    Seeds cargados: {string.Join(", ", entities)}");
                    }
                }

                // Registrar en log para debugging
                if (masterDataResult.Loaded && demoDataResult.Loaded)
                {
                    logger.LogInformation("Todos los datos iniciales han sido cargados correctamente");
                }
                else
                {
                    logger.LogWarning("Algunos datos iniciales no se pudieron cargar. master-data.json: {MasterLoaded}, demo-data.json: {DemoLoaded}",
                        masterDataResult.Loaded, demoDataResult.Loaded);
                }
            }
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
        // Reglas:
        // - Usar IgnoreQueryFilters para detectar admin aunque esté soft-deleted.
        // - Si existe: reactivar y asegurar que tenga CompanyId y Company válida.
        // - Si no existe: crear Company base (si hace falta) + crear admin.

        var sanitizer = services.GetRequiredService<ISensitiveDataSanitizer>();

        const string AdminUsername = "admin";
        // const string AdminPassword = "admin123"; // REMOVED SECURITY RISK
        // const string FixedAdminHash = ...; // REMOVED SECURITY RISK

        // Seeds de Testing (test-data.json) usan estos IDs para admin/empresa demo
        var defaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111115");
        var defaultAdminUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var defaultLanguageId = Guid.Parse("10000000-0000-0000-0000-000000000001");

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
                    var newPwd = sanitizer.GenerateRandomPassword();
                    localAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd);
                    logger.LogWarning("[ENSURE ADMIN] 🔐 Set RANDOM password for existing local admin: {Password}", newPwd);
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
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Username == AdminUsername);

            if (admin != null)
            {
                // Reactivar si estaba borrado lógicamente
                if (admin.DeletedAt != null)
                {
                    admin.DeletedAt = null;
                    admin.IsActive = true;
                }

                // Blindaje mínimo: password hash no vacío (y compatible con login)
                if (string.IsNullOrWhiteSpace(admin.PasswordHash))
                {
                    var newPwd = sanitizer.GenerateRandomPassword();
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd);
                    logger.LogWarning("[ENSURE ADMIN] 🔐 Set RANDOM password for existing admin (was empty): {Password}", newPwd);
                }

                // Si CompanyId es inválido, forzar a la empresa demo
                if (admin.CompanyId == Guid.Empty || admin.CompanyId == default(Guid))
                {
                    admin.CompanyId = defaultCompanyId;
                }

                // Asegurar que Company existe (si no, crear fallback)
                if (admin.Company == null)
                {
                    var existingCompany = await context.Companies
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(c => c.Id == admin.CompanyId);

                    if (existingCompany == null)
                    {
                        existingCompany = new Company
                        {
                            Id = admin.CompanyId,
                            Name = "Empresa Demo",
                            Address = "Calle Gran Vía, 1",
                            Phone = "912345678",
                            Email = Email.Create("demo@empresa.com"),
                            LanguageId = defaultLanguageId,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        try
                        {
                            existingCompany.TaxId = TaxId.Create("B87654321");
                        }
                        catch (ArgumentException ex)
                        {
                            // Si el TaxId falla por regla de dominio, mantener null y continuar.
                            logger.LogWarning(ex, "[SEED] TaxId fallback inválido para Company demo. Se continuará sin TaxId.");
                        }

                        context.Companies.Add(existingCompany);
                    }
                }

                await context.SaveChangesAsync();
                return;
            }

            // No existe: crear Company (si hace falta) + crear admin
            var company = await context.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == defaultCompanyId);

            if (company == null)
            {
                company = new Company
                {
                    Id = defaultCompanyId,
                    Name = "Empresa Demo",
                    Address = "Calle Gran Vía, 1",
                    Phone = "912345678",
                    Email = Email.Create("demo@empresa.com"),
                    LanguageId = defaultLanguageId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                try
                {
                    company.TaxId = TaxId.Create("B87654321");
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "[SEED] TaxId fallback inválido para Company demo. Se continuará sin TaxId.");
                }

                context.Companies.Add(company);
            }
            else if (company.DeletedAt != null)
            {
                company.DeletedAt = null;
                company.IsActive = true;
            }

            var generatedPassword = sanitizer.GenerateRandomPassword();
            var newAdmin = new User
            {
                Id = defaultAdminUserId,
                CompanyId = defaultCompanyId,
                Username = AdminUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword),
                FirstName = "Administrador",
                LastName = "Sistema",
                Email = Email.Create("admin@empresa.com"),
                Phone = "912345678",
                LanguageId = defaultLanguageId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Users.Add(newAdmin);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Admin garantizado: '{Username}' creado con contraseña aleatoria", AdminUsername);
            Console.WriteLine($"    ✅ Admin garantizado: '{AdminUsername}' creado. Clave: '{generatedPassword}'");
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
