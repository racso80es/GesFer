using GesFer.Api;
using GesFer.Api.Services;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.IntegrationTests.Helpers;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Diagnostics;
using Testcontainers;
using Testcontainers.MySql;
using Xunit;

namespace GesFer.IntegrationTests;

public class IntegrationTestWebAppFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    private MySqlContainer? _mySqlContainer;
    private bool _useInMemory = false;
    private string? _connectionString;
    private readonly object _connectionStringLock = new object();
    private readonly string _inMemoryDbName = "GesFerTestDb_InMemory_" + Guid.NewGuid();

    public IntegrationTestWebAppFactory()
    {
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextOptionsDescriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)).ToList();
            foreach (var descriptor in dbContextOptionsDescriptors) services.Remove(descriptor);

            var dbContextDescriptors = services.Where(d => d.ServiceType == typeof(ApplicationDbContext)).ToList();
            foreach (var descriptor in dbContextDescriptors) services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                if (_useInMemory)
                {
                    options.UseInMemoryDatabase(_inMemoryDbName);
                    options.EnableSensitiveDataLogging();
                }
                else
                {
                    string connectionString;
                    lock (_connectionStringLock)
                    {
                         if (_connectionString == null)
                         {
                            // This should not happen if InitializeAsync works correctly,
                            // but if it does, it means we are in a broken state.
                            // Fallback to InMemory or throw?
                            // Since we are inside ConfigureWebHost, we can't easily switch _useInMemory here effectively
                            // if services were already built, but here we are defining the service.
                            // However, let's assume if connection string is null, something went wrong.
                            throw new InvalidOperationException("Connection string not available for MySql, but _useInMemory is false.");
                         }
                         connectionString = _connectionString;
                    }
                    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
                }
            }, ServiceLifetime.Scoped);

            // Reemplazar IAdminApiClient por mock para tests (MyCompanyController no llama a Admin real)
            var adminClientDescriptors = services.Where(d => d.ServiceType == typeof(IAdminApiClient)).ToList();
            foreach (var d in adminClientDescriptors)
                services.Remove(d);
            services.AddScoped<IAdminApiClient, GesFer.IntegrationTests.Helpers.MockAdminApiClient>();

            // Reemplazar ISetupService por mock para tests (evitar Docker en Initialize_EndpointShouldExist)
            var setupServiceDescriptors = services.Where(d => d.ServiceType == typeof(ISetupService)).ToList();
            foreach (var d in setupServiceDescriptors)
                services.Remove(d);
            services.AddScoped<ISetupService, MockSetupService>();
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Error);
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // 1. Check for explicit Override
        var envVar = Environment.GetEnvironmentVariable("TEST_USE_IN_MEMORY");
        if (!string.IsNullOrEmpty(envVar) && (envVar == "1" || envVar.ToLower() == "true"))
        {
            Console.WriteLine("[IntegrationTestWebAppFactory] TEST_USE_IN_MEMORY detected. Force InMemory.");
            _useInMemory = true;
            await InitializeInMemoryAsync();
            return;
        }

        // 2. Check Docker Availability
        if (!IsDockerAvailable())
        {
            Console.WriteLine("[IntegrationTestWebAppFactory] Docker not detected. Switching to InMemory mode.");
            _useInMemory = true;
            await InitializeInMemoryAsync();
            return;
        }

        // 3. Try to Start Docker
        try
        {
            _mySqlContainer = new MySqlBuilder("mysql:8.0")
                .WithDatabase("GesFerTestDb")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .WithEnvironment("MYSQL_ROOT_PASSWORD", "rootpassword")
                .Build();

            await _mySqlContainer.StartAsync();
            lock (_connectionStringLock) _connectionString = _mySqlContainer.GetConnectionString();

            // Only AFTER successful start do we access Services, which triggers Host Build
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
            await DbInitializer.InitializeAsync(Services, false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[IntegrationTestWebAppFactory] Docker container failed to start. Switching to InMemory. Error: {ex.Message}");
            _useInMemory = true;
            await InitializeInMemoryAsync();
        }
    }

    private async Task InitializeInMemoryAsync()
    {
        // Accessing Services triggers Host Build.
        // At this point _useInMemory is guaranteed to be true.
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.InitializeAsync(Services, false);
    }

    public new async Task DisposeAsync()
    {
        try
        {
            if (_mySqlContainer != null)
            {
                await _mySqlContainer.DisposeAsync();
            }
        }
        catch
        {
            // Ignore disposal errors if container failed to start
        }
        await base.DisposeAsync();
    }

    private bool IsDockerAvailable()
    {
        try
        {
            using var process = new Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = "info"; // 'info' is more robust than 'ps'
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            // Wait a bit longer, 5 seconds
            bool exited = process.WaitForExit(5000);
            if (!exited)
            {
                process.Kill();
                return false;
            }
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
