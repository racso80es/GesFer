using GesFer.Api;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Aplicar migraciones automáticamente en desarrollo (antes de configurar el pipeline)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Verificando conexión a la base de datos...");
        
        if (context.Database.CanConnect())
        {
            logger.LogInformation("Verificando si la base de datos necesita migraciones...");
            
            // Solo intentar migraciones si es una base de datos relacional (no InMemory)
            if (context.Database.IsRelational())
            {
                try
                {
                    // Verificar si hay migraciones pendientes
                    var pendingMigrations = context.Database.GetPendingMigrations().ToList();
                    
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation($"Aplicando {pendingMigrations.Count} migraciones pendientes...");
                        context.Database.Migrate();
                        logger.LogInformation("Migraciones aplicadas correctamente.");
                    }
                    else
                    {
                        // Si no hay migraciones, verificar si las tablas existen
                        // Si no existen, usar EnsureCreated (solo para desarrollo)
                        if (app.Environment.IsDevelopment())
                        {
                            try
                            {
                                // Verificar si existe la tabla Companies
                                var tableCount = context.Database.SqlQueryRaw<int>(
                                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Companies'")
                                    .FirstOrDefault();
                                
                                if (tableCount == 0)
                                {
                                    // No existe la tabla, crear la base de datos
                                    logger.LogWarning("No se encontró la tabla Companies. Creando base de datos con EnsureCreated (solo desarrollo)...");
                                    
                                    // Eliminar la tabla de migraciones si existe para permitir EnsureCreated
                                    try
                                    {
                                        context.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS __EFMigrationsHistory;");
                                        logger.LogInformation("Tabla de migraciones eliminada para permitir EnsureCreated.");
                                    }
                                    catch (Exception dropEx)
                                    {
                                        logger.LogWarning(dropEx, "No se pudo eliminar la tabla de migraciones, continuando...");
                                    }
                                    
                                    context.Database.EnsureCreated();
                                    logger.LogInformation("Base de datos creada correctamente con EnsureCreated.");
                                }
                                else
                                {
                                    logger.LogInformation("Base de datos ya tiene tablas.");
                                }
                            }
                            catch (Exception tableCheckEx)
                            {
                                // Si falla la verificación, intentar crear la base de datos directamente
                                logger.LogWarning(tableCheckEx, "Error al verificar tablas. Intentando crear base de datos con EnsureCreated (solo desarrollo)...");
                                try
                                {
                                    // Eliminar la tabla de migraciones si existe
                                    try
                                    {
                                        context.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS __EFMigrationsHistory;");
                                    }
                                    catch { }
                                    
                                    context.Database.EnsureCreated();
                                    logger.LogInformation("Base de datos creada correctamente.");
                                }
                                catch (Exception ensureEx)
                                {
                                    logger.LogError(ensureEx, "No se pudo crear la base de datos. Asegúrate de que MySQL esté ejecutándose. Error: {ErrorMessage}", ensureEx.Message);
                                }
                            }
                        }
                        else
                        {
                            logger.LogInformation("No hay migraciones pendientes y la base de datos está actualizada.");
                        }
                    }
                }
                catch (Exception migrationEx)
                {
                    logger.LogWarning(migrationEx, "Error al verificar migraciones. Intentando crear base de datos con EnsureCreated (solo desarrollo)...");
                    if (app.Environment.IsDevelopment())
                    {
                        try
                        {
                            context.Database.EnsureCreated();
                            logger.LogInformation("Base de datos creada correctamente.");
                        }
                        catch (Exception ensureEx)
                        {
                            logger.LogError(ensureEx, "No se pudo crear la base de datos. Asegúrate de que MySQL esté ejecutándose.");
                        }
                    }
                }
            }
            else
            {
                // Base de datos no relacional (por ejemplo, InMemory para tests)
                logger.LogInformation("Base de datos no relacional detectada. Omitiendo migraciones.");
            }
        }
        else
        {
            logger.LogWarning("No se puede conectar a la base de datos. Las migraciones se aplicarán cuando la BD esté disponible.");
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error al aplicar migraciones. La aplicación continuará sin aplicar migraciones. Error: {ErrorMessage}", ex.Message);
    
    // En desarrollo, intentar crear la base de datos si no existe
    if (app.Environment.IsDevelopment())
    {
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                logger.LogWarning("Intentando crear la base de datos con EnsureCreated como último recurso...");
                context.Database.EnsureCreated();
                logger.LogInformation("Base de datos creada con EnsureCreated.");
            }
        }
        catch (Exception ensureEx)
        {
            logger.LogError(ensureEx, "No se pudo crear la base de datos. Asegúrate de que MySQL esté ejecutándose y las credenciales sean correctas.");
        }
    }
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
app.UseAuthorization();
app.MapControllers();

app.Run();
