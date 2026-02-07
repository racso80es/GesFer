using GesFer.Admin.Api;
using GesFer.Admin.Infrastructure.Data;
using GesFer.Admin.Infrastructure.Services;
using GesFer.Infrastructure.Services;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MySql;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GesFer.Admin.IntegrationTests;

public class AdminWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private bool _useInMemory = true;
    private readonly string _inMemoryDbName = "GesFerAdminTestDb_InMemory_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContextOptions for AdminDbContext
            var dbContextOptionsDescriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AdminDbContext>)).ToList();
            foreach (var descriptor in dbContextOptionsDescriptors) services.Remove(descriptor);

            var dbContextDescriptors = services.Where(d => d.ServiceType == typeof(AdminDbContext)).ToList();
            foreach (var descriptor in dbContextDescriptors) services.Remove(descriptor);

            // Add new DbContext for AdminDbContext
            services.AddDbContext<AdminDbContext>((serviceProvider, options) =>
            {
                options.UseInMemoryDatabase(_inMemoryDbName);
                options.EnableSensitiveDataLogging();
            }, ServiceLifetime.Scoped);

            // Register Seeder dependencies
            services.AddScoped<AdminJsonDataSeeder>();
            services.AddSingleton<ISensitiveDataSanitizer, SensitiveDataSanitizer>();
        });

        builder.UseEnvironment("Development");
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Run Seeder
        var seeder = scope.ServiceProvider.GetRequiredService<AdminJsonDataSeeder>();
        await seeder.SeedAdminUsersAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
