<#
.SYNOPSIS
    Valida la ejecucion de servicios: inicia APIs y Front, hace ping a /health y revisa logs.
.DESCRIPTION
    Opcion A: Si los servicios ya estan en marcha, solo ejecuta health checks y muestra ultimas lineas de logs.
    Opcion B: Con -StartServices, inicia ProductApi y AdminApi en background, espera, hace health checks y muestra logs.
    Referencia: docs/operations/FIX_PROCEDURE_VALIDATION_RUN.md
.PARAMETER StartServices
    Si se especifica, inicia ProductApi y AdminApi en background antes de los health checks (ProductFront no se inicia por tiempo).
#>
[CmdletBinding()]
param(
    [switch] $StartServices
)

$ErrorActionPreference = "SilentlyContinue"
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$logsDir = Join-Path $projectRoot "logs\services"

$endpoints = @(
    @{ Service = "ProductApi";   Url = "http://localhost:5000/health" },
    @{ Service = "AdminApi";    Url = "http://localhost:5010/health" },
    @{ Service = "ProductFront"; Url = "http://localhost:3000" }
)

function Get-HealthStatus {
    param([string]$Url, [int]$TimeoutSec = 8)
    try {
        $r = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec $TimeoutSec
        return @{ StatusCode = $r.StatusCode; OK = $true }
    } catch {
        $code = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { $null }
        return @{ StatusCode = $code; OK = $false; Error = $_.Exception.Message }
    }
}

# Iniciar servicios en background si se solicita
if ($StartServices) {
    Write-Host "Iniciando ProductApi y AdminApi en background..." -ForegroundColor Cyan
    $productApiPath = Join-Path $projectRoot "src\Product\Back\Api"
    $adminApiPath = Join-Path $projectRoot "src\Admin\Back\Api"
    $runScript = Join-Path $PSScriptRoot "run-service-with-log.ps1"
    $jobProduct = Start-Job -ScriptBlock {
        param($script, $name, $dir, $cmd)
        & $script -ServiceName $name -WorkingDir $dir -Command $cmd
    } -ArgumentList $runScript, "ProductApi", $productApiPath, "dotnet run"
    $jobAdmin = Start-Job -ScriptBlock {
        param($script, $name, $dir, $cmd)
        & $script -ServiceName $name -WorkingDir $dir -Command $cmd
    } -ArgumentList $runScript, "AdminApi", $adminApiPath, "dotnet run"
    Write-Host "Esperando 25 segundos para que las APIs enlacen..." -ForegroundColor Yellow
    Start-Sleep -Seconds 25
}

Write-Host "`n=== Health checks ===" -ForegroundColor Green
$healthResults = @()
foreach ($ep in $endpoints) {
    $result = Get-HealthStatus -Url $ep.Url
    $healthResults += [PSCustomObject]@{
        Servicio   = $ep.Service
        Endpoint   = $ep.Url
        StatusCode = if ($result.OK) { $result.StatusCode } else { "N/A" }
        Estado     = if ($result.OK) { "OK" } else { $result.Error }
    }
    $color = if ($result.OK) { "Green" } else { "Red" }
    $msg = if ($result.OK) { "$($ep.Service): $($ep.Url) -> $($result.StatusCode)" } else { "$($ep.Service): $($ep.Url) -> Error" }
    Write-Host $msg -ForegroundColor $color
}
$healthResults | Format-Table -AutoSize

Write-Host "`n=== Ultimas lineas de logs (cola) ===" -ForegroundColor Green
foreach ($name in @("ProductApi", "AdminApi", "ProductFront")) {
    $logFile = Join-Path $logsDir "$name.log"
    if (Test-Path $logFile) {
        Write-Host "--- $name.log (ultimas 12 lineas) ---" -ForegroundColor Cyan
        Get-Content $logFile -Tail 12 -ErrorAction SilentlyContinue
        Write-Host ""
    }
}

if ($StartServices) {
    Write-Host "Los trabajos de ProductApi y AdminApi siguen en ejecucion. Para detenerlos: Get-Job | Stop-Job; Get-Job | Remove-Job" -ForegroundColor Yellow
}

return $healthResults
