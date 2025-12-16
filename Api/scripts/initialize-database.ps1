# Script para inicializar la base de datos y datos de prueba
Write-Host "=== Inicializacion de Base de Datos GesFer ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker este corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1
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
    $result = docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   MySQL esta listo" -ForegroundColor Green
        $mysqlReady = $true
        break
    }
    Write-Host "   Intento $attempt/$maxAttempts..." -ForegroundColor Gray
} while ($attempt -lt $maxAttempts)

if (-not $mysqlReady) {
    Write-Host "ERROR: MySQL no esta listo despues de $maxAttempts intentos" -ForegroundColor Red
    exit 1
}

# Esperar un poco más para asegurar que MySQL esté completamente listo
Start-Sleep -Seconds 5

# Conectar a MySQL y crear la base de datos si no existe
Write-Host "3. Verificando base de datos..." -ForegroundColor Yellow
$dbExists = docker exec gesfer_api_db mysql -u scrapuser -pscrappassword -e "SHOW DATABASES LIKE 'ScrapDb';" 2>&1
if ($LASTEXITCODE -ne 0 -or $dbExists -notmatch "ScrapDb") {
    Write-Host "   La base de datos no existe, se creara automaticamente con EnsureCreated" -ForegroundColor Yellow
} else {
    Write-Host "   Base de datos existe" -ForegroundColor Green
}

# Eliminar tabla de migraciones si existe para permitir EnsureCreated
Write-Host "4. Preparando base de datos para EnsureCreated..." -ForegroundColor Yellow
docker exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e "DROP TABLE IF EXISTS __EFMigrationsHistory;" 2>&1 | Out-Null
Write-Host "   Preparacion completada" -ForegroundColor Green

Write-Host ""
Write-Host "=== Base de datos lista para inicializacion ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para inicializar la base de datos y datos de prueba, ejecuta:" -ForegroundColor Yellow
Write-Host "  1. Inicia la API (dotnet run desde src/Api)" -ForegroundColor White
Write-Host "  2. Llama al endpoint: POST https://localhost:5001/api/setup/initialize" -ForegroundColor White
Write-Host ""
Write-Host "O usa Swagger en: https://localhost:5001" -ForegroundColor Cyan
Write-Host ""

