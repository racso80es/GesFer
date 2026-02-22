using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Domain.Services;
using GesFer.Product.Back.Infrastructure.Services;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Product.Back.Infrastructure.DTOs;
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

        // Mock Migration Service (Skip for InMemory)
        var mockMigrationService = new Mock<IMigrationService>();
        services.AddScoped<IMigrationService>(_ => mockMigrationService.Object);

        // Mock Admin API Client
        var mockAdminApiClient = new Mock<IAdminApiClient>();
        // Setup GetCompanyAsync to return a valid company for the admin user
        // The integrity service checks for this company.
        // Expected ID from test-data.json: 11111111-1111-1111-1111-111111111115
        var adminCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111115");
        mockAdminApiClient.Setup(c => c.GetCompanyAsync(adminCompanyId))
            .ReturnsAsync(new AdminCompanyDto { Id = adminCompanyId, Name = "Empresa Demo" });
        services.AddScoped<IAdminApiClient>(_ => mockAdminApiClient.Object);


        // Mock Sanitizer
        var mockSanitizer = new Mock<ISensitiveDataSanitizer>();
        mockSanitizer.Setup(s => s.GenerateRandomPassword(It.IsAny<int>())).Returns("RandomPass123!");
        mockSanitizer.Setup(s => s.GenerateRandomEmail(It.IsAny<string>(), It.IsAny<string>())).Returns("admin@gesfer.local");
        services.AddSingleton(mockSanitizer.Object);

        // Real Integrity Service (to test EnsureAdminUser logic)
        services.AddScoped<IIntegrityCheckService, ProductIntegrityService>();

        // Real DbInitializer
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
        admin!.PasswordHash.Should().NotBeNullOrEmpty();
    }
}
