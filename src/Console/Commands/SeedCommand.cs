using System;
using System.IO;
using System.Threading.Tasks;
using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;
using GesFer.Infrastructure.Data;
using GesFer.Admin.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Infrastructure.Services;
using GesFer.Admin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Pomelo.EntityFrameworkCore.MySql;

namespace GesFer.ConsoleApp.Commands;

public class SeedCommand : ICommandHandler<SeedCommandInput, bool>
{
    private readonly LogService _logService;
    private readonly string _rootPath;

    public SeedCommand(LogService logService)
    {
        _logService = logService;
        _rootPath = _logService.GetRootPath();
    }

    public async Task<CommandResult<bool>> HandleAsync(SeedCommandInput command)
    {
        var result = new CommandResult<bool>();
        result.Data = false;

        var scopeName = command.Scope switch
        {
            SeedScope.Shared => "Shared",
            SeedScope.Admin => "Admin",
            SeedScope.Product => "Product",
            SeedScope.All => "Todos los ámbitos",
            _ => "Desconocido"
        };

        var levelName = command.Level switch
        {
            SeedLevel.Master => "maestros",
            SeedLevel.Demo => "de demostración",
            SeedLevel.Test => "de prueba",
            _ => "maestros"
        };

        result.AddLog($"Insertando datos {levelName} ({scopeName}) desde JSON...");
        _logService.WriteLog($"Iniciando seeding de datos {levelName} para ámbito {scopeName}");

        try
        {
            var serviceProvider = CreateServiceProvider();
            using (serviceProvider as IDisposable)
            {
                using var scope2 = serviceProvider.CreateScope();
                var seeder = scope2.ServiceProvider.GetRequiredService<JsonDataSeeder>();
                var adminSeeder = scope2.ServiceProvider.GetRequiredService<AdminJsonDataSeeder>();

                if (command.Scope == SeedScope.All)
                {
                    // Cargar todos los ámbitos en orden: Shared -> Admin -> Product
                    var sharedResult = await ExecuteSeedForScopeAsync(seeder, adminSeeder, SeedScope.Shared, command.Level);
                    var adminResult = await ExecuteSeedForScopeAsync(seeder, adminSeeder, SeedScope.Admin, command.Level);
                    var productResult = await ExecuteSeedForScopeAsync(seeder, adminSeeder, SeedScope.Product, command.Level);

                    var allLoaded = sharedResult || adminResult || productResult;
                    if (allLoaded)
                    {
                        result.AddLog($"    ✓ Datos {levelName} insertados correctamente para todos los ámbitos");
                        _logService.WriteLog($"Datos {levelName} insertados correctamente para todos los ámbitos");
                        result.Data = true;
                        result.Success = true;
                        result.Message = "Seeding completo exitoso.";
                    }
                    else
                    {
                        result.AddLog($"    ⚠ No se pudieron cargar los datos {levelName} para ningún ámbito");
                        _logService.WriteLog($"Advertencia: No se pudieron cargar los datos {levelName}");
                        result.Success = false;
                        result.Message = "Fallo en seeding (ningún dato cargado).";
                    }
                }
                else
                {
                    var success = await ExecuteSeedForScopeAsync(seeder, adminSeeder, command.Scope, command.Level);
                    if (success)
                    {
                        result.AddLog($"    ✓ Datos {levelName} ({scopeName}) insertados correctamente");
                        _logService.WriteLog($"Datos {levelName} ({scopeName}) insertados correctamente");
                        result.Data = true;
                        result.Success = true;
                        result.Message = "Seeding exitoso.";
                    }
                    else
                    {
                        result.AddLog($"    ⚠ No se pudieron cargar los datos {levelName} ({scopeName})");
                        _logService.WriteLog($"Advertencia: No se pudieron cargar los datos {levelName} ({scopeName})");
                        result.Success = false;
                        result.Message = "Fallo en seeding.";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error al insertar datos {levelName} ({scopeName}): {ex.Message}";
            result.AddLog($"    ⚠ {errorMsg}");
            result.Errors.Add(errorMsg);
            _logService.WriteError(errorMsg, ex);

            result.Success = false;
            result.Message = errorMsg;
            result.Data = false;
        }

        return result;
    }

    private async Task<bool> ExecuteSeedForScopeAsync(JsonDataSeeder seeder, AdminJsonDataSeeder adminSeeder, SeedScope scope, SeedLevel level)
    {
        if (scope == SeedScope.Admin)
        {
            if (level == SeedLevel.Master || level == SeedLevel.Demo)
            {
                return (await adminSeeder.SeedAdminUsersAsync()).Loaded;
            }
            return false;
        }
        else if (scope == SeedScope.Shared || scope == SeedScope.Product)
        {
            return level switch
            {
                SeedLevel.Master => (await seeder.SeedMasterDataAsync()).Loaded,
                SeedLevel.Demo => (await seeder.SeedDemoDataAsync()).Loaded,
                SeedLevel.Test => await Task.Run(async () => { await seeder.SeedTestDataAsync(); return true; }),
                _ => false
            };
        }

        return false;
    }

    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Configuración - Usar Product/Back/Api para obtener connection string compartida
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(_rootPath, "src", "Product", "Back", "Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

        // DbContext Product
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

        // DbContext Admin
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
                });
        });

        // Servicios necesarios
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        // Servicios de infraestructura
        services.AddScoped<JsonDataSeeder>();
        services.AddScoped<AdminJsonDataSeeder>();
        services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();
        services.AddSingleton<ISensitiveDataSanitizer, SensitiveDataSanitizer>();

        var apiPath = Path.Combine(_rootPath, "src", "Product", "Back", "Api");
        services.AddSingleton<IHostEnvironment>(new DevelopmentHostEnvironment(apiPath));

        return services.BuildServiceProvider();
    }

    private class DevelopmentHostEnvironment : IHostEnvironment
    {
        public DevelopmentHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
        }

        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "GesFer.Console";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
