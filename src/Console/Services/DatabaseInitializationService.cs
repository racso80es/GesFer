using GesFer.Product.Back.src.Infrastructure.Data;
using GesFer.Product.Back.src.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para ejecutar el punto 8 de la opción 1: Aplicar migraciones y cargar datos iniciales
/// </summary>
public class DatabaseInitializationService
{
    private readonly DockerService _dockerService;
    private readonly LogService _logService;

    public DatabaseInitializationService(DockerService dockerService, LogService logService)
    {
        _dockerService = dockerService;
        _logService = logService;
    }

    /// <summary>
    /// Resultado de la inicialización de base de datos
    /// </summary>
    public class InitializationResult
    {
        /// <summary>
        /// Estado del resultado: "ok" o "ko"
        /// </summary>
        public string Status { get; set; } = "ko";

        /// <summary>
        /// Lista de información relevante sobre el proceso
        /// </summary>
        public List<string> Information { get; set; } = new List<string>();

        /// <summary>
        /// Lista de errores si los hay
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Mensaje resumen
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Ejecuta el punto 8 de la opción 1: Aplicar migraciones y cargar datos iniciales
    /// Asegura que cada ejecución empiece de 0 eliminando la base de datos, creando estructura y añadiendo datos
    /// </summary>
    public async Task<InitializationResult> ExecuteStep8Async()
    {
        var result = new InitializationResult();
        result.Information.Add("Inicialización de base de datos");
        _logService.WriteLog("========================================");
        _logService.WriteLog("Ejecutando inicialización de base de datos");
        _logService.WriteLog("========================================");

        try
        {
            // Se asume que Docker y MySQL están correctamente configurados y funcionando
            // Paso 1: Configurar servicios y contexto de base de datos
            // (No se muestra en la información, solo se ejecuta)
            _logService.WriteLog("Configurando servicios...");
            
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            var apiPath = Path.Combine(rootPath, "src", "Product", "Back", "src", "Api");

            if (!Directory.Exists(apiPath))
            {
                result.Status = "ko";
                result.Errors.Add($"No se encontró la ruta de la API: {apiPath}");
                result.Message = "Ruta de API no encontrada";
                _logService.WriteError($"Ruta de API no encontrada: {apiPath}");
                return result;
            }

            // Configurar servicios igual que la API
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 0)),
                    mysqlOptions =>
                    {
                        mysqlOptions.EnableStringComparisonTranslations();
                        mysqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });

            // Configurar HostEnvironment como Development
            services.AddSingleton<IHostEnvironment>(new DevelopmentHostEnvironment());

