using FluentAssertions;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using SeedResult = GesFer.Infrastructure.Services.SeedResult;

namespace GesFer.IntegrationTests.Services;

/// <summary>
/// Tests para validar que JsonDataSeeder puede encontrar los archivos JSON de seeds
/// desde diferentes contextos de ejecución (API, Consola, etc.)
/// </summary>
public class JsonDataSeederTests
{
    /// <summary>
    /// Valida que JsonDataSeeder puede encontrar los archivos JSON de seeds
    /// cuando se ejecuta desde el contexto de la consola.
    /// 
    /// IMPORTANTE: Este test DEBE FALLAR si los archivos no se encuentran.
    /// Si el test pasa, significa que la consola puede encontrar los archivos JSON correctamente.
    /// </summary>
    [Fact]
    public async Task JsonDataSeeder_ShouldFindSeedsFiles_WhenExecutedFromConsole()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Configurar DbContext en memoria para el test
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}");
        });

        // Configurar logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Solo warnings y errores para el test
        });

        // Registrar JsonDataSeeder
        services.AddScoped<JsonDataSeeder>();
        services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        
        var seeder = scope.ServiceProvider.GetRequiredService<JsonDataSeeder>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        // Intentar cargar datos maestros (debe encontrar el archivo)
        var masterDataResult = await seeder.SeedMasterDataAsync();
        
        // Intentar cargar datos de demostración (debe encontrar el archivo)
        var demoDataResult = await seeder.SeedDemoDataAsync();

        // Assert
        // AMBOS archivos deben ser encontrados para que el test pase
        // Si alguno no se encuentra, el test debe fallar
        masterDataResult.Loaded.Should().BeTrue(
            $"master-data.json DEBE ser encontrado. " +
            $"Esto valida que JsonDataSeeder puede encontrar los archivos desde el contexto de ejecución actual. " +
            $"Si falla, significa que la lógica de búsqueda de archivos necesita ser corregida.");
        
        demoDataResult.Loaded.Should().BeTrue(
            $"demo-data.json DEBE ser encontrado. " +
            $"Esto valida que JsonDataSeeder puede encontrar los archivos desde el contexto de ejecución actual. " +
            $"Si falla, significa que la lógica de búsqueda de archivos necesita ser corregida.");
    }


    /// <summary>
    /// Valida que JsonDataSeeder puede encontrar los archivos incluso cuando se ejecuta
    /// desde un contexto diferente (simulando ejecución desde la consola)
    /// </summary>
    [Fact]
    public async Task JsonDataSeeder_ShouldNotThrow_WhenExecutedFromDifferentContext()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Configurar DbContext en memoria
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}");
        });

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddScoped<JsonDataSeeder>();
        services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        
        var seeder = scope.ServiceProvider.GetRequiredService<JsonDataSeeder>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act & Assert
        // No debe lanzar excepción incluso si los archivos no se encuentran
        // (simula el comportamiento cuando se ejecuta desde diferentes ubicaciones)
        Func<Task<SeedResult>> masterDataAction = async () => await seeder.SeedMasterDataAsync();
        Func<Task<SeedResult>> demoDataAction = async () => await seeder.SeedDemoDataAsync();

        // Los métodos deben ejecutarse sin lanzar excepciones
        await masterDataAction.Should().NotThrowAsync("SeedMasterDataAsync no debe lanzar excepciones");
        await demoDataAction.Should().NotThrowAsync("SeedDemoDataAsync no debe lanzar excepciones");
    }
}
