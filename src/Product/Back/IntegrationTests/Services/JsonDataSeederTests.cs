using FluentAssertions;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
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

    /// <summary>
    /// Test de seguridad: Valida que usuarios huérfanos (vinculados a empresas rechazadas)
    /// son descartados silenciosamente sin lanzar excepciones de Foreign Key.
    /// </summary>
    [Fact]
    public async Task Seed_OrphanUsers_AreSkipped()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Configurar DbContext en memoria
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName: $"TestDb_OrphanUsers_{Guid.NewGuid()}");
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

        // Crear Language necesario para la empresa
        var languageId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var language = new Language
        {
            Id = languageId,
            Name = "Español",
            Code = "es",
            Description = "Español",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Languages.Add(language);
        await context.SaveChangesAsync();

        // Crear JSON temporal con 1 Empresa Inválida y 1 Usuario vinculado
        var invalidCompanyId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var orphanUserId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        
        var testData = new
        {
            languages = new[]
            {
                new
                {
                    id = languageId.ToString(),
                    name = "Español",
                    code = "es",
                    description = "Español"
                }
            },
            companies = new[]
            {
                new
                {
                    id = invalidCompanyId.ToString(),
                    name = "Empresa Inválida",
                    taxId = "INVALIDO", // TaxId inválido que será rechazado
                    address = "Calle Test",
                    phone = "912345678",
                    email = "test@test.com",
                    languageId = languageId.ToString()
                }
            },
            users = new[]
            {
                new
                {
                    id = orphanUserId.ToString(),
                    companyId = invalidCompanyId.ToString(), // Vinculado a empresa inválida
                    username = "usuario_huérfano",
                    password = "admin123",
                    firstName = "Usuario",
                    lastName = "Huérfano",
                    email = "usuario@test.com",
                    phone = "912345678",
                    languageId = languageId.ToString()
                }
            }
        };

        // Crear archivo JSON en la ubicación esperada por JsonDataSeeder
        var basePath = AppContext.BaseDirectory;
        var seedsPath = Path.Combine(basePath, "Data", "Seeds");
        if (!Directory.Exists(seedsPath))
        {
            Directory.CreateDirectory(seedsPath);
        }
        var expectedFilePath = Path.Combine(seedsPath, "test-data.json");
        
        // Guardar el archivo original si existe
        string? originalContent = null;
        var originalExists = File.Exists(expectedFilePath);
        if (originalExists)
        {
            originalContent = await File.ReadAllTextAsync(expectedFilePath);
        }
        
        try
        {
            // Escribir el archivo de test
            var jsonContent = JsonSerializer.Serialize(testData, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(expectedFilePath, jsonContent);

            // Act: Ejecutar SeedTestDataAsync
            // No debe lanzar excepción de Foreign Key
            Func<Task> seedAction = async () => await seeder.SeedTestDataAsync();
            await seedAction.Should().NotThrowAsync("SeedTestDataAsync no debe lanzar excepción de Foreign Key");

            // Assert: Verificar que el conteo de usuarios en BD sea 0
            var userCount = await context.Users
                .IgnoreQueryFilters()
                .CountAsync();
            
            userCount.Should().Be(0, "No debe haber usuarios insertados porque la empresa padre fue rechazada");

            // Verificar que la empresa inválida tampoco fue insertada
            var companyCount = await context.Companies
                .IgnoreQueryFilters()
                .CountAsync();
            
            companyCount.Should().Be(0, "La empresa inválida no debe haber sido insertada");
        }
        finally
        {
            // Restaurar archivo original si existía
            if (originalExists && originalContent != null)
            {
                await File.WriteAllTextAsync(expectedFilePath, originalContent);
            }
            else if (File.Exists(expectedFilePath) && !originalExists)
            {
                File.Delete(expectedFilePath);
            }
        }
    }
}
