# Script de validación completa para Pull Request / Push (PowerShell)
# Ejecuta todos los tests: Backend build, Frontend unitarios, E2E
# Si cualquier test falla, termina con exit 1

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🔍 JUEZ DEL PROYECTO - Validación Completa" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorCount = 0

# 1. Validar Backend - Build
Write-Host "📦 [1/3] Compilando Backend (dotnet build)..." -ForegroundColor Yellow
if (Test-Path "Api") {
    Push-Location Api
    try {
        dotnet build --no-restore
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Backend compilado correctamente" -ForegroundColor Green
        } else {
            Write-Host "❌ ERROR: Falló la compilación del Backend" -ForegroundColor Red
            $ErrorCount++
        }
    } catch {
        Write-Host "❌ ERROR: Falló la compilación del Backend" -ForegroundColor Red
        $ErrorCount++
    } finally {
        Pop-Location
    }
} else {
    Write-Host "❌ ERROR: Directorio Api/ no encontrado" -ForegroundColor Red
    $ErrorCount++
}

Write-Host ""

# 2. Tests unitarios Frontend
Write-Host "🧪 [2/3] Ejecutando tests unitarios del Frontend (npm test)..." -ForegroundColor Yellow
if (Test-Path "Cliente") {
    Push-Location Cliente
    try {
        npm run test -- --passWithNoTests
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Tests unitarios del Frontend pasados" -ForegroundColor Green
        } else {
            Write-Host "❌ ERROR: Fallaron los tests unitarios del Frontend" -ForegroundColor Red
            $ErrorCount++
        }
    } catch {
        Write-Host "❌ ERROR: Fallaron los tests unitarios del Frontend" -ForegroundColor Red
        $ErrorCount++
    } finally {
        Pop-Location
    }
} else {
    Write-Host "❌ ERROR: Directorio Cliente/ no encontrado" -ForegroundColor Red
    $ErrorCount++
}

Write-Host ""

# 3. Tests E2E Frontend (Playwright)
Write-Host "🎭 [3/3] Ejecutando tests E2E (Playwright)..." -ForegroundColor Yellow
if (Test-Path "Cliente") {
    Push-Location Cliente
    try {
        npx playwright test
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Tests E2E pasados" -ForegroundColor Green
        } else {
            Write-Host "❌ ERROR: Fallaron los tests E2E" -ForegroundColor Red
            $ErrorCount++
        }
    } catch {
        Write-Host "❌ ERROR: Fallaron los tests E2E" -ForegroundColor Red
        $ErrorCount++
    } finally {
        Pop-Location
    }
} else {
    Write-Host "❌ ERROR: Directorio Cliente/ no encontrado" -ForegroundColor Red
    $ErrorCount++
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan

# Resultado final
if ($ErrorCount -eq 0) {
    Write-Host "✅ TODAS LAS VALIDACIONES PASARON" -ForegroundColor Green
    Write-Host "   El Juez del Proyecto aprueba el push." -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Cyan
    exit 0
} else {
    Write-Host "❌ VALIDACIÓN FALLÓ con $ErrorCount error(es)" -ForegroundColor Red
    Write-Host "   El Juez del Proyecto BLOQUEA el push." -ForegroundColor Red
    Write-Host "   Corrige los errores antes de intentar push nuevamente." -ForegroundColor Red
    Write-Host "==========================================" -ForegroundColor Cyan
    exit 1
}