            services.AddLogging(builder =>
            {
                // Configurar logging sin mostrar etiquetas "info:" en consola
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = null;
                    options.UseUtcTimestamp = false;
                    options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                });
                // Filtrar los mensajes de Entity Framework y otros que muestran "info:"
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.Warning);
                // Filtrar el warning esperado de DbInitializer sobre conexión a BD (es normal cuando la BD no existe aún)
                builder.AddFilter((category, level) =>
                {
                    // Si es DbInitializer, solo mostrar errores
                    if (category == "DbInitializer")
                    {
                        return level >= LogLevel.Error;
                    }
                    // Para otros, seguir el nivel mínimo
                    return level >= LogLevel.Warning;
                });
                builder.SetMinimumLevel(LogLevel.Warning); // Solo mostrar warnings y errores en consola
            });

            services.AddScoped<JsonDataSeeder>();
            services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();

            var serviceProvider = services.BuildServiceProvider();
            
            using (serviceProvider as IDisposable)
            {
                var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<ApplicationDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<DatabaseInitializationService>>();

                // Paso 2: Eliminar base de datos para empezar de 0
                // (No se muestra en la información, solo se ejecuta)
                _logService.WriteLog("Eliminando base de datos para empezar de 0...");
                
                try
                {
                    // Verificar si podemos conectar
                    if (await context.Database.CanConnectAsync())
                    {
                        logger.LogInformation("Eliminando base de datos completamente...");
                        await context.Database.EnsureDeletedAsync();
                        logger.LogInformation("Base de datos eliminada completamente");
                    }
                    else
                    {
                        logger.LogInformation("La base de datos no existe o no se puede conectar, continuando...");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "No se pudo eliminar la base de datos, intentando eliminar manualmente...");
                    _logService.WriteError("Error al eliminar base de datos", ex);
                    
                    // Intentar eliminar manualmente usando SQL directo
                    try
                    {
                        // Conectar sin especificar base de datos
                        var connectionStringWithoutDb = connectionString.Replace("Database=ScrapDb;", "");
                        using var tempContext = new ApplicationDbContext(
                            new DbContextOptionsBuilder<ApplicationDbContext>()
                                .UseMySql(connectionStringWithoutDb, new MySqlServerVersion(new Version(8, 0, 0)))
                                .Options);
                        
                        await tempContext.Database.ExecuteSqlRawAsync("DROP DATABASE IF EXISTS ScrapDb;");
                        await tempContext.Database.ExecuteSqlRawAsync("CREATE DATABASE ScrapDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
                        logger.LogInformation("Base de datos recreada manualmente");
                    }
                    catch (Exception fallbackEx)
                    {
                        logger.LogWarning(fallbackEx, "No se pudo recrear la base de datos manualmente, continuando...");
                        // Continuar de todas formas, las migraciones pueden manejar esto
                    }
                }

                // Paso 3: Aplicar migraciones y cargar datos usando DbInitializer
                _logService.WriteLog("Aplicando migraciones y cargando datos iniciales...");
                
                try
                {
                    // Capturar información de migraciones antes de aplicar
                    var migrationsBefore = await context.Database.GetAppliedMigrationsAsync();
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    
                    // Usar DbInitializer para aplicar migraciones y seeding
                    // Forzamos isDevelopment=true para que siempre ejecute en la consola
                    await DbInitializer.InitializeAsync(serviceProvider, isDevelopment: true);
                    
                    // Capturar información de migraciones aplicadas
                    var migrationsAfter = await context.Database.GetAppliedMigrationsAsync();
                    var appliedMigrations = migrationsAfter.Except(migrationsBefore).ToList();
                    
                    if (appliedMigrations.Any())
                    {
                        result.Information.Add($"✓ Migraciones aplicadas: {string.Join(", ", appliedMigrations)}");
                    }
                    
                    _logService.WriteLog("Inicialización de base de datos completada usando DbInitializer");
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Error al inicializar base de datos: {ex.Message}";
                    result.Status = "ko";
                    result.Errors.Add(errorMsg);
                    result.Message = errorMsg;
                    _logService.WriteError(errorMsg, ex);
                    logger.LogError(ex, errorMsg);
                    return result;
                }

                // Paso 4: Verificar que las tablas se crearon correctamente y obtener información de seeds
                _logService.WriteLog("Verificando estructura de base de datos y datos cargados...");
                
                try
                {
                    if (await context.Database.CanConnectAsync())
                    {
                        // Obtener lista de tablas
                        var tableNames = new List<string>();
                        using var command = context.Database.GetDbConnection().CreateCommand();
                        command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'ScrapDb' ORDER BY table_name;";
                        context.Database.OpenConnection();
                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            tableNames.Add(reader.GetString(0));
                        }
                        context.Database.CloseConnection();
                        
                        result.Information.Add($"✓ Base de datos verificada: {tableNames.Count} tablas creadas");
                        result.Information.Add($"  Tablas: {string.Join(", ", tableNames)}");
                        _logService.WriteLog($"Base de datos verificada: {tableNames.Count} tablas");
                        
                        // Obtener información de seeds cargados
                        var seedInfo = new List<string>();
                        
                        var languageCount = await context.Languages.CountAsync();
                        if (languageCount > 0) seedInfo.Add($"  • {languageCount} Language(s)");
                        
                        var permissionCount = await context.Permissions.CountAsync();
                        if (permissionCount > 0) seedInfo.Add($"  • {permissionCount} Permission(s)");
                        
                        var groupCount = await context.Groups.CountAsync();
                        if (groupCount > 0) seedInfo.Add($"  • {groupCount} Group(s)");
                        
                        var groupPermissionCount = await context.GroupPermissions.CountAsync();
                        if (groupPermissionCount > 0) seedInfo.Add($"  • {groupPermissionCount} GroupPermission(s)");
                        
                        var adminUserCount = await context.AdminUsers.CountAsync();
                        if (adminUserCount > 0) seedInfo.Add($"  • {adminUserCount} AdminUser(s)");
                        
                        var companyCount = await context.Companies.CountAsync();
                        if (companyCount > 0) seedInfo.Add($"  • {companyCount} Company(ies)");
                        
                        var userCount = await context.Users.CountAsync();
                        if (userCount > 0) seedInfo.Add($"  • {userCount} User(s)");
                        
                        var userGroupCount = await context.UserGroups.CountAsync();
                        if (userGroupCount > 0) seedInfo.Add($"  • {userGroupCount} UserGroup(s)");
                        
                        var userPermissionCount = await context.UserPermissions.CountAsync();
                        if (userPermissionCount > 0) seedInfo.Add($"  • {userPermissionCount} UserPermission(s)");
                        
                        var supplierCount = await context.Suppliers.CountAsync();
                        if (supplierCount > 0) seedInfo.Add($"  • {supplierCount} Supplier(s)");
                        
                        var customerCount = await context.Customers.CountAsync();
                        if (customerCount > 0) seedInfo.Add($"  • {customerCount} Customer(s)");
                        
                        if (seedInfo.Any())
                        {
                            result.Information.Add($"✓ Seeds cargados:");
                            result.Information.AddRange(seedInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.WriteError("Error al verificar estructura y datos", ex);
                    // No es crítico, continuar
                }
            }

            // Si llegamos aquí, todo fue exitoso
            result.Status = "ok";
            result.Message = "Inicialización de base de datos completada exitosamente";
            _logService.WriteLog("========================================");
            _logService.WriteLog("Punto 8 completado exitosamente");
            _logService.WriteLog("========================================");

            return result;
        }
        catch (Exception ex)
        {
            result.Status = "ko";
            result.Errors.Add($"Error inesperado: {ex.Message}");
            result.Message = $"Error durante la inicialización: {ex.Message}";
            _logService.WriteError("Error inesperado durante el punto 8", ex);
            return result;
        }
    }

    /// <summary>
    /// Implementación simple de IHostEnvironment para Development
    /// </summary>
    private class DevelopmentHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "GesFer.Console";
        public string ContentRootPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory);
    }
}
