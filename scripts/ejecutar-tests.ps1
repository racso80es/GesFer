#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Ejecutando Tests GesFer (Dockerized)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Definir Raíz del Repositorio
$RootPath = Resolve-Path "$PSScriptRoot/.."

# Rutas de validación
$FrontPackageJson = Join-Path $RootPath "src/Product/Front/package.json"
$BackTestProject = Join-Path $RootPath "src/Product/Back/IntegrationTests/GesFer.IntegrationTests.csproj"

# 1. Validación de Rutas
Write-Host "`n[1/3] Verificando rutas..." -ForegroundColor Yellow
if (-not (Test-Path $FrontPackageJson)) {
    Write-Error "❌ ERROR: No se encontró $FrontPackageJson"
}
if (-not (Test-Path $BackTestProject)) {
    Write-Error "❌ ERROR: No se encontró $BackTestProject"
}
Write-Host "✅ Rutas verificadas." -ForegroundColor Green

# 2. Tests Frontend
Write-Host "`n[2/3] Ejecutando Tests Frontend (Container)..." -ForegroundColor Yellow
$ComposeFile = Join-Path $RootPath "docker-compose.test.yml"

try {
    # Ejecuta el comando definido en docker-compose.test.yml (npm ci && npm run test:all)
    docker compose -f $ComposeFile run --rm frontend-test
    if ($LASTEXITCODE -ne 0) {
        throw "Fallaron los tests de Frontend."
    }
    Write-Host "✅ Tests Frontend Exitosos." -ForegroundColor Green
}
catch {
    Write-Error "❌ $_"
}

# 3. Tests Backend
Write-Host "`n[3/3] Ejecutando Tests Backend (Container)..." -ForegroundColor Yellow

try {
    # Ejecuta dotnet test dentro del contenedor SDK
    # Nota: Se usa la ruta relativa desde la raíz del repo (working_dir: /app)
    $TestCmd = "dotnet test src/Product/Back/IntegrationTests/GesFer.IntegrationTests.csproj --verbosity normal --logger 'console;verbosity=detailed'"

    docker compose -f $ComposeFile run --rm backend-test $TestCmd
    if ($LASTEXITCODE -ne 0) {
        throw "Fallaron los tests de Backend."
    }
    Write-Host "✅ Tests Backend Exitosos." -ForegroundColor Green
}
catch {
    Write-Error "❌ $_"
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "🎉 EJECUCIÓN FINALIZADA CON ÉXITO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
