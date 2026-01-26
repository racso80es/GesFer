using GesFer.Product.Back.src.Infrastructure.Data;
using GesFer.Product.Back.src.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para ejecutar seeds de datos desde archivos JSON
/// Utiliza la nueva taxonomía de Seeds organizada por ámbito (Shared, Admin, Product)
/// </summary>
public class SeedService
{
    private readonly string _rootPath;
    private readonly LogService _logService;

    public SeedService(LogService logService)
    {
        _logService = logService;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _rootPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
    }

    /// <summary>
    /// Enum para los ámbitos de seeding
    /// </summary>
    public enum SeedScope
    {
        Shared = 1,
        Admin = 2,
        Product = 3,
        All = 4
    }

    /// <summary>
    /// Enum para los niveles de seed
    /// </summary>
    public enum SeedLevel
    {
        Master = 1,
        Demo = 2,
        Test = 3
    }

    /// <summary>
    /// Crea un ServiceProvider configurado para la consola
    /// </summary>
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Configuración - Usar Product/Back/src/Api
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(_rootPath, "src", "Product", "Back", "src", "Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

        // DbContext
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

        // Servicios necesarios
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Servicios de infraestructura
        services.AddScoped<JsonDataSeeder>();
        services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Obtiene la ruta del archivo de seed según ámbito y nivel
    /// </summary>
    private string GetSeedFilePath(SeedScope scope, SeedLevel level)
    {
        var seedsBasePath = Path.Combine(_rootPath, "src", "Utils", "Data", "Seeds");
        var levelFolder = level switch
        {
            SeedLevel.Master => "master",
            SeedLevel.Demo => "demo",
            SeedLevel.Test => "test",
            _ => "master"
        };

        var fileName = scope switch
        {
            SeedScope.Shared => "master-data.json", // Por ahora usa el mismo formato
            SeedScope.Admin => $"admin-{levelFolder}-data.json",
            SeedScope.Product => $"product-{levelFolder}-data.json",
            _ => $"{levelFolder}-data.json"
        };

        // Si el archivo no existe en Utils, buscar en la ubicación legacy (Product/Back)
        var utilsPath = Path.Combine(seedsBasePath, levelFolder, fileName);
        if (File.Exists(utilsPath))
        {
            return utilsPath;
        }

        // Fallback a ubicación legacy
        var legacyPath = Path.Combine(_rootPath, "src", "Product", "Back", "src", "Infrastructure", "Data", "Seeds", fileName);
        if (File.Exists(legacyPath))
        {
            return legacyPath;
        }

        // Si es Shared y no existe archivo específico, usar master-data.json legacy
        if (scope == SeedScope.Shared)
        {
            var sharedLegacyPath = Path.Combine(_rootPath, "src", "Product", "Back", "src", "Infrastructure", "Data", "Seeds", $"{levelFolder}-data.json");
            if (File.Exists(sharedLegacyPath))
            {
                return sharedLegacyPath;
            }
        }

        return utilsPath; // Retornar la ruta esperada aunque no exista
    }

    /// <summary>
    /// Ejecuta el seeding según ámbito y nivel
    /// </summary>
    public async Task<bool> ExecuteSeedAsync(SeedScope scope, SeedLevel level)
    {
        var scopeName = scope switch
        {
            SeedScope.Shared => "Shared",
            SeedScope.Admin => "Admin",
            SeedScope.Product => "Product",
            SeedScope.All => "Todos los ámbitos",
            _ => "Desconocido"
        };

        var levelName = level switch
        {
            SeedLevel.Master => "maestros",
            SeedLevel.Demo => "de demostración",
            SeedLevel.Test => "de prueba",
            _ => "maestros"
        };

        Console.WriteLine($"Insertando datos {levelName} ({scopeName}) desde JSON...");
        _logService.WriteLog($"Iniciando seeding de datos {levelName} para ámbito {scopeName}");

        try
        {
            var serviceProvider = CreateServiceProvider();
            using (serviceProvider as IDisposable)
            {
                using var scope2 = serviceProvider.CreateScope();
                var context = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var seeder = scope2.ServiceProvider.GetRequiredService<JsonDataSeeder>();
                var logger = scope2.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

                if (scope == SeedScope.All)
                {
                    // Cargar todos los ámbitos en orden: Shared -> Admin -> Product
                    var sharedResult = await ExecuteSeedForScopeAsync(seeder, SeedScope.Shared, level);
                    var adminResult = await ExecuteSeedForScopeAsync(seeder, SeedScope.Admin, level);
                    var productResult = await ExecuteSeedForScopeAsync(seeder, SeedScope.Product, level);

                    var allLoaded = sharedResult || adminResult || productResult;
                    if (allLoaded)
                    {
                        Console.WriteLine($"    ✓ Datos {levelName} insertados correctamente para todos los ámbitos");
                        _logService.WriteLog($"Datos {levelName} insertados correctamente para todos los ámbitos");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"    ⚠ No se pudieron cargar los datos {levelName} para ningún ámbito");
                        _logService.WriteLog($"Advertencia: No se pudieron cargar los datos {levelName}");
                        return false;
                    }
                }
                else
                {
                    var result = await ExecuteSeedForScopeAsync(seeder, scope, level);
                    if (result)
                    {
                        Console.WriteLine($"    ✓ Datos {levelName} ({scopeName}) insertados correctamente");
                        _logService.WriteLog($"Datos {levelName} ({scopeName}) insertados correctamente");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"    ⚠ No se pudieron cargar los datos {levelName} ({scopeName})");
                        _logService.WriteLog($"Advertencia: No se pudieron cargar los datos {levelName} ({scopeName})");
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error al insertar datos {levelName} ({scopeName}): {ex.Message}";
            Console.WriteLine($"    ⚠ {errorMsg}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Ejecuta el seeding para un ámbito específico
    /// </summary>
    private async Task<bool> ExecuteSeedForScopeAsync(JsonDataSeeder seeder, SeedScope scope, SeedLevel level)
    {
        // Por ahora, usar los métodos existentes de JsonDataSeeder
        // TODO: Refactorizar JsonDataSeeder para soportar la nueva taxonomía
        return level switch
        {
            SeedLevel.Master => (await seeder.SeedMasterDataAsync()).Loaded,
            SeedLevel.Demo => (await seeder.SeedDemoDataAsync()).Loaded,
            SeedLevel.Test => await Task.Run(async () => { await seeder.SeedTestDataAsync(); return true; }),
            _ => false
        };
    }

    /// <summary>
    /// Ejecuta el seeding de datos maestros desde JSON (método legacy - mantiene compatibilidad)
    /// </summary>
    public async Task<bool> ExecuteMasterDataAsync()
    {
        return await ExecuteSeedAsync(SeedScope.All, SeedLevel.Master);
    }

    /// <summary>
    /// Ejecuta el seeding de datos de muestra desde JSON (método legacy - mantiene compatibilidad)
    /// </summary>
    public async Task<bool> ExecuteSampleDataAsync()
    {
        return await ExecuteSeedAsync(SeedScope.All, SeedLevel.Demo);
    }

    /// <summary>
    /// Ejecuta el seeding de datos de prueba desde JSON (método legacy - mantiene compatibilidad)
    /// </summary>
    public async Task<bool> ExecuteTestDataAsync()
    {
        return await ExecuteSeedAsync(SeedScope.All, SeedLevel.Test);
    }

    /// <summary>
    /// Ejecuta todos los seeds desde JSON en orden (método legacy - mantiene compatibilidad)
    /// </summary>
    public async Task<bool> ExecuteAllSeedsAsync()
    {
        Console.WriteLine("Insertando todos los datos iniciales desde JSON...");
        _logService.WriteLog("Iniciando seeding completo desde archivos JSON");

        try
        {
            // Ejecutar todos los niveles para todos los ámbitos
            var masterResult = await ExecuteSeedAsync(SeedScope.All, SeedLevel.Master);
            await Task.Delay(500);

            var demoResult = await ExecuteSeedAsync(SeedScope.All, SeedLevel.Demo);
            await Task.Delay(500);

            var testResult = await ExecuteSeedAsync(SeedScope.All, SeedLevel.Test);

            if (masterResult || demoResult || testResult)
            {
                Console.WriteLine("    ✓ Todos los datos iniciales insertados correctamente");
                _logService.WriteLog("Seeding completo desde JSON ejecutado correctamente");
                return true;
            }
            else
            {
                Console.WriteLine("    ⚠ No se pudieron cargar los datos iniciales");
                _logService.WriteLog("Advertencia: No se pudieron cargar los datos iniciales");
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error al insertar datos iniciales: {ex.Message}";
            Console.WriteLine($"    ⚠ {errorMsg}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }
}
