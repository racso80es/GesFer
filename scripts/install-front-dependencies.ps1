<#
.SYNOPSIS
    Instala dependencias npm en los proyectos Front (Product y Admin).
.DESCRIPTION
    Ejecuta npm install en src/Product/Front y src/Admin/Front.
    Usar antes de la primera ejecución o si faltan node_modules (p. ej. next).
    Referencia: docs/operations/FIX_PROCEDURE_SERVICES_OBJECTIVES.md
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

$fronts = @(
    @{ Name = "ProductFront"; Path = Join-Path $projectRoot "src\Product\Front" },
    @{ Name = "AdminFront";  Path = Join-Path $projectRoot "src\Admin\Front" }
)

foreach ($front in $fronts) {
    if (-not (Test-Path (Join-Path $front.Path "package.json"))) {
        Write-Warning "No se encontró package.json en $($front.Path). Se omite."
        continue
    }
    Write-Host "[$($front.Name)] npm install en $($front.Path)" -ForegroundColor Cyan
    Push-Location $front.Path
    try {
        npm install
        if ($LASTEXITCODE -ne 0) { throw "npm install falló con código $LASTEXITCODE" }
        Write-Host "[$($front.Name)] OK" -ForegroundColor Green
    } finally {
        Pop-Location
    }
}

Write-Host "Dependencias de fronts instaladas." -ForegroundColor Green
