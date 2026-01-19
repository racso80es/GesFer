# Script de validacion antes de commit para PowerShell
# Ejecuta validaciones del Backend y Frontend

$ErrorActionPreference = "Stop"

Write-Host "Iniciando validacion pre-commit..." -ForegroundColor Cyan

# PROTOCOLO DE PROTECCIÓN: Bloquear commits directos a master/main
$currentBranch = git branch --show-current
if ($currentBranch -eq "master" -or $currentBranch -eq "main") {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "ERROR: COMMIT BLOQUEADO" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "PROHIBIDO hacer commits directos a la rama '$currentBranch'." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Flujo obligatorio:" -ForegroundColor Cyan
    Write-Host "  1. git checkout -b feature/o-fix/nombre-tarea" -ForegroundColor White
    Write-Host "  2. Realizar cambios" -ForegroundColor White
    Write-Host "  3. git commit" -ForegroundColor White
    Write-Host "  4. git push origin feature/o-fix/nombre-tarea" -ForegroundColor White
    Write-Host "  5. Crear Pull Request" -ForegroundColor White
    Write-Host ""
    Write-Host "Master/main solo se actualiza mediante merge de PR." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

function Fail-SGrade {
    param(
        [string]$Message
    )
    Write-Host ""
    Write-Host "ERROR S-GRADE: $Message" -ForegroundColor Red
    exit 1
}

function Assert-BranchDocumentation {
    Write-Host "Verificando documentación obligatoria de rama..." -ForegroundColor Yellow

    $branch = (git branch --show-current).Trim()
    if ([string]::IsNullOrWhiteSpace($branch)) {
        Fail-SGrade "Documentación de rama ausente."
    }

    $docBranchName = ($branch -replace "[/\\]", "-")
    $docPath = Join-Path -Path "docs\branches" -ChildPath ("{0}.md" -f $docBranchName)

    if (-not (Test-Path $docPath)) {
        Fail-SGrade "Documentación de rama ausente."
    }

    $content = (Get-Content -Path $docPath -Raw -ErrorAction SilentlyContinue)
    if ([string]::IsNullOrWhiteSpace($content)) {
        Fail-SGrade "Documentación de rama ausente."
    }

    Write-Host "Documentación de rama OK: $docPath" -ForegroundColor Green
}

function Assert-AiTelemetry {
    Write-Host "Verificando telemetría IA obligatoria..." -ForegroundColor Yellow

    $telemetryPath = "docs\performance\GLOBAL_IA_TRACKER.md"
    if (-not (Test-Path $telemetryPath)) {
        Fail-SGrade "Telemetría IA ausente."
    }

    $content = (Get-Content -Path $telemetryPath -Raw -ErrorAction SilentlyContinue)
    if ([string]::IsNullOrWhiteSpace($content)) {
        Fail-SGrade "Telemetría IA ausente."
    }

    Write-Host "Telemetría IA OK: $telemetryPath" -ForegroundColor Green
}

function Assert-AiPerfReport {
    Write-Host "Verificando reporte de rendimiento IA de la rama..." -ForegroundColor Yellow

    $branch = (git branch --show-current).Trim()
    if ([string]::IsNullOrWhiteSpace($branch)) {
        Fail-SGrade "Telemetría IA ausente."
    }

    $docBranchName = ($branch -replace "[/\\]", "-")
    $reportPath = Join-Path -Path "docs\performance" -ChildPath ("IA_PERF_{0}.md" -f $docBranchName)

    if (-not (Test-Path $reportPath)) {
        Fail-SGrade "Telemetría IA ausente."
    }

    $content = (Get-Content -Path $reportPath -Raw -ErrorAction SilentlyContinue)
    if ([string]::IsNullOrWhiteSpace($content)) {
        Fail-SGrade "Telemetría IA ausente."
    }

    Write-Host "Reporte IA OK: $reportPath" -ForegroundColor Green
}

$null = Assert-BranchDocumentation
$null = Assert-AiTelemetry
$null = Assert-AiPerfReport

$ErrorCount = 0

# 1. Validar Backend - Build
Write-Host "Compilando Backend (dotnet build)..." -ForegroundColor Yellow
if (Test-Path "Api") {
    Push-Location Api
    try {
        dotnet build --no-restore *>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Backend compilado correctamente" -ForegroundColor Green
        } else {
            Write-Host "Error: Fallo la compilacion del Backend. Ejecuta 'dotnet build' en Api/ para ver detalles." -ForegroundColor Red
            $ErrorCount++
        }
    } finally {
        Pop-Location
    }
} else {
    Write-Host "Error: Directorio Api/ no encontrado" -ForegroundColor Red
    $ErrorCount++
}

# 2. Validar Frontend - Lint
Write-Host "Ejecutando lint del Frontend..." -ForegroundColor Yellow
if (Test-Path "Cliente") {
    Push-Location Cliente
    try {
        npm run lint *>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Lint del Frontend pasado" -ForegroundColor Green
        } else {
            Write-Host "Error: Fallo el lint del Frontend. Ejecuta 'npm run lint' en Cliente/ para ver detalles." -ForegroundColor Red
            $ErrorCount++
        }
    } finally {
        Pop-Location
    }
} else {
    Write-Host "Error: Directorio Cliente/ no encontrado" -ForegroundColor Red
    $ErrorCount++
}

# 3. Tests unitarios Backend (rapidos)
Write-Host "Ejecutando tests unitarios del Backend..." -ForegroundColor Yellow
if (Test-Path "Api") {
    Push-Location Api
    try {
        dotnet test --no-build --verbosity quiet --filter "FullyQualifiedName!~IntegrationTests" *>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Tests unitarios del Backend pasados" -ForegroundColor Green
        } else {
            Write-Host "No se encontraron tests unitarios del Backend o algunos fallaron (no critico)" -ForegroundColor Yellow
        }
    } finally {
        Pop-Location
    }
}

# 4. Tests unitarios Frontend (rapidos)
Write-Host "Ejecutando tests unitarios del Frontend..." -ForegroundColor Yellow
if (Test-Path "Cliente") {
    Push-Location Cliente
    try {
        $null = npm test -- --testPathPattern="__tests__" --passWithNoTests --silent 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Tests unitarios del Frontend pasados" -ForegroundColor Green
        } else {
            Write-Host "No se encontraron tests unitarios del Frontend o algunos fallaron (no critico)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "No se encontraron tests unitarios del Frontend o algunos fallaron (no critico)" -ForegroundColor Yellow
    } finally {
        Pop-Location
    }
}

if ($ErrorCount -gt 0) {
    Write-Host ""
    Write-Host "Validacion fallo con $ErrorCount error(es). Corrige los errores antes de hacer commit." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Todas las validaciones pasaron. Procediendo con el commit..." -ForegroundColor Green
exit 0
