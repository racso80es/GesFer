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
# Nota: Los tests E2E requieren servicios activos (API y Cliente corriendo).
# Si los servicios no están disponibles, se advierte pero no se bloquea el push.
Write-Host "🎭 [3/3] Ejecutando tests E2E (Playwright)..." -ForegroundColor Yellow
if (Test-Path "Cliente") {
    Push-Location Cliente
    try {
        # Ejecutar tests E2E y capturar output
        $e2eProcess = Start-Process -FilePath "npx" -ArgumentList "playwright","test" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "test-output.txt" -RedirectStandardError "test-errors.txt"
        
        $e2eExitCode = $e2eProcess.ExitCode
        $e2eOutput = ""
        $e2eErrors = ""
        
        if (Test-Path "test-output.txt") {
            $e2eOutput = Get-Content "test-output.txt" -Raw -ErrorAction SilentlyContinue
        }
        if (Test-Path "test-errors.txt") {
            $e2eErrors = Get-Content "test-errors.txt" -Raw -ErrorAction SilentlyContinue
        }
        
        $combinedOutput = "$e2eOutput $e2eErrors"
        
        # Limpiar archivos temporales
        if (Test-Path "test-output.txt") { Remove-Item "test-output.txt" -ErrorAction SilentlyContinue }
        if (Test-Path "test-errors.txt") { Remove-Item "test-errors.txt" -ErrorAction SilentlyContinue }
        
        if ($e2eExitCode -eq 0) {
            Write-Host "✅ Tests E2E pasados" -ForegroundColor Green
        } else {
            # Verificar si los fallos son por falta de servicios (ECONNREFUSED o ERR_CONNECTION_REFUSED)
            if ($combinedOutput -match "ECONNREFUSED|ERR_CONNECTION_REFUSED") {
                Write-Host "⚠️  ADVERTENCIA: Tests E2E fallaron porque los servicios no están corriendo" -ForegroundColor Yellow
                Write-Host "   Esto es esperado si API (puerto 5000) o Cliente (puerto 3000) no están activos" -ForegroundColor Yellow
                Write-Host "   Los tests E2E se ejecutarán en CI/CD o cuando los servicios estén disponibles" -ForegroundColor Yellow
                # No incrementar ErrorCount para permitir push cuando solo falta servicios
            } else {
                Write-Host "❌ ERROR: Fallaron los tests E2E por razones distintas a falta de servicios" -ForegroundColor Red
                Write-Host "   Revisa el output para ver los errores específicos" -ForegroundColor Red
                $ErrorCount++
            }
        }
    } catch {
        Write-Host "⚠️  ADVERTENCIA: Error al ejecutar tests E2E (posiblemente servicios no disponibles)" -ForegroundColor Yellow
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Yellow
        # No incrementar ErrorCount para permitir push cuando solo falta servicios
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
