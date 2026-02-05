using GesFer.Api;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MySql;
using Testcontainers;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Xunit;

namespace GesFer.IntegrationTests;

public class IntegrationTestWebAppFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    private MySqlContainer? _mySqlContainer;
    private bool _useInMemory = false;
    private string? _connectionString;
    private readonly object _connectionStringLock = new object();

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
                    options.UseInMemoryDatabase("GesFerTestDb_InMemory_" + Guid.NewGuid());
                    options.EnableSensitiveDataLogging();
                }
                else
                {
                    string connectionString;
                    lock (_connectionStringLock)
                    {
                         if (_connectionString == null) throw new InvalidOperationException("Connection string not available.");
                         connectionString = _connectionString;
                    }
                    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
                }
            }, ServiceLifetime.Scoped);

            services.AddHttpClient("AdminApi", client => client.BaseAddress = new Uri("http://localhost:5010"));
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        if (!IsDockerAvailable())
        {
            Console.WriteLine("[IntegrationTestWebAppFactory] Docker not detected. Switching to InMemory mode.");
            _useInMemory = true;
            await InitializeInMemoryAsync();
            return;
        }

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
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = "ps -q";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit(3000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
