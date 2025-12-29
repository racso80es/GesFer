# Script para forzar la creación de tablas usando Entity Framework
Write-Host "=== Forzando creación de tablas ===" -ForegroundColor Cyan

$apiPath = "C:\Proyectos\GesFer\Api\src\Api"
$projectPath = Join-Path $apiPath "GesFer.Api.csproj"

Write-Host "Compilando proyecto..." -ForegroundColor Yellow
Set-Location $apiPath
dotnet build --no-restore 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No se pudo compilar el proyecto" -ForegroundColor Red
    exit 1
}

Write-Host "Ejecutando inicialización de base de datos..." -ForegroundColor Yellow

# Crear un script C# temporal que ejecute EnsureCreated
$tempScript = @"
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

var connectionString = configuration.GetConnectionString("DefaultConnection");
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var serviceProvider = services.BuildServiceProvider();
var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try {
    logger.LogInformation("Verificando conexión a la base de datos...");
    if (context.Database.CanConnect()) {
        logger.LogInformation("Eliminando tabla de migraciones si existe...");
        try {
            context.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS __EFMigrationsHistory;");
        } catch { }
        
        logger.LogInformation("Creando tablas con EnsureCreated...");
        context.Database.EnsureCreated();
        logger.LogInformation("Tablas creadas correctamente!");
    } else {
        logger.LogError("No se puede conectar a la base de datos");
        Environment.Exit(1);
    }
} catch (Exception ex) {
    logger.LogError(ex, "Error al crear tablas: {0}", ex.Message);
    Environment.Exit(1);
}
"@

$scriptPath = Join-Path $apiPath "temp-init.cs"
$tempProgram = Join-Path $apiPath "TempInit.csproj"

# Crear archivo temporal
$tempProgramContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Infrastructure\GesFer.Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
  </ItemGroup>
</Project>
"@

Write-Host "Creando script temporal..." -ForegroundColor Yellow
$tempScript | Out-File -FilePath $scriptPath -Encoding UTF8
$tempProgramContent | Out-File -FilePath $tempProgram -Encoding UTF8

# Renombrar el archivo temporal a Program.cs
$programPath = Join-Path $apiPath "TempInitProgram.cs"
Copy-Item $scriptPath $programPath

try {
    Write-Host "Compilando y ejecutando script de inicialización..." -ForegroundColor Yellow
    Set-Location $apiPath
    dotnet run --project $tempProgram 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Tablas creadas correctamente!" -ForegroundColor Green
    } else {
        Write-Host "Error al crear tablas" -ForegroundColor Red
    }
} finally {
    # Limpiar archivos temporales
    if (Test-Path $scriptPath) { Remove-Item $scriptPath -Force }
    if (Test-Path $programPath) { Remove-Item $programPath -Force }
    if (Test-Path $tempProgram) { Remove-Item $tempProgram -Force }
}




