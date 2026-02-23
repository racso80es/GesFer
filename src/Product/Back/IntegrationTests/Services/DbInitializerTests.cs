using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Product.Back.Domain.Services;
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
        services.AddScoped<IIntegrityCheckService, ProductIntegrityService>();
        services.AddScoped<DbInitializer>();

        // Mock Migration Service (avoid DB operations on InMemory)
        var mockMigration = new Mock<IMigrationService>();
        services.AddSingleton(mockMigration.Object);

        // Mock Sanitizer
        var mockSanitizer = new Mock<ISensitiveDataSanitizer>();
        mockSanitizer.Setup(s => s.GenerateRandomPassword(It.IsAny<int>())).Returns("RandomPass123!");
        services.AddSingleton(mockSanitizer.Object);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        using (var scope = serviceProvider.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            await initializer.InitializeAsync();
        }

        // Assert
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            var admin = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == "admin");

            admin.Should().NotBeNull();
            admin!.PasswordHash.Should().NotBeNullOrEmpty();
        }
    }
}
