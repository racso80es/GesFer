# Script para inicializar la base de datos GesFer (Product + Admin) usando GesFer.Console
# Autor: Backend Architect
# Fecha: 2026-02-18

$ErrorActionPreference = "Stop"

Write-Host "=== Inicializacion de Base de Datos GesFer ===" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar Docker
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
try {
    docker info 2>&1 | Out-Null
    Write-Host "   Docker esta corriendo" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Docker no esta corriendo. Por favor, inicia Docker Desktop." -ForegroundColor Red
    exit 1
}

# 2. Verificar MySQL
Write-Host "2. Verificando MySQL..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$mysqlReady = $false

do {
    Start-Sleep -Seconds 2
    $attempt++
    try {
        $ping = docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword 2>&1
        if ($ping -match "alive") {
            $mysqlReady = $true
            break
        }
    } catch {
        # Ignorar error y reintentar
    }
    Write-Host "   Intento $attempt/$maxAttempts..." -ForegroundColor Gray
} while ($attempt -lt $maxAttempts)

if (-not $mysqlReady) {
    Write-Host "ERROR: MySQL no esta listo despues de $maxAttempts intentos" -ForegroundColor Red
    exit 1
}
Write-Host "   MySQL esta listo" -ForegroundColor Green

# Esperar estabilizacion
Start-Sleep -Seconds 3

# 3. Ejecutar GesFer.Console
Write-Host "3. Ejecutando GesFer.Console (Inicializacion)..." -ForegroundColor Yellow
$scriptsPath = $PSScriptRoot
$consoleProjectPath = Join-Path $scriptsPath "../../../Console/GesFer.Console.csproj"

if (-not (Test-Path $consoleProjectPath)) {
    Write-Host "ERROR: No se encontro GesFer.Console.csproj en $consoleProjectPath" -ForegroundColor Red
    exit 1
}

# Ejecutar el comando de inicialización de base de datos (--step8)
# Esto ejecuta migraciones (Admin+Product) y Seeds (Master+Admin+Demo)
try {
    dotnet run --project "$consoleProjectPath" -- --step8
} catch {
    Write-Host "ERROR: Fallo la ejecucion de GesFer.Console" -ForegroundColor Red
    exit 1
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: GesFer.Console termino con errores" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Inicializacion completada ===" -ForegroundColor Green
Write-Host ""
Write-Host "Datos de prueba disponibles:" -ForegroundColor Cyan
Write-Host "  - Usuario Admin: admin / admin123" -ForegroundColor White
Write-Host "  - Empresa Demo: Empresa Demo" -ForegroundColor White
Write-Host ""
