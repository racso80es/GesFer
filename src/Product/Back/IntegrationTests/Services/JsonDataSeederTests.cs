using FluentAssertions;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:CompanyId"] = "11111111-1111-1111-1111-111111111115" })
            .Build();
        services.AddSingleton<IConfiguration>(config);

        // Registrar JsonDataSeeder
        services.AddScoped<JsonDataSeeder>();
        services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();
        services.AddSingleton<ISensitiveDataSanitizer, SensitiveDataSanitizer>();

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

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:CompanyId"] = "11111111-1111-1111-1111-111111111115" })
            .Build();
        services.AddSingleton<IConfiguration>(config);

        services.AddScoped<JsonDataSeeder>();
        services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();
        services.AddSingleton<ISensitiveDataSanitizer, SensitiveDataSanitizer>();

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

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:CompanyId"] = "11111111-1111-1111-1111-111111111115" })
            .Build();
        services.AddSingleton<IConfiguration>(config);

        services.AddScoped<JsonDataSeeder>();
        services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();
        services.AddSingleton<ISensitiveDataSanitizer, SensitiveDataSanitizer>();

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

    /// <summary>
    /// Valida que demo-data.json contiene taxTypes con los códigos habituales en España
    /// (IVA21, IVA10, IVA4, EXENTO) según la spec ArticleFamily.
    /// </summary>
    [Fact]
    public void DemoData_ShouldContainTaxTypes_WithSpainCodes()
    {
        var basePath = AppContext.BaseDirectory;
        var seedsPath = Path.Combine(basePath, "Data", "Seeds");
        var filePath = Path.Combine(seedsPath, "demo-data.json");
        File.Exists(filePath).Should().BeTrue("demo-data.json debe estar en Data/Seeds (copiado por el proyecto de tests)");

        var json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.TryGetProperty("taxTypes", out var taxTypesProp).Should().BeTrue("demo-data debe tener clave taxTypes");
        var taxTypes = taxTypesProp;
        taxTypes.GetArrayLength().Should().Be(4, "debe haber 4 tipos de impuesto (IVA21, IVA10, IVA4, EXENTO)");

        var codes = new List<string>();
        var values = new List<decimal>();
        foreach (var item in taxTypes.EnumerateArray())
        {
            if (item.TryGetProperty("code", out var code))
                codes.Add(code.GetString() ?? "");
            if (item.TryGetProperty("value", out var value))
                values.Add(value.GetDecimal());
        }
        codes.Should().Contain("IVA21").And.Contain("IVA10").And.Contain("IVA4").And.Contain("EXENTO");
        values.Should().Contain(21).And.Contain(10).And.Contain(4).And.Contain(0);
    }

    /// <summary>
    /// Valida que demo-data.json contiene articleFamilies con campos requeridos (id, companyId, code, name, taxTypeId).
    /// Kaizen: protege la estructura tras Fase 2.
    /// </summary>
    [Fact]
    public void DemoData_ShouldContainArticleFamilies_WithRequiredFields()
    {
        var basePath = AppContext.BaseDirectory;
        var seedsPath = Path.Combine(basePath, "Data", "Seeds");
        var filePath = Path.Combine(seedsPath, "demo-data.json");
        File.Exists(filePath).Should().BeTrue("demo-data.json debe estar en Data/Seeds");

        var json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.TryGetProperty("articleFamilies", out var afProp).Should().BeTrue("demo-data debe tener clave articleFamilies");
        var articleFamilies = afProp;
        articleFamilies.GetArrayLength().Should().BeGreaterThan(0, "debe haber al menos una familia de artículos");

        foreach (var item in articleFamilies.EnumerateArray())
        {
            item.TryGetProperty("id", out _).Should().BeTrue("cada articleFamily debe tener id");
            item.TryGetProperty("companyId", out _).Should().BeTrue("cada articleFamily debe tener companyId");
            item.TryGetProperty("code", out _).Should().BeTrue("cada articleFamily debe tener code");
            item.TryGetProperty("name", out _).Should().BeTrue("cada articleFamily debe tener name");
            item.TryGetProperty("taxTypeId", out _).Should().BeTrue("cada articleFamily debe tener taxTypeId");
        }
    }
}
