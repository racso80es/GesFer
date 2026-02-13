<#
.SYNOPSIS
    Ejecuta un servicio (API o Front) y persiste toda la salida en un log estructurado.
.DESCRIPTION
    Escribe en logs/services/<ServiceName>.log con formato:
    timestamp|level|service|message
    (timestamp ISO8601, level INFO|ERROR, service nombre, message línea de salida).
    Cumple docs/operations/LOGS_SERVICES_REFERENCE.md.
.PARAMETER ServiceName
    Nombre del servicio (ej: ProductApi, AdminApi, ProductFront).
.PARAMETER WorkingDir
    Directorio de trabajo (raíz del proyecto del servicio).
.PARAMETER Command
    Comando a ejecutar (ej: "dotnet run", "npm run dev").
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $ServiceName,

    [Parameter(Mandatory = $true)]
    [string] $WorkingDir,

    [Parameter(Mandatory = $true)]
    [string] $Command
)

$ErrorActionPreference = "Continue"
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$servicesLogDir = Join-Path $projectRoot (Join-Path "logs" "services")
$logFile = Join-Path $servicesLogDir "$ServiceName.log"

if (-not (Test-Path $servicesLogDir)) {
    New-Item -ItemType Directory -Path $servicesLogDir -Force | Out-Null
}

function Write-StructuredLog {
    param([string]$Level, [string]$Message)
    $ts = Get-Date -Format "o"
    $escaped = $Message -replace "`r`n", " " -replace "`n", " " -replace "`r", ""
    $line = "${ts}|${Level}|${ServiceName}|${escaped}"
    Add-Content -Path $logFile -Value $line -Encoding UTF8 -ErrorAction SilentlyContinue
}

# Encabezado de sesión en el log
Write-StructuredLog -Level "INFO" -Message "=== Sesion iniciada: $Command en $WorkingDir ==="

# En Windows, npm/npx son .cmd; ProcessStartInfo con FileName="npm" no los encuentra.
# Usar cmd /c "comando" para que el shell resuelva npm.cmd correctamente.
$parts = $Command -split "\s+", 2
$exe = $parts[0]
$exeArgs = if ($parts.Length -gt 1) { $parts[1] } else { "" }

$useCmd = $exe -match '^(npm|npx|node)$'
if ($useCmd) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "cmd.exe"
    $psi.Arguments = "/c `"$Command`""
    $psi.WorkingDirectory = $WorkingDir
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $false
    $psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $psi.StandardErrorEncoding = [System.Text.Encoding]::UTF8
} else {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $exe
    $psi.Arguments = $exeArgs
    $psi.WorkingDirectory = $WorkingDir
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $false
    $psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $psi.StandardErrorEncoding = [System.Text.Encoding]::UTF8
}

$process = New-Object System.Diagnostics.Process
$process.StartInfo = $psi

$onStdout = {
    if ($EventArgs.Data -ne $null) {
        $level = "INFO"
        $msg = $EventArgs.Data
        $ts = Get-Date -Format "o"
        $escaped = $msg -replace "`r`n", " " -replace "`n", " " -replace "`r", ""
        $line = "${ts}|${level}|$($Event.MessageData.ServiceName)|${escaped}"
        Add-Content -Path $Event.MessageData.LogFile -Value $line -Encoding UTF8 -ErrorAction SilentlyContinue
        Write-Host $msg
    }
}
$onStderr = {
    if ($EventArgs.Data -ne $null) {
        $level = "ERROR"
        $msg = $EventArgs.Data
        $ts = Get-Date -Format "o"
        $escaped = $msg -replace "`r`n", " " -replace "`n", " " -replace "`r", ""
        $line = "${ts}|${level}|$($Event.MessageData.ServiceName)|${escaped}"
        Add-Content -Path $Event.MessageData.LogFile -Value $line -Encoding UTF8 -ErrorAction SilentlyContinue
        Write-Host $msg -ForegroundColor Red
    }
}

$eventData = @{ ServiceName = $ServiceName; LogFile = $logFile }
$stdoutHandler = Register-ObjectEvent -InputObject $process -EventName OutputDataReceived -Action $onStdout -MessageData $eventData
$stderrHandler = Register-ObjectEvent -InputObject $process -EventName ErrorDataReceived -Action $onStderr -MessageData $eventData

try {
    if (-not $process.Start()) {
        Write-StructuredLog -Level "ERROR" -Message "No se pudo iniciar el proceso."
        Unregister-Event -SourceIdentifier $stdoutHandler.Name -ErrorAction SilentlyContinue
        Unregister-Event -SourceIdentifier $stderrHandler.Name -ErrorAction SilentlyContinue
        exit 1
    }
    $process.BeginOutputReadLine()
    $process.BeginErrorReadLine()
    $process.WaitForExit()
} catch {
    Write-StructuredLog -Level "ERROR" -Message "Excepcion al iniciar proceso: $($_.Exception.Message)"
    Write-Host $_.Exception.Message -ForegroundColor Red
    Unregister-Event -SourceIdentifier $stdoutHandler.Name -ErrorAction SilentlyContinue
    Unregister-Event -SourceIdentifier $stderrHandler.Name -ErrorAction SilentlyContinue
    exit 1
}

Start-Sleep -Milliseconds 200
Unregister-Event -SourceIdentifier $stdoutHandler.Name -ErrorAction SilentlyContinue
Unregister-Event -SourceIdentifier $stderrHandler.Name -ErrorAction SilentlyContinue

$exitCode = $process.ExitCode
Write-StructuredLog -Level "INFO" -Message "=== Sesion finalizada (ExitCode: $exitCode) ==="
exit $exitCode
