using GesFer.Api;
using GesFer.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Async;
using Serilog.Sinks.MySQL;
using Serilog.Debugging;
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

    // Configurar Serilog
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

    var isDevelopment = builder.Environment.IsDevelopment();

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "GesFer.Api")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

        if (isDevelopment)
        {
            // En desarrollo: loguear TODOS los tipos de logs (Verbose, Debug, Information, Warning, Error, Fatal) a Consola y MySQL
            configuration
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.MySQL(
                    connectionString: connectionString,
                    tableName: "Logs",
                    restrictedToMinimumLevel: LogEventLevel.Verbose, // Nivel mínimo explícito para el sink
                    storeTimestampInUtc: true);
        }
        else
        {
            // En producción: solo Information y superiores a la Base de Datos
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.MySQL(
                    connectionString: connectionString,
                    tableName: "Logs",
                    restrictedToMinimumLevel: LogEventLevel.Information, // Nivel mínimo explícito para el sink
                    storeTimestampInUtc: true);
        }
    });

    // Configurar servicios
    builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GesFer API",
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

// Configurar inyección de dependencias
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

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
        ClockSkew = TimeSpan.Zero // Eliminar el tiempo de gracia por defecto
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

// CORS debe ir ANTES de UseHttpsRedirection para que las peticiones preflight funcionen
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Autenticación y autorización deben ir en este orden
app.UseAuthentication();
app.UseAuthorization();

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
