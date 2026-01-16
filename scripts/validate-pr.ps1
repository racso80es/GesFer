# Script de validación completa para Pull Request / Push (PowerShell)
# Ejecuta todos los tests: Backend build, Frontend unitarios, E2E
# ORQUESTA servicios automáticamente: levanta Backend y Frontend antes de tests E2E
# Limpia procesos al finalizar

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🔍 JUEZ DEL PROYECTO - Validación Completa" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorCount = 0
$BackendProcess = $null
$FrontendProcess = $null

# Función para esperar que un puerto esté disponible
function Wait-ForPort {
    param (
        [int]$Port,
        [int]$MaxAttempts = 60,
        [string]$ServiceName = "Servicio"
    )
    
    Write-Host "   Esperando que $ServiceName esté disponible en puerto $Port..." -ForegroundColor Gray
    $attempt = 0
    
    while ($attempt -lt $MaxAttempts) {
        try {
            # Intentar conexión TCP usando .NET
            $tcpClient = New-Object System.Net.Sockets.TcpClient
            $connection = $tcpClient.BeginConnect("localhost", $Port, $null, $null)
            $wait = $connection.AsyncWaitHandle.WaitOne(500, $false)
            
            if ($wait) {
                try {
                    $tcpClient.EndConnect($connection)
                    $tcpClient.Close()
                    Write-Host "   ✅ $ServiceName está disponible en puerto $Port" -ForegroundColor Green
                    return $true
                } catch {
                    $tcpClient.Close()
                }
            } else {
                $tcpClient.Close()
            }
        } catch {
            # Continuar intentando
        }
        
        Start-Sleep -Seconds 1
        $attempt++
        if ($attempt % 10 -eq 0) {
            Write-Host "   ⏳ Intento $attempt/$MaxAttempts..." -ForegroundColor Gray
        }
    }
    
    Write-Host "   ⚠️  Timeout esperando $ServiceName en puerto $Port" -ForegroundColor Yellow
    return $false
}

# Función para matar procesos en un puerto específico
function Stop-ProcessOnPort {
    param (
        [int]$Port,
        [string]$ServiceName = "Servicio"
    )
    
    try {
        $processes = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
        if ($processes) {
            foreach ($pid in $processes) {
                try {
                    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                    Write-Host "   [STOP] Proceso $pid ($ServiceName) en puerto $Port terminado" -ForegroundColor Yellow
                } catch {
                    # Ignorar si el proceso ya no existe
                }
            }
        }
    } catch {
        # Ignorar si no hay procesos
    }
}

# Función de limpieza al finalizar
function Cleanup-Services {
    Write-Host ""
    Write-Host "🧹 Limpiando servicios..." -ForegroundColor Yellow
    
    # Matar procesos de Backend y Frontend si existen
    if ($BackendProcess -and -not $BackendProcess.HasExited) {
        try {
            Stop-Process -Id $BackendProcess.Id -Force -ErrorAction SilentlyContinue
            Write-Host "   ✅ Proceso Backend terminado" -ForegroundColor Green
        } catch {
            # Ignorar
        }
    }
    
    if ($FrontendProcess -and -not $FrontendProcess.HasExited) {
        try {
            Stop-Process -Id $FrontendProcess.Id -Force -ErrorAction SilentlyContinue
            Write-Host "   ✅ Proceso Frontend terminado" -ForegroundColor Green
        } catch {
            # Ignorar
        }
    }
    
    # Matar procesos en puertos 5000 y 3000 por si acaso
    Stop-ProcessOnPort -Port 5000 -ServiceName "Backend API"
    Stop-ProcessOnPort -Port 3000 -ServiceName "Frontend Next.js"
    
    Write-Host "   ✅ Limpieza completada" -ForegroundColor Green
}

# Registrar cleanup al finalizar (éxito o error)
trap {
    Cleanup-Services
    throw
}

# 1. Validar Backend - Build
Write-Host "📦 [1/4] Compilando Backend (dotnet build)..." -ForegroundColor Yellow
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
Write-Host "🧪 [2/4] Ejecutando tests unitarios del Frontend (npm test)..." -ForegroundColor Yellow
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

