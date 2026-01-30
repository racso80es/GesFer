using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCompany.SysAdmin.Infrastructure.Data;
using Pomelo.EntityFrameworkCore.MySql;

namespace GesFer.ConsoleApp.Commands;

public class InitializeDatabaseCommand : ICommandHandler<InitializeDatabaseInput, InitializationResultData>
{
    private readonly LogService _logService;

    public InitializeDatabaseCommand(LogService logService)
    {
        _logService = logService;
    }

    public async Task<CommandResult<InitializationResultData>> HandleAsync(InitializeDatabaseInput input)
    {
        var result = new CommandResult<InitializationResultData>();
        result.Data = new InitializationResultData();

        // Add "Inicialización de base de datos" to info/logs?
        result.Data.Information.Add("Inicialización de base de datos");
        // result.AddLog("Inicialización de base de datos"); // Calling code usually prints title.

        _logService.WriteLog("========================================");
        _logService.WriteLog("Ejecutando inicialización de base de datos");
        _logService.WriteLog("========================================");

        try
        {
            _logService.WriteLog("Configurando servicios...");
            
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Ir 5 niveles arriba para llegar a la raíz del repositorio
            var rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
            var apiPath = Path.Combine(rootPath, "src", "Product", "Back", "Api");

            if (!Directory.Exists(apiPath))
            {
                result.Data.Status = "ko";
                var errorMsg = $"No se encontró la ruta de la API: {apiPath}";
                result.Data.Errors.Add(errorMsg);
                result.Data.Message = "Ruta de API no encontrada";

                result.AddLog($"ERROR: {errorMsg}");
                result.Errors.Add(errorMsg);
                _logService.WriteError(errorMsg);

                result.Success = false;
                result.Message = result.Data.Message;
                return result;
            }

            // Configurar servicios
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

            services.AddDbContext<AdminDbContext>(options =>
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
                        mysqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Admin");
                    });
            });

            services.AddSingleton<IHostEnvironment>(new DevelopmentHostEnvironment());

            services.AddLogging(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = null;
                    options.UseUtcTimestamp = false;
                    options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                });
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.Warning);
                builder.AddFilter((category, level) =>
                {
                    if (category == "DbInitializer")
                    {
                        return level >= LogLevel.Error;
                    }
                    return level >= LogLevel.Warning;
                });
                builder.SetMinimumLevel(LogLevel.Warning);
            });

            services.AddScoped<JsonDataSeeder>();
            services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();

            var serviceProvider = services.BuildServiceProvider();
            
            using (serviceProvider as IDisposable)
            {
                var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<ApplicationDbContext>();
                var adminContext = scopedServices.GetRequiredService<AdminDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<InitializeDatabaseCommand>>();

                _logService.WriteLog("Eliminando base de datos para empezar de 0...");
                
                try
                {
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
                    
                    try
                    {
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
                    }
                }

                _logService.WriteLog("Aplicando migraciones y cargando datos iniciales...");
                
                try
                {
                    var migrationsBefore = await context.Database.GetAppliedMigrationsAsync();
                    
                    await DbInitializer.InitializeAsync(serviceProvider, isDevelopment: true);
                    
                    _logService.WriteLog("Aplicando migraciones de Admin...");
                    await adminContext.Database.MigrateAsync();

                    var migrationsAfter = await context.Database.GetAppliedMigrationsAsync();
                    var appliedMigrations = migrationsAfter.Except(migrationsBefore).ToList();
                    
                    if (appliedMigrations.Any())
                    {
                        var info = $"✓ Migraciones Product aplicadas: {string.Join(", ", appliedMigrations)}";
                        result.Data.Information.Add(info);
                        // result.AddLog(info); // Let caller decide via Data.Information
                    }
                    
                    var adminMigrations = await adminContext.Database.GetAppliedMigrationsAsync();
                    if (adminMigrations.Any())
                    {
                        var info = $"✓ Migraciones Admin aplicadas: {string.Join(", ", adminMigrations)}";
                        result.Data.Information.Add(info);
                    }

                    _logService.WriteLog("Inicialización de base de datos completada (Product + Admin)");
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Error al inicializar base de datos: {ex.Message}";
                    result.Data.Status = "ko";
                    result.Data.Errors.Add(errorMsg);
                    result.Data.Message = errorMsg;
                    _logService.WriteError(errorMsg, ex);
                    logger.LogError(ex, errorMsg);

                    result.AddLog($"ERROR: {errorMsg}");
                    result.Errors.Add(errorMsg);

                    result.Success = false;
                    result.Message = errorMsg;
                    return result;
                }

                _logService.WriteLog("Verificando estructura de base de datos y datos cargados...");
                
                try
                {
                    if (await context.Database.CanConnectAsync())
                    {
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
                        
                        result.Data.Information.Add($"✓ Base de datos verificada: {tableNames.Count} tablas creadas");
                        result.Data.Information.Add($"  Tablas: {string.Join(", ", tableNames)}");
                        _logService.WriteLog($"Base de datos verificada: {tableNames.Count} tablas");
                        
                        var seedInfo = new List<string>();
                        
                        var languageCount = await context.Languages.CountAsync();
                        if (languageCount > 0) seedInfo.Add($"  • {languageCount} Language(s)");
                        
                        var permissionCount = await context.Permissions.CountAsync();
                        if (permissionCount > 0) seedInfo.Add($"  • {permissionCount} Permission(s)");
                        
                        var groupCount = await context.Groups.CountAsync();
                        if (groupCount > 0) seedInfo.Add($"  • {groupCount} Group(s)");
                        
                        var groupPermissionCount = await context.GroupPermissions.CountAsync();
                        if (groupPermissionCount > 0) seedInfo.Add($"  • {groupPermissionCount} GroupPermission(s)");
                        
                        var adminUserCount = await adminContext.AdminUsers.CountAsync();
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
                            result.Data.Information.Add($"✓ Seeds cargados:");
                            result.Data.Information.AddRange(seedInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.WriteError("Error al verificar estructura y datos", ex);
                }
            }

            result.Data.Status = "ok";
            result.Data.Message = "Inicialización de base de datos completada exitosamente";
            _logService.WriteLog("========================================");
            _logService.WriteLog("Punto 8 completado exitosamente");
            _logService.WriteLog("========================================");

            result.Success = true;
            result.Message = result.Data.Message;
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error inesperado: {ex.Message}";
            result.Data.Status = "ko";
            result.Data.Errors.Add(errorMsg);
            result.Data.Message = $"Error durante la inicialización: {ex.Message}";
            _logService.WriteError("Error inesperado durante el punto 8", ex);

            result.AddLog($"ERROR: {errorMsg}");
            result.Errors.Add(errorMsg);
            result.Success = false;
            result.Message = errorMsg;
            return result;
        }
    }

    private class DevelopmentHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "GesFer.Console";
        public string ContentRootPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory);
    }
}
