# Script para verificar el estado del servidor
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Verificacion del Servidor" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar procesos
Write-Host "1. Procesos de Node.js:" -ForegroundColor Yellow
$nodeProcesses = Get-Process -Name node -ErrorAction SilentlyContinue
if ($nodeProcesses) {
    Write-Host "   OK $($nodeProcesses.Count) proceso(s) encontrado(s)" -ForegroundColor Green
    $nodeProcesses | ForEach-Object {
        Write-Host "   PID: $($_.Id) - CPU: $([math]::Round($_.CPU, 2))s" -ForegroundColor Gray
    }
} else {
    Write-Host "   ERROR No hay procesos de Node.js" -ForegroundColor Red
}

# Verificar puerto
Write-Host "`n2. Puerto 3000:" -ForegroundColor Yellow
$port3000 = netstat -ano | findstr ":3000" | findstr "LISTENING"
if ($port3000) {
    Write-Host "   OK Puerto 3000 esta en uso" -ForegroundColor Green
    $port3000 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
} else {
    Write-Host "   ERROR Puerto 3000 no esta en uso" -ForegroundColor Red
    Write-Host "   El servidor no esta escuchando" -ForegroundColor Yellow
}

# Verificar archivos
Write-Host "`n3. Archivos necesarios:" -ForegroundColor Yellow
$files = @("package.json", "next.config.js", "app/layout.tsx", "app/page.tsx", "app/globals.css")
$allExist = $true
foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "   OK $file" -ForegroundColor Green
    } else {
        Write-Host "   ERROR $file no existe" -ForegroundColor Red
        $allExist = $false
    }
}

# Intentar conectar
Write-Host "`n4. Conexion al servidor:" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:3000" -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
    Write-Host "   OK Servidor responde (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   ERROR No se puede conectar" -ForegroundColor Red
    Write-Host "   Mensaje: $($_.Exception.Message)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Recomendaciones" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not $port3000) {
    Write-Host "El servidor no esta corriendo. Ejecuta:" -ForegroundColor Yellow
    Write-Host "  npm run dev" -ForegroundColor White
    Write-Host ""
    Write-Host "Y espera a ver 'Ready in X.Xs' en la terminal" -ForegroundColor Gray
} else {
    Write-Host "El servidor deberia estar funcionando." -ForegroundColor Green
    Write-Host "Abre: http://localhost:3000" -ForegroundColor Cyan
}

Write-Host ""

