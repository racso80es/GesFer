using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Extensions;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace GesFer.Infrastructure.SeedRunner;

class Program
{
    static async Task Main(string[] args)
    {
        // Determinar qué datos insertar según los argumentos
        // --all o --full: inserta todos los datos (maestros + demo + test)
        // Sin argumentos: solo datos maestros
        var insertAll = args.Contains("--all", StringComparer.OrdinalIgnoreCase) || 
                        args.Contains("--full", StringComparer.OrdinalIgnoreCase) ||
                        args.Contains("--complete", StringComparer.OrdinalIgnoreCase);

        // Buscar appsettings.json en la carpeta de la API
        var currentDir = Directory.GetCurrentDirectory();
        var apiPath = Path.Combine(currentDir, "..", "..", "Api");
        if (!Directory.Exists(apiPath))
        {
            apiPath = Path.Combine(currentDir, "..", "Api");
        }
        if (!Directory.Exists(apiPath))
        {
            apiPath = currentDir; // Fallback al directorio actual
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        
        // Configurar logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Configurar DbContext
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

        // Registrar servicios
        services.AddScoped<JsonDataSeeder>();

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            if (insertAll)
            {
                logger.LogInformation("Iniciando seeding completo de datos (maestros + demo + test)...");
            }
            else
            {
                logger.LogInformation("Iniciando seeding de datos maestros...");
            }

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ejecutar seeding según el modo
            await context.SeedDataAsync(scope.ServiceProvider, includeTestData: insertAll);

            logger.LogInformation("Seeding completado exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al ejecutar seeding");
            Environment.Exit(1);
        }
    }
}

