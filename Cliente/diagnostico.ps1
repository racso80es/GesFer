# Script de diagnostico para GesFer Cliente
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Diagnostico del Servidor" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Node.js
Write-Host "1. Verificando Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "   OK Node.js: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "   ERROR Node.js no encontrado" -ForegroundColor Red
    exit 1
}

# Verificar npm
Write-Host "2. Verificando npm..." -ForegroundColor Yellow
try {
    $npmVersion = npm --version
    Write-Host "   OK npm: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "   ERROR npm no encontrado" -ForegroundColor Red
    exit 1
}

# Verificar puerto 3000
Write-Host "3. Verificando puerto 3000..." -ForegroundColor Yellow
$port3000 = netstat -ano | findstr ":3000" | findstr "LISTENING"
if ($port3000) {
    Write-Host "   OK Puerto 3000 esta en uso (servidor corriendo)" -ForegroundColor Green
    $port3000 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
} else {
    Write-Host "   ERROR Puerto 3000 no esta en uso" -ForegroundColor Red
    Write-Host "   El servidor no esta corriendo" -ForegroundColor Yellow
}

# Verificar puerto 5001 (API)
Write-Host "4. Verificando puerto 5001 (API)..." -ForegroundColor Yellow
$port5001 = netstat -ano | findstr ":5001" | findstr "LISTENING"
if ($port5001) {
    Write-Host "   OK Puerto 5001 esta en uso (API corriendo)" -ForegroundColor Green
} else {
    Write-Host "   ADVERTENCIA Puerto 5001 no esta en uso" -ForegroundColor Yellow
    Write-Host "   La API puede no estar ejecutandose" -ForegroundColor Yellow
}

# Verificar .env.local
Write-Host "5. Verificando configuracion..." -ForegroundColor Yellow
if (Test-Path ".env.local") {
    $envContent = Get-Content ".env.local"
    Write-Host "   OK Archivo .env.local existe" -ForegroundColor Green
    $apiUrl = ($envContent | Select-String "NEXT_PUBLIC_API_URL").ToString()
    if ($apiUrl) {
        Write-Host "   $apiUrl" -ForegroundColor Gray
    }
} else {
    Write-Host "   ERROR Archivo .env.local no existe" -ForegroundColor Red
}

# Verificar node_modules
Write-Host "6. Verificando dependencias..." -ForegroundColor Yellow
if (Test-Path "node_modules") {
    Write-Host "   OK Dependencias instaladas" -ForegroundColor Green
} else {
    Write-Host "   ERROR Dependencias no instaladas" -ForegroundColor Red
    Write-Host "   Ejecuta: npm install" -ForegroundColor Yellow
}

# Intentar conectar al servidor
Write-Host "7. Probando conexion al servidor..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:3000" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
    Write-Host "   OK Servidor respondiendo correctamente" -ForegroundColor Green
    Write-Host "   Status Code: $($response.StatusCode)" -ForegroundColor Gray
} catch {
    Write-Host "   ERROR No se pudo conectar al servidor" -ForegroundColor Red
    Write-Host "   Mensaje: $($_.Exception.Message)" -ForegroundColor Gray
}

# Verificar procesos de Node
Write-Host "8. Verificando procesos de Node.js..." -ForegroundColor Yellow
$nodeProcesses = Get-Process -Name node -ErrorAction SilentlyContinue
if ($nodeProcesses) {
    Write-Host "   OK Procesos de Node.js encontrados: $($nodeProcesses.Count)" -ForegroundColor Green
    $nodeProcesses | ForEach-Object {
        Write-Host "   PID: $($_.Id) - CPU: $([math]::Round($_.CPU, 2))s" -ForegroundColor Gray
    }
} else {
    Write-Host "   ERROR No hay procesos de Node.js ejecutandose" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Resumen" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Si el servidor no responde:" -ForegroundColor Yellow
Write-Host "  1. Verifica que el servidor este corriendo: npm run dev" -ForegroundColor White
Write-Host "  2. Intenta acceder con: http://127.0.0.1:3000" -ForegroundColor White
Write-Host "  3. Revisa el firewall de Windows" -ForegroundColor White
Write-Host "  4. Abre la consola del navegador (F12) para ver errores" -ForegroundColor White
Write-Host "  5. Limpia la cache: Ctrl+Shift+R" -ForegroundColor White
Write-Host ""

