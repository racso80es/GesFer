using GesFer.Api.Services;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.Handlers.Auth;
using GesFer.Application.Handlers.PurchaseDeliveryNote;
using GesFer.Application.Handlers.SalesDeliveryNote;
using GesFer.Domain.Services;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Repositories;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using System.Reflection;

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
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAdminJwtService, AdminJwtService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IStockService, StockService>();

        // Command Handlers - Registro automático de todos los handlers
        RegisterCommandHandlers(services);

        // Servicios de API
        services.AddScoped<ISetupService, SetupService>();

        return services;
    }

    /// <summary>
    /// Registra todos los Command Handlers automáticamente mediante reflexión
    /// </summary>
    private static void RegisterCommandHandlers(IServiceCollection services)
    {
        var assembly = typeof(Application.Common.Interfaces.ICommandHandler<>).Assembly;
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && 
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) || 
                     i.GetGenericTypeDefinition() == typeof(ICommandHandler<>))))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && 
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) || 
                     i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }
}

