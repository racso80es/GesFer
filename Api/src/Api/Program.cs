using GesFer.Api;
using GesFer.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

builder.Services.AddAuthorization();

var app = builder.Build();

// NOTA: La gestión de la base de datos (migraciones, creación de tablas, datos iniciales)
// se realiza mediante scripts externos (inicializar-completo.bat, scripts SQL, etc.)
// La API solo se conecta a la base de datos existente sin realizar verificaciones automáticas.

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

app.Run();
