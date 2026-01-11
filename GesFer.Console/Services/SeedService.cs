using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para ejecutar seeds de datos desde archivos JSON
/// Utiliza el sistema profesionalizado de DbInitializer y JsonDataSeeder
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
    /// Crea un ServiceProvider configurado para la consola
    /// </summary>
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Configuración
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(_rootPath, "Api", "src", "Api"))
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
    /// Ejecuta el seeding de datos maestros desde JSON
    /// </summary>
    public async Task<bool> ExecuteMasterDataAsync()
    {
        Console.WriteLine("Insertando datos maestros desde JSON...");
        _logService.WriteLog("Iniciando seeding de datos maestros desde JSON");

        try
        {
            var serviceProvider = CreateServiceProvider();
            using (serviceProvider as IDisposable)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var seeder = scope.ServiceProvider.GetRequiredService<JsonDataSeeder>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

                var masterDataResult = await seeder.SeedMasterDataAsync();
                
                if (masterDataResult.Loaded)
                {
                    Console.WriteLine("    ✓ Datos maestros insertados correctamente");
                    _logService.WriteLog("Datos maestros insertados correctamente desde JSON");
                    return true;
                }
                else
                {
                    Console.WriteLine("    ⚠ No se pudieron cargar los datos maestros (archivo no encontrado)");
                    _logService.WriteLog("Advertencia: No se pudieron cargar los datos maestros");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error al insertar datos maestros: {ex.Message}";
            Console.WriteLine($"    ⚠ {errorMsg}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Ejecuta el seeding de datos de muestra desde JSON
    /// </summary>
    public async Task<bool> ExecuteSampleDataAsync()
    {
        Console.WriteLine("Insertando datos de muestra desde JSON...");
        _logService.WriteLog("Iniciando seeding de datos de muestra desde JSON");

        try
        {
            var serviceProvider = CreateServiceProvider();
            using (serviceProvider as IDisposable)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var seeder = scope.ServiceProvider.GetRequiredService<JsonDataSeeder>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

                var demoDataResult = await seeder.SeedDemoDataAsync();
                
                if (demoDataResult.Loaded)
                {
                    Console.WriteLine("    ✓ Datos de muestra insertados correctamente");
                    _logService.WriteLog("Datos de muestra insertados correctamente desde JSON");
                    return true;
                }
                else
                {
                    Console.WriteLine("    ⚠ No se pudieron cargar los datos de muestra (archivo no encontrado)");
                    _logService.WriteLog("Advertencia: No se pudieron cargar los datos de muestra");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error al insertar datos de muestra: {ex.Message}";
            Console.WriteLine($"    ⚠ {errorMsg}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Ejecuta el seeding de datos de prueba desde JSON
    /// </summary>
    public async Task<bool> ExecuteTestDataAsync()
    {
        Console.WriteLine("Insertando datos de prueba desde JSON...");
        _logService.WriteLog("Iniciando seeding de datos de prueba desde JSON");

        try
        {
            var serviceProvider = CreateServiceProvider();
            using (serviceProvider as IDisposable)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var seeder = scope.ServiceProvider.GetRequiredService<JsonDataSeeder>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

                await seeder.SeedTestDataAsync();
                
                Console.WriteLine("    ✓ Datos de prueba insertados correctamente");
                _logService.WriteLog("Datos de prueba insertados correctamente desde JSON");
                return true;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error al insertar datos de prueba: {ex.Message}";
            Console.WriteLine($"    ⚠ {errorMsg}");
            _logService.WriteError(errorMsg, ex);
            return false;
        }
    }

    /// <summary>
    /// Ejecuta todos los seeds desde JSON en orden
    /// Utiliza el mismo sistema que DbInitializer para mantener consistencia
    /// </summary>
    public async Task<bool> ExecuteAllSeedsAsync()
    {
        Console.WriteLine("Insertando todos los datos iniciales desde JSON...");
        _logService.WriteLog("Iniciando seeding completo desde archivos JSON");

        try
        {
            var serviceProvider = CreateServiceProvider();
            using (serviceProvider as IDisposable)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var seeder = scope.ServiceProvider.GetRequiredService<JsonDataSeeder>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

                // Paso 1: Datos maestros
                Console.WriteLine("    Cargando datos maestros...");
                var masterDataResult = await seeder.SeedMasterDataAsync();
                if (masterDataResult.Loaded)
                {
                    Console.WriteLine("    ✓ Datos maestros cargados");
                }
                else
                {
                    Console.WriteLine("    ⚠ No se pudieron cargar los datos maestros");
                }
                await Task.Delay(500);

                // Paso 2: Datos de demostración
                Console.WriteLine("    Cargando datos de demostración...");
                var demoDataResult = await seeder.SeedDemoDataAsync();
                if (demoDataResult.Loaded)
                {
                    Console.WriteLine("    ✓ Datos de demostración cargados");
                }
                else
                {
                    Console.WriteLine("    ⚠ No se pudieron cargar los datos de demostración");
                }
                await Task.Delay(500);

                // Nota: AdminUser ahora se carga desde master-data.json mediante JsonDataSeeder
                // No es necesario crearlo manualmente aquí

                Console.WriteLine("    ✓ Todos los datos iniciales insertados correctamente");
                _logService.WriteLog("Seeding completo desde JSON ejecutado correctamente");
                return true;
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
