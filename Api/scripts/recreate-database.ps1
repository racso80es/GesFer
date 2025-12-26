# Script para recrear completamente la base de datos y añadir datos
Write-Host "=== Recreacion Completa de Base de Datos GesFer ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker este corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no esta corriendo. Por favor, inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "   Docker esta corriendo" -ForegroundColor Green

# Verificar que MySQL este listo
Write-Host "2. Verificando MySQL..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$mysqlReady = $false
do {
    Start-Sleep -Seconds 2
    $attempt++
    docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $mysqlReady = $true
        break
    }
    Write-Host "   Intento $attempt/$maxAttempts..." -ForegroundColor Gray
} while ($attempt -lt $maxAttempts)

if (-not $mysqlReady) {
    Write-Host "ERROR: MySQL no esta listo despues de $maxAttempts intentos" -ForegroundColor Red
    exit 1
}
Write-Host "   MySQL esta listo" -ForegroundColor Green

# Esperar un poco mas para asegurar que MySQL este completamente listo
Start-Sleep -Seconds 3

# Ejecutar InitDatabase.cs para recrear todas las tablas
Write-Host "3. Recreando estructura de base de datos..." -ForegroundColor Yellow
$scriptsPath = $PSScriptRoot
$initDatabasePath = Join-Path $scriptsPath "InitDatabase.csproj"

if (-not (Test-Path $initDatabasePath)) {
    Write-Host "ERROR: No se encontro InitDatabase.csproj" -ForegroundColor Red
    exit 1
}

Set-Location $scriptsPath
dotnet run --project InitDatabase.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No se pudieron crear las tablas" -ForegroundColor Red
    exit 1
}
Write-Host "   Estructura de base de datos recreada" -ForegroundColor Green

# Insertar datos de prueba
Write-Host "4. Insertando datos iniciales..." -ForegroundColor Yellow
$sqlFile = Join-Path $scriptsPath "seed-data.sql"

if (Test-Path $sqlFile) {
    $result = Get-Content $sqlFile -Raw | docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-Host "   Datos iniciales insertados correctamente" -ForegroundColor Green
    }
    else {
        if ($result -match "Duplicate entry") {
            Write-Host "   Algunos datos ya existen (esto es normal si ejecutas el script varias veces)" -ForegroundColor Yellow
        }
        else {
            Write-Host "   Error al insertar datos. Verifica los logs arriba." -ForegroundColor Yellow
            Write-Host "   Resultado: $result" -ForegroundColor Gray
        }
    }
}
else {
    Write-Host "   ERROR: No se encontro el archivo seed-data.sql" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Recreacion completada ===" -ForegroundColor Green
Write-Host ""
Write-Host "Datos de prueba creados:" -ForegroundColor Cyan
Write-Host "  - Idiomas: Espanol, English, Catala" -ForegroundColor White
Write-Host "  - Empresa: Empresa Demo" -ForegroundColor White
Write-Host "  - Usuario: admin" -ForegroundColor White
Write-Host "  - Contrasena: admin123" -ForegroundColor White
Write-Host ""
Write-Host "Puedes probar el login en:" -ForegroundColor Cyan
Write-Host "  https://localhost:5001/api/auth/login" -ForegroundColor White
Write-Host ""
