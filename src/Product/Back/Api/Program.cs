using GesFer.Api;
using GesFer.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Debugging;
using GesFer.Infrastructure.Logging;
using System.Text;

// Habilitar self-logging de Serilog para diagnosticar problemas
SelfLog.Enable(msg => Console.Error.WriteLine($"[SERILOG INTERNAL] {msg}"));

// Configurar Serilog antes de crear el builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando aplicación GesFer API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("appsettings.Seed.json", optional: true, reloadOnChange: false);

    // Configurar Serilog
    // NOTA: Los logs NO se escriben directamente a BD en Product
    // Se envían a Admin API mediante AsyncLogPublisher (Fire and Forget)
    var isDevelopment = builder.Environment.IsDevelopment();

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration) // <--- CARGA EL SINK DESDE EL JSON
            .ReadFrom.Services(services) // <--- PERMITE INYECTAR DEPENDENCIAS EN EL SINK
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "GesFer.Api")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

        if (isDevelopment || context.HostingEnvironment.EnvironmentName == "Testing")
        {
            // El nivel mínimo y el Console Sink ya vienen del JSON, 
            // pero podemos mantener esto como refuerzo o configurarlo todo en el JSON.
            configuration.MinimumLevel.Verbose();
        }
        else
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            // En PRO: Añadimos explícitamente el Sink aquí si no está en el appsettings.json base
            // Solo si el servicio está registrado (para evitar crash en entornos intermedios)
            var sink = services.GetService<AdminApiLogSink>();
            if (sink != null)
            {
                configuration.WriteTo.Sink(sink);
            }
        }
    });

    // Configurar servicios
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "GesFer product API",
            Version = "v1",
            Description = "API RESTful para gestión de compra/venta de chatarra"
        });
    
    // Configurar para mostrar valores por defecto desde el atributo [DefaultValue]
    c.SchemaFilter<GesFer.Api.Swagger.DefaultValueSchemaFilter>();
    c.UseInlineDefinitionsForEnums();
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Seguridad: HTTPS en todos los entornos. Redirección HTTP → HTTPS.
if (isDevelopment)
    builder.Services.Configure<HttpsRedirectionOptions>(options => { options.HttpsPort = 5001; });

// Configurar inyección de dependencias
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

// Healthchecks
builder.Services.AddHealthChecks();

// Configurar autenticación JWT
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] 
    ?? throw new InvalidOperationException("JwtSettings:SecretKey no está configurado");

// Validar que la clave tenga al menos 32 caracteres (256 bits) para SHA-256 (HS256)
if (jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException(
        $"JwtSettings:SecretKey debe tener al menos 32 caracteres (256 bits) para cumplir con el algoritmo SHA-256 (HS256). " +
        $"Longitud actual: {jwtSecretKey.Length} caracteres.");
}

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] 
    ?? throw new InvalidOperationException("JwtSettings:Issuer no está configurado");
var jwtAudience = builder.Configuration["JwtSettings:Audience"] 
    ?? throw new InvalidOperationException("JwtSettings:Audience no está configurado");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero, // Eliminar el tiempo de gracia por defecto
        // Configurar el tipo de claim para roles
        // ASP.NET Core busca roles en el claim especificado por RoleClaimType
        // Por defecto usa "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        // que es el valor de ClaimTypes.Role
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

builder.Services.AddAuthorization(options =>
{
    // Política de autorización que exige el claim role: Admin
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });
});

var app = builder.Build();

    // Agregar sink de Admin API después de que los servicios estén disponibles
    // Esto permite que los logs se envíen a Admin API mediante AsyncLogPublisher
    var logPublisher = app.Services.GetService<IAsyncLogPublisher>();
    if (logPublisher != null)
    {
        var adminApiSink = new AdminApiLogSink(app.Services);
        var minimumLevel = isDevelopment ? LogEventLevel.Verbose : LogEventLevel.Information;
        
        // Reconfigurar Serilog para incluir el sink de Admin API
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(isDevelopment ? LogEventLevel.Verbose : LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "GesFer.Api")
            .Enrich.WithProperty("Environment", app.Environment.EnvironmentName)
            .WriteTo.Console()
            .WriteTo.Sink(adminApiSink, restrictedToMinimumLevel: minimumLevel)
            .CreateLogger();
    }

// Inicializar base de datos (migraciones y seeding) según configuración
// Este proceso es idempotente y seguro de ejecutar múltiples veces
var shouldInitialize = false;
var isTesting = app.Environment.EnvironmentName == "Testing";

// En Testing siempre ejecutar (necesario para tests E2E)
if (isTesting)
{
    shouldInitialize = true;
}
// En Development, verificar la configuración AutoRunMigrations
else if (isDevelopment)
{
    // Leer configuración, por defecto false si no está configurado
    var autoRunMigrations = app.Configuration.GetValue<bool>("Database:AutoRunMigrations", false);
    shouldInitialize = autoRunMigrations;
    
    if (autoRunMigrations)
    {
        Log.Information("AutoRunMigrations está habilitado. Las migraciones se ejecutarán automáticamente al iniciar.");
    }
    else
    {
        Log.Information("AutoRunMigrations está deshabilitado. Las migraciones no se ejecutarán automáticamente.");
    }
}

if (shouldInitialize)
{
    await DbInitializer.InitializeAsync(app.Services, isDevelopment);
}

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GesFer API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

// CORS debe ir ANTES de UseHttpsRedirection para que las peticiones preflight funcionen.
// Redirección HTTP → HTTPS en todos los entornos (puerto HTTPS en Development: 5001).
app.UseCors("AllowAll");

// KAIZEN: Skip HTTPS Redirection in Testing to prevent "Failed to determine the https port" warning
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
else
{
    Log.Information("HTTPS Redirection skipped in Testing environment");
}

// Autenticación y autorización deben ir en este orden
app.UseAuthentication();
app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();

    Log.Information("Aplicación GesFer API iniciada correctamente");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error fatal al iniciar la aplicación");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
