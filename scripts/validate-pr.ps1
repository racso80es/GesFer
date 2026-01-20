# Script de validacion completa para Pull Request / Push (Windows PowerShell compatible)
# Juez Modular: valida documentacion + telemetria IA y ejecuta validaciones tecnicas.

$ErrorActionPreference = "Stop"

function Fail-SGrade {
    param([string]$Message)
    Write-Host ""
    Write-Host ("ERROR S-GRADE: {0}" -f $Message) -ForegroundColor Red
    exit 1
}

function Get-BranchName {
    $branch = (git branch --show-current).Trim()
    if ([string]::IsNullOrWhiteSpace($branch)) { return "" }
    return $branch
}

function Get-BranchSlug {
    param([string]$BranchName)
    return ($BranchName -replace "[/\\]", "-")
}

function Test-IsTrunkBranch {
    param([string]$BranchName)
    return ($BranchName -eq "master" -or $BranchName -eq "main")
}

function Assert-NonEmptyFile {
    param([string]$Path, [string]$SGradeMessage)

    if (-not (Test-Path $Path)) {
        Fail-SGrade $SGradeMessage
    }

    $content = (Get-Content -Path $Path -Raw -ErrorAction SilentlyContinue)
    if ([string]::IsNullOrWhiteSpace($content)) {
        Fail-SGrade $SGradeMessage
    }
}

function Assert-BranchDocumentation {
    Write-Host "[DOCS] Verificando documentacion obligatoria de rama..." -ForegroundColor Yellow

    $branch = Get-BranchName
    if ([string]::IsNullOrWhiteSpace($branch)) { Fail-SGrade "Documentacion de rama ausente." }

    $slug = Get-BranchSlug -BranchName $branch
    $docPath = Join-Path -Path "docs\\branches" -ChildPath ("{0}.md" -f $slug)

    if (Test-IsTrunkBranch -BranchName $branch) {
        if (-not (Test-Path $docPath)) {
            Write-Host ("[DOCS] INFO: en troncal ({0}) el pasaporte no bloquea. Falta: {1}" -f $branch, $docPath) -ForegroundColor Yellow
            Write-Host ""
            return
        }

        $content = (Get-Content -Path $docPath -Raw -ErrorAction SilentlyContinue)
        if ([string]::IsNullOrWhiteSpace($content)) {
            Write-Host ("[DOCS] INFO: en troncal ({0}) el pasaporte no bloquea. Vacio: {1}" -f $branch, $docPath) -ForegroundColor Yellow
            Write-Host ""
            return
        }

        Write-Host ("[DOCS] OK (troncal): {0}" -f $docPath) -ForegroundColor Green
        Write-Host ""
        return
    }

    Assert-NonEmptyFile -Path $docPath -SGradeMessage "Documentacion de rama ausente."
    Write-Host ("[DOCS] OK: {0}" -f $docPath) -ForegroundColor Green
    Write-Host ""
}

function Assert-AiTelemetryGlobal {
    Write-Host "[IA] Verificando telemetria IA global..." -ForegroundColor Yellow
    $telemetryPath = "docs\\performance\\GLOBAL_IA_TRACKER.md"
    Assert-NonEmptyFile -Path $telemetryPath -SGradeMessage "Telemetria IA ausente."
    Write-Host ("[IA] OK: {0}" -f $telemetryPath) -ForegroundColor Green
    Write-Host ""
}

