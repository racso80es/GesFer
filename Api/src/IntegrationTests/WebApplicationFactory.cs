using GesFer.Api;
using GesFer.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GesFer.IntegrationTests;

/// <summary>
/// Factory para crear una instancia de la aplicación para tests de integración
/// </summary>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover el DbContext real (buscar por el tipo de servicio registrado)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // También remover el ApplicationDbContext si está registrado directamente
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Agregar DbContext en memoria para tests
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("GesFerTestDb");
            });
        });

        builder.UseEnvironment("Testing");
    }
}

