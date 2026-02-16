
using GesFer.Admin.Infrastructure.Services;
using GesFer.Admin.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace GesFer.Admin.Api;

/// <summary>
/// Configuración de inyección de dependencias para Admin API
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra todos los servicios de la aplicación Admin
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment = null)
    {
        // Configurar DbContext - Usar la misma cadena de conexión que Product
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

        var isDevelopment = environment?.IsDevelopment() ?? false;

        // 1. Contexto de Admin (Escritura de Logs, Audit, AdminUsers)
        services.AddDbContext<AdminDbContext>((serviceProvider, options) =>
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
                    // Usar tabla de historial de migraciones separada
                    mysqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Admin");
                });

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Servicios de infraestructura Admin
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAdminJwtService, AdminJwtService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<AdminJsonDataSeeder>();

        // Servicios Shared
        services.AddSingleton<ISequentialGuidGenerator, MySqlSequentialGuidGenerator>();
        services.AddSingleton<ISensitiveDataSanitizer, SensitiveDataSanitizer>();

        // Registrar MediatR para manejar comandos/queries
        // Escanear el ensamblado de Application
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GesFer.Admin.Application.DTOs.Company.CompanyDto).Assembly));

        // Registrar cliente de Product API (Dashboard Stats Aggregation)
        services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
        {
            // Usar configuración ProductApi:BaseUrl o default a 5002
            var productApiBaseUrl = configuration["ProductApi:BaseUrl"] ?? "http://localhost:5002";
            client.BaseAddress = new Uri(productApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
