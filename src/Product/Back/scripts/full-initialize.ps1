# Script completo para inicializar Docker, base de datos y datos de prueba
Write-Host "=== Inicializacion Completa GesFer ===" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar Docker
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no esta corriendo. Por favor, inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "   Docker esta corriendo" -ForegroundColor Green

# 2. Detener y eliminar contenedores existentes
Write-Host "2. Limpiando contenedores existentes..." -ForegroundColor Yellow
docker-compose down -v 2>&1 | Out-Null
Write-Host "   Contenedores eliminados" -ForegroundColor Green

# 3. Crear contenedores
Write-Host "3. Creando contenedores Docker..." -ForegroundColor Yellow
docker-compose up -d 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: No se pudieron crear los contenedores" -ForegroundColor Red
    exit 1
}
Write-Host "   Contenedores creados" -ForegroundColor Green

# 4. Esperar a que MySQL este listo
Write-Host "4. Esperando a que MySQL este listo..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$mysqlReady = $false
do {
    Start-Sleep -Seconds 2
    $attempt++
    $result = docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword 2>&1
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

# Esperar un poco mas
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "=== Docker y MySQL listos ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Ahora necesitas:" -ForegroundColor Yellow
Write-Host "  1. Iniciar la API (desde Visual Studio o con: dotnet run --project src/Api)" -ForegroundColor White
Write-Host "  2. Una vez que la API este corriendo, ejecuta:" -ForegroundColor White
Write-Host "     POST https://localhost:5001/api/setup/initialize" -ForegroundColor Cyan
Write-Host ""
Write-Host "O abre Swagger en: https://localhost:5001 y ejecuta el endpoint /api/setup/initialize" -ForegroundColor Cyan
Write-Host ""