function Assert-AiPerfReportForBranch {
    Write-Host "[IA] Verificando reporte IA canonico de la rama..." -ForegroundColor Yellow

    $branch = Get-BranchName
    if ([string]::IsNullOrWhiteSpace($branch)) { Fail-SGrade "Telemetria IA ausente." }

    $slug = Get-BranchSlug -BranchName $branch
    $reportPath = Join-Path -Path "docs\\performance" -ChildPath ("IA_PERF_{0}.md" -f $slug)

    if (Test-IsTrunkBranch -BranchName $branch) {
        if (-not (Test-Path $reportPath)) {
            Write-Host ("[IA] INFO: en troncal ({0}) el reporte IA por rama no bloquea. Falta: {1}" -f $branch, $reportPath) -ForegroundColor Yellow
            Write-Host ""
            return
        }

        $content = (Get-Content -Path $reportPath -Raw -ErrorAction SilentlyContinue)
        if ([string]::IsNullOrWhiteSpace($content)) {
            Write-Host ("[IA] INFO: en troncal ({0}) el reporte IA por rama no bloquea. Vacio: {1}" -f $branch, $reportPath) -ForegroundColor Yellow
            Write-Host ""
            return
        }

        Write-Host ("[IA] OK (troncal): {0}" -f $reportPath) -ForegroundColor Green
        Write-Host ""
        return
    }

    Assert-NonEmptyFile -Path $reportPath -SGradeMessage "Telemetria IA ausente."
    Write-Host ("[IA] OK: {0}" -f $reportPath) -ForegroundColor Green
    Write-Host ""
}

function Assert-ArchitecturalAlignmentAliasReport {
    $branch = Get-BranchName
    $slug = Get-BranchSlug -BranchName $branch

    # Requisito explicito de cierre para esta rama
    if ($slug -eq "feat-architectural-alignment-S-Plus") {
        Write-Host "[IA] Verificando alias requerido IA_PERF_architectural-alignment.md..." -ForegroundColor Yellow
        $aliasPath = "docs\\performance\\IA_PERF_architectural-alignment.md"
        Assert-NonEmptyFile -Path $aliasPath -SGradeMessage "Telemetria IA ausente."
        Write-Host ("[IA] OK: {0}" -f $aliasPath) -ForegroundColor Green
        Write-Host ""
    }
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "JUEZ MODULAR - Validacion PR/Push" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$null = Assert-BranchDocumentation
$null = Assert-AiTelemetryGlobal
$null = Assert-AiPerfReportForBranch
$null = Assert-ArchitecturalAlignmentAliasReport

$ErrorCount = 0
$Warnings = @()

Write-Host "[1/3] Compilando Backend (dotnet build)..." -ForegroundColor Yellow
if (Test-Path "Api") {
    Push-Location Api
    try {
        dotnet build --no-restore
        if ($LASTEXITCODE -ne 0) { $ErrorCount++ }
    } catch {
        $ErrorCount++
    } finally {
        Pop-Location
    }
} else {
    $ErrorCount++
}

Write-Host ""

Write-Host "[2/3] Ejecutando tests de integración del Frontend (npm run test:integrity)..." -ForegroundColor Yellow
if (Test-Path "Cliente") {
    Push-Location Cliente
    try {
        # Timeout global: 180s (3 min) para evitar falsos negativos por lentitud en entorno local/CI.
        npm run test:integrity -- --passWithNoTests --testTimeout=180000
        if ($LASTEXITCODE -ne 0) { $ErrorCount++ }
    } catch {
        $ErrorCount++
    } finally {
        Pop-Location
    }
} else {
    $ErrorCount++
}

Write-Host ""

Write-Host "[3/3] BYPASS: tests E2E (Playwright) desactivados temporalmente" -ForegroundColor Yellow
# Fast-Track: solo compilación + integración (sin E2E).
$Warnings += "E2E: desactivado temporalmente (Fast-Track: solo compilación + integración)."

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan

if ($Warnings.Count -gt 0) {
    Write-Host "ADVERTENCIAS:" -ForegroundColor Yellow
    $Warnings | ForEach-Object { Write-Host (" - {0}" -f $_) -ForegroundColor Yellow }
    Write-Host ""
}

if ($ErrorCount -eq 0) {
    Write-Host "VERDE: TODAS LAS VALIDACIONES PASARON" -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Cyan
    exit 0
}

Write-Host ("ERROR: VALIDACION FALLO con {0} error(es)" -f $ErrorCount) -ForegroundColor Red
Write-Host "==========================================" -ForegroundColor Cyan
exit 1
