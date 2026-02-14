using GesFer.Admin.Api;
using GesFer.Admin.Infrastructure.Data;
using GesFer.Admin.Infrastructure.Services;
using Serilog.Sinks.MySQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;

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
    Log.Information("Iniciando aplicación GesFer Admin API");

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
            .Enrich.WithProperty("Application", "GesFer.Admin.Api")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

        if (isDevelopment)
        {
            configuration
                .MinimumLevel.Verbose()
                .WriteTo.Console();
            // MySQL sink opcional en dev: si no está disponible, evita 500 en Swagger/endpoints
            var useMySqlLogging = builder.Configuration.GetValue<bool>("Serilog:UseMySql");
            if (useMySqlLogging)
            {
                configuration.WriteTo.MySQL(
                    connectionString: connectionString,
                    tableName: "Logs",
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    storeTimestampInUtc: true);
            }
        }
        else
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.MySQL(
                    connectionString: connectionString,
                    tableName: "Logs",
                    restrictedToMinimumLevel: LogEventLevel.Information,
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
            Title = "GesFer Admin API",
            Version = "v1",
            Description = "API RESTful para gestión administrativa del sistema GesFer"
        });
        
        // Configurar seguridad JWT en Swagger
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        c.UseInlineDefinitionsForEnums();
        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
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
        builder.Services.Configure<HttpsRedirectionOptions>(options => { options.HttpsPort = 5011; });

    // Configurar inyección de dependencias
    builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

    // Healthchecks
    builder.Services.AddHealthChecks();

    // Configurar autenticación JWT (misma configuración que Product)
    var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] 
        ?? throw new InvalidOperationException("JwtSettings:SecretKey no está configurado");

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
            ClockSkew = TimeSpan.Zero,
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

    // Inicializar base de datos y seeds (Se ejecuta siempre para garantizar consistencia)
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            // Ejecutar migraciones si es necesario (o usar EnsureCreated si es InMemory/Dev)
            var context = services.GetRequiredService<AdminDbContext>();
            // En producción idealmente se usa 'dotnet ef database update', pero aquí simplificamos
            // await context.Database.MigrateAsync();

            var seeder = services.GetRequiredService<AdminJsonDataSeeder>();
            await seeder.SeedCompaniesAsync();
            await seeder.SeedAdminUsersAsync();
            Log.Information("Seeds de Admin ejecutados correctamente.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ocurrió un error al ejecutar los seeds de Admin.");
        }
    }

    // Configurar el pipeline HTTP
    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    {
        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "GesFer Admin API v1");
            c.RoutePrefix = "swagger";
        });
    }

    // Redirección HTTP → HTTPS: omitir en Development para que Swagger funcione solo con HTTP (5010).
    // Si se redirige a 5011 y el perfil solo enlaza 5010, el fetch de swagger.json falla con 500/Fetch error.
    if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
    {
        app.UseHttpsRedirection();
    }
    app.UseRouting();
    app.UseCors("AllowAll");
    

    // Autenticación y autorización deben ir en este orden
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();

    Log.Information("Aplicación GesFer Admin API iniciada correctamente");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error fatal al iniciar la aplicación: {Message}. Inner: {Inner}",
        ex.Message, ex.InnerException?.Message ?? "(ninguno)");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
