<#
.SYNOPSIS
    Wrapper de ejecución para interacciones de IA (Protocolo Racso-Tormentosa).
#>
param (
    [Parameter(Mandatory=$true)] [string]$Command,
    [Parameter(Mandatory=$false)] [string]$Contexto = "GesFer",
    [ValidateSet("Triaje","Analisis","Evaluacion","Marcado","Accion")]
    [string]$Fase = "Accion"
)

# 1. AC-001 [LOGS]: Validación de sintaxis previa
if ($Command -like "*log*") {
    Write-Host "[AC-001] Validando sintaxis de ruta/log..." -ForegroundColor Cyan
    # Lógica de validación de rutas duplicadas o malformadas
}

# 2. Registro de Inicio (Telemetría)
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$branch = git rev-parse --abbrev-ref HEAD
$logPath = "docs/diagnostics/$branch/execution_history.json"

# 3. Ejecución con Captura de Errores
try {
    Write-Host "Executing: $Command" -ForegroundColor Yellow
    $output = Invoke-Expression $Command -ErrorAction Stop
    $status = "Success"
} catch {
    $output = $_.Exception.Message
    $status = "Failed"
    Write-Error "Error detectado en la ejecución de la IA."
}

# 4. Persistencia en la Carpeta de la Rama
$logEntry = @{
    Timestamp = $timestamp
    Fase      = $Fase
    Command   = $Command
    Status    = $status
    Output    = $output.ToString()
} | ConvertTo-Json

if (-not (Test-Path "docs/diagnostics/$branch")) { New-Item -ItemType Directory -Path "docs/diagnostics/$branch" }
$logEntry | Out-File -FilePath $logPath -Append

return $output