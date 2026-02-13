#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Cierra procesos que ocupan puertos y procesos dotnet/GesFer que bloquean DLLs.
.DESCRIPTION
    Resuelve MSB3027/MSB3021 (archivos en uso) y VariableNotWritable ($PID).
    Usa $processId en lugar de $PID (variable automática de solo lectura en PowerShell).
#>

$ErrorActionPreference = "Continue"

# Puertos usados por GesFer (API Product, API Admin, Frontend)
$ports = @(5000, 5001, 5010, 5011, 3000, 3001)

Write-Host "[1/2] Cerrando procesos en puertos $($ports -join ', ')..."
foreach ($port in $ports) {
    $connections = netstat -ano 2>$null | Select-String ":$port\s+.*LISTENING"
    foreach ($conn in $connections) {
        $parts = ($conn -split '\s+')
        $processId = $parts[-1]
        if ($processId -match '^\d+$') {
            Write-Host "  Cerrando proceso en puerto $port (PID: $processId)..."
            taskkill /PID $processId /F 2>$null
        }
    }
}

Write-Host "[2/2] Cerrando procesos dotnet que ejecutan GesFer (evita bloqueo de DLLs)..."
try {
    $gesferProcesses = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -like '*GesFer*' -and $_.ProcessId -ne $PID }
    foreach ($proc in $gesferProcesses) {
        Write-Host "  Cerrando dotnet GesFer (PID: $($proc.ProcessId))..."
        Stop-Process -Id $proc.ProcessId -Force -ErrorAction SilentlyContinue
    }
}
catch { }

# Pausa breve para que Windows libere file handles
Start-Sleep -Seconds 2
Write-Host "Verificacion completada." -ForegroundColor Green
