using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Product.Back.Domain.Services;
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
    public async Task InitializeAsync_ShouldCreateAdminUser_WithSanitizedPassword()
    {
        // Arrange
        var services = new ServiceCollection();

        // Mock Environment
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");
        services.AddSingleton(mockEnv.Object);

        // Logging
        services.AddLogging();

        // DbContext (InMemory)
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<ProductDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: dbName));

        // IConfiguration (JsonDataSeeder usa SeedConfig.GetValidCompanyIds)
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:CompanyId"] = "11111111-1111-1111-1111-111111111115" })
            .Build();
        services.AddSingleton<IConfiguration>(config);

        // Dependencies
        services.AddScoped<JsonDataSeeder>();

        // Mock Sanitizer (We will use a real instance or mock to verify calls)
        var mockSanitizer = new Mock<ISensitiveDataSanitizer>();
        mockSanitizer.Setup(s => s.GenerateRandomPassword(It.IsAny<int>())).Returns("RandomPass123!");
        mockSanitizer.Setup(s => s.GenerateRandomEmail(It.IsAny<string>(), It.IsAny<string>())).Returns("admin@gesfer.local");
        services.AddSingleton(mockSanitizer.Object);

        // Mock IMigrationService (no-op for test)
        var mockMigration = new Mock<IMigrationService>();
        services.AddSingleton(mockMigration.Object);

        // Mock IIntegrityCheckService (no-op for test)
        var mockIntegrity = new Mock<IIntegrityCheckService>();
        services.AddSingleton(mockIntegrity.Object);

        // Register DbInitializer
        services.AddScoped<DbInitializer>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await initializer.InitializeAsync();

        // Assert
        var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

        var admin = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == "admin");

        admin.Should().NotBeNull();
        // We expect the password hash to be present.
        admin!.PasswordHash.Should().NotBeNullOrEmpty();
    }
}
