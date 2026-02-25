using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;

namespace GesFer.IntegrationTests.Services;

public class DbInitializerTests
{
    [Fact]
    public async Task InitializeAsync_ShouldOrchestrateInitialization()
    {
        // Arrange
        var services = new ServiceCollection();

        // Mock Environment
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");
        services.AddSingleton(mockEnv.Object);

        // Logging
        services.AddLogging();

        // DbContext (InMemory) - Real Instance for Seeder
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<ProductDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: dbName));

        // IConfiguration
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:CompanyId"] = "11111111-1111-1111-1111-111111111115" })
            .Build();
        services.AddSingleton<IConfiguration>(config);

        // Dependencies
        services.AddScoped<ISensitiveDataSanitizer, SensitiveDataSanitizer>();

        // Use Mock for Migration
        var mockMigration = new Mock<IMigrationService>();
        services.AddScoped(_ => mockMigration.Object);

        // Use Mock for Integrity to avoid side effects and "Admin not found" error if seeds are missing
        var mockIntegrity = new Mock<IIntegrityCheckService>();
        services.AddScoped(_ => mockIntegrity.Object);

        // Use Real JsonDataSeeder with Real DbContext
        // We need to register it so it picks up the DbContext from DI
        services.AddScoped<JsonDataSeeder>();

        // DbInitializer
        services.AddScoped<DbInitializer>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();

        await initializer.InitializeAsync();

        // Assert
        // Verify Migration Service was called
        mockMigration.Verify(m => m.ApplyMigrationsAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify Integrity Service was called (last step)
        mockIntegrity.Verify(i => i.EnsureIntegrityAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify Seeder attempted (implied by execution reaching Integrity, but we can't verify calls on real object easily without spying)
        // However, if Seeder throws (e.g. file missing and no handling), test fails.
        // JsonDataSeeder logs warning if file missing but doesn't throw.
        // So this confirms the flow completed without error.
    }
}
