using GesFer.Api.Services;
using GesFer.Application.Services;
using GesFer.Domain.Services;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Repositories;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace GesFer.Api;

/// <summary>
/// Configuración de inyección de dependencias
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios de la aplicación
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment = null)
    {
        // Configurar DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

        var isDevelopment = environment?.IsDevelopment() ?? false;

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 0)),
                mysqlOptions =>
                {
                    mysqlOptions.EnableStringComparisonTranslations();
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Repositorios genéricos (si se necesitan)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Servicios de infraestructura
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStockService, StockService>();

        // Servicios de aplicación
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        services.AddScoped<IPurchaseDeliveryNoteService, PurchaseDeliveryNoteService>();
        services.AddScoped<ISalesDeliveryNoteService, SalesDeliveryNoteService>();

        // Servicios de API
        services.AddScoped<ISetupService, SetupService>();

        return services;
    }
}