# Solo continuar con tests E2E si no hay errores previos
if ($ErrorCount -eq 0) {
    # 3. Orquestación de Servicios para Tests E2E
    Write-Host "🚀 [3/4] Orquestando servicios para tests E2E..." -ForegroundColor Yellow
    
    # Limpiar puertos por si están ocupados
    Stop-ProcessOnPort -Port 5000 -ServiceName "Backend API"
    Stop-ProcessOnPort -Port 3000 -ServiceName "Frontend Next.js"
    Start-Sleep -Seconds 2
    
    # 3.1. Iniciar Backend
    Write-Host "   🔧 Iniciando Backend API en puerto 5000..." -ForegroundColor Cyan
    if (Test-Path "Api\src\Api") {
        Push-Location Api\src\Api
        try {
            # Iniciar Backend en background
            $backendStartInfo = New-Object System.Diagnostics.ProcessStartInfo
            $backendStartInfo.FileName = "dotnet"
            $backendStartInfo.Arguments = "run --urls http://localhost:5000"
            $backendStartInfo.WorkingDirectory = (Get-Location).Path
            $backendStartInfo.UseShellExecute = $false
            $backendStartInfo.RedirectStandardOutput = $true
            $backendStartInfo.RedirectStandardError = $true
            $backendStartInfo.CreateNoWindow = $true
            
            $BackendProcess = New-Object System.Diagnostics.Process
            $BackendProcess.StartInfo = $backendStartInfo
            $null = $BackendProcess.Start()
            
            Write-Host "   ✅ Proceso Backend iniciado (PID: $($BackendProcess.Id))" -ForegroundColor Green
            
            # Esperar a que el puerto 5000 esté disponible
            if (-not (Wait-ForPort -Port 5000 -MaxAttempts 90 -ServiceName "Backend API")) {
                Write-Host "   ❌ ERROR: Backend no respondió en puerto 5000" -ForegroundColor Red
                $ErrorCount++
            }
        } catch {
            Write-Host "   ❌ ERROR: No se pudo iniciar Backend: $($_.Exception.Message)" -ForegroundColor Red
            $ErrorCount++
        } finally {
            Pop-Location
        }
    } else {
        Write-Host "   ❌ ERROR: Directorio Api\src\Api no encontrado" -ForegroundColor Red
        $ErrorCount++
    }
    
    # 3.2. Iniciar Frontend (solo si Backend inició correctamente)
    if ($ErrorCount -eq 0 -and $BackendProcess -and -not $BackendProcess.HasExited) {
        Write-Host "   🔧 Iniciando Frontend Next.js en puerto 3000..." -ForegroundColor Cyan
        if (Test-Path "Cliente") {
            Push-Location Cliente
            try {
                # Iniciar Frontend en background
                $frontendStartInfo = New-Object System.Diagnostics.ProcessStartInfo
                $frontendStartInfo.FileName = "npm"
                $frontendStartInfo.Arguments = "run dev"
                $frontendStartInfo.WorkingDirectory = (Get-Location).Path
                $frontendStartInfo.UseShellExecute = $false
                $frontendStartInfo.RedirectStandardOutput = $true
                $frontendStartInfo.RedirectStandardError = $true
                $frontendStartInfo.CreateNoWindow = $true
                if (-not $frontendStartInfo.EnvironmentVariables.ContainsKey("PORT")) {
                    $frontendStartInfo.EnvironmentVariables.Add("PORT", "3000")
                } else {
                    $frontendStartInfo.EnvironmentVariables["PORT"] = "3000"
                }
                
                $FrontendProcess = New-Object System.Diagnostics.Process
                $FrontendProcess.StartInfo = $frontendStartInfo
                $null = $FrontendProcess.Start()
                
                Write-Host "   ✅ Proceso Frontend iniciado (PID: $($FrontendProcess.Id))" -ForegroundColor Green
                
                # Esperar a que el puerto 3000 esté disponible
                if (-not (Wait-ForPort -Port 3000 -MaxAttempts 120 -ServiceName "Frontend Next.js")) {
                    Write-Host "   ⚠️  ADVERTENCIA: Frontend puede tardar más en iniciarse" -ForegroundColor Yellow
                    Write-Host "   Continuando con tests E2E..." -ForegroundColor Yellow
                }
                
                # Esperar adicional para que Next.js compile
                Start-Sleep -Seconds 5
            } catch {
                Write-Host "   ❌ ERROR: No se pudo iniciar Frontend: $($_.Exception.Message)" -ForegroundColor Red
                $ErrorCount++
            } finally {
                Pop-Location
            }
        } else {
            Write-Host "   ❌ ERROR: Directorio Cliente/ no encontrado" -ForegroundColor Red
            $ErrorCount++
        }
    }
    
    Write-Host ""
    
    # 4. Tests E2E Frontend (Playwright)
    Write-Host "🎭 [4/4] Ejecutando tests E2E (Playwright)..." -ForegroundColor Yellow
    if (Test-Path "Cliente") {
        Push-Location Cliente
        try {
            # Ejecutar tests E2E
            npx playwright test
            $e2eExitCode = $LASTEXITCODE
            
            if ($e2eExitCode -eq 0) {
                Write-Host "✅ Tests E2E pasados" -ForegroundColor Green
            } else {
                Write-Host "❌ ERROR: Fallaron los tests E2E" -ForegroundColor Red
                $ErrorCount++
            }
        } catch {
            Write-Host "❌ ERROR: Error al ejecutar tests E2E" -ForegroundColor Red
            $ErrorCount++
        } finally {
            Pop-Location
        }
    } else {
        Write-Host "❌ ERROR: Directorio Cliente/ no encontrado" -ForegroundColor Red
        $ErrorCount++
    }
} else {
    Write-Host "⏭️  [3/4] Saltando tests E2E debido a errores previos" -ForegroundColor Yellow
    Write-Host "⏭️  [4/4] Saltando tests E2E debido a errores previos" -ForegroundColor Yellow
}

Write-Host ""

# Limpiar servicios antes de mostrar resultado final
Cleanup-Services

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
