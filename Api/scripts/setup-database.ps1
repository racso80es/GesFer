# Script para configurar la base de datos
# 1. Verifica Docker y MySQL
# 2. Inserta datos de prueba

Write-Host "=== Configuración de Base de Datos GesFer ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker esté corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
docker ps | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Docker está corriendo" -ForegroundColor Green

# Verificar que MySQL esté disponible
Write-Host "2. Verificando MySQL..." -ForegroundColor Yellow
docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: MySQL no está disponible." -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ MySQL está disponible" -ForegroundColor Green

# Verificar si las tablas existen
Write-Host "3. Verificando si las tablas existen..." -ForegroundColor Yellow
$tableResult = docker exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e "SHOW TABLES;" 2>&1
$hasTables = $tableResult -match "Companies"

if (-not $hasTables) {
    Write-Host "   ⚠ Las tablas no existen aún." -ForegroundColor Yellow
    Write-Host "   Por favor:" -ForegroundColor Yellow
    Write-Host "   1. Inicia la API desde Visual Studio (F5)" -ForegroundColor White
    Write-Host "   2. Espera a que las migraciones se ejecuten automáticamente" -ForegroundColor White
    Write-Host "   3. Vuelve a ejecutar este script para insertar datos de prueba" -ForegroundColor White
    Write-Host ""
    Write-Host "   O ejecuta manualmente:" -ForegroundColor Yellow
    Write-Host "   cd src/Api" -ForegroundColor White
    Write-Host "   dotnet ef database update --project ../Infrastructure/GesFer.Infrastructure.csproj" -ForegroundColor White
    exit 0
}

Write-Host "   ✓ Las tablas existen" -ForegroundColor Green

# Insertar datos de prueba
Write-Host "4. Insertando datos de prueba..." -ForegroundColor Yellow
$sqlFile = Join-Path $PSScriptRoot "seed-data.sql"

if (Test-Path $sqlFile) {
    $result = Get-Content $sqlFile | docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ Datos de prueba insertados correctamente" -ForegroundColor Green
    }
    else {
        if ($result -match "Duplicate entry") {
            Write-Host "   ⚠ Algunos datos ya existen (esto es normal si ejecutas el script varias veces)" -ForegroundColor Yellow
        }
        else {
            Write-Host "   ⚠ Error al insertar datos. Verifica los logs arriba." -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "   ERROR: No se encontró el archivo seed-data.sql" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Configuración completada ===" -ForegroundColor Green
Write-Host ""
Write-Host "Datos de prueba creados:" -ForegroundColor Cyan
Write-Host "  Empresa: Empresa Demo" -ForegroundColor White
Write-Host "  Usuario: admin" -ForegroundColor White
Write-Host "  Contraseña: admin123" -ForegroundColor White
Write-Host ""
Write-Host "Puedes probar el login en:" -ForegroundColor Cyan
Write-Host "  http://localhost:5000/api/auth/login" -ForegroundColor White
Write-Host ""
