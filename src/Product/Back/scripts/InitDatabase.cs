using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InitDatabase;

class Program
{
    static async Task Main(string[] args)
    {
        // Obtener la ruta correcta al directorio de la API Product
        var currentDir = Directory.GetCurrentDirectory();
        var apiPath = Path.Combine(currentDir, "..", "src", "Api");
        if (!Directory.Exists(apiPath))
        {
            // Si no existe, intentar desde la raíz del proyecto
            apiPath = Path.Combine(currentDir, "..", "..", "..", "src", "Product", "Back", "src", "Api");
        }
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

var services = new ServiceCollection();
services.AddLogging(builder => 
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning); // Solo mostrar warnings y errores por defecto
    builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None); // Ocultar comandos SQL
    builder.AddFilter("InitDatabase.Program", LogLevel.Information); // Permitir mensajes de información del programa
});

var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERROR: No se encontró la cadena de conexión");
    Environment.Exit(1);
}

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var serviceProvider = services.BuildServiceProvider();
var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    if (await context.Database.CanConnectAsync())
    {
        // Eliminar tabla de migraciones si existe
        try
        {
            var sql = "DROP TABLE IF EXISTS __EFMigrationsHistory;";
            await context.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo eliminar la tabla de migraciones");
        }
        
        // Eliminar todas las tablas existentes para empezar limpio (en orden inverso de dependencias)
        var tablesToDrop = new[] { 
            "SalesDeliveryNoteLines", "SalesDeliveryNotes", "SalesInvoices",
            "PurchaseDeliveryNoteLines", "PurchaseDeliveryNotes", "PurchaseInvoices",
            "TariffItems", "Articles", "Suppliers", "Customers",
            "UserGroups", "UserPermissions", "GroupPermissions", 
            "Users", "Groups", "Permissions", "ArticleFamilies", "Tariffs", "Companies",
            "PostalCodes", "Cities", "States", "Countries", "Languages"
        };
        foreach (var tableName in tablesToDrop)
        {
            try
            {
            // Los nombres de tablas son constantes hardcodeadas, por lo que son seguros
            // Usar ExecuteSqlRaw directamente ya que no hay riesgo de SQL injection
#pragma warning disable EF1002 // El nombre de la tabla es una constante, no entrada del usuario
                await context.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS `{tableName}`;");
#pragma warning restore EF1002
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo eliminar la tabla {TableName}, continuando...", tableName);
            }
        }
        
        // Aplicar migraciones para crear tablas
        // NO usar EnsureCreated - siempre usar migraciones
        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("    ✓ Migraciones aplicadas correctamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al aplicar migraciones: {Message}", ex.Message);
            throw;
        }
    }
    else
    {
        logger.LogError("No se puede conectar a la base de datos");
        Environment.Exit(1);
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Error al crear tablas: {Message}", ex.Message);
    Environment.Exit(1);
}
    }
}
