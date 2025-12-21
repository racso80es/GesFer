# Script para reiniciar el servidor limpiamente
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Reiniciando Servidor GesFer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Detener todos los procesos de Node.js
Write-Host "1. Deteniendo procesos de Node.js..." -ForegroundColor Yellow
$nodeProcesses = Get-Process -Name node -ErrorAction SilentlyContinue
if ($nodeProcesses) {
    $nodeProcesses | Stop-Process -Force
    Write-Host "   OK $($nodeProcesses.Count) proceso(s) detenido(s)" -ForegroundColor Green
    Start-Sleep -Seconds 2
} else {
    Write-Host "   OK No hay procesos de Node.js corriendo" -ForegroundColor Green
}

# Limpiar cache de Next.js
Write-Host "2. Limpiando cache de Next.js..." -ForegroundColor Yellow
if (Test-Path ".next") {
    Remove-Item -Recurse -Force .next -ErrorAction SilentlyContinue
    Write-Host "   OK Cache limpiado" -ForegroundColor Green
} else {
    Write-Host "   OK No hay cache para limpiar" -ForegroundColor Green
}

# Verificar que estamos en el directorio correcto
Write-Host "3. Verificando directorio..." -ForegroundColor Yellow
if (-not (Test-Path "package.json")) {
    Write-Host "   ERROR No se encuentra package.json" -ForegroundColor Red
    Write-Host "   Asegurate de estar en la carpeta Cliente" -ForegroundColor Yellow
    exit 1
}
Write-Host "   OK Directorio correcto" -ForegroundColor Green

# Verificar dependencias
Write-Host "4. Verificando dependencias..." -ForegroundColor Yellow
if (-not (Test-Path "node_modules")) {
    Write-Host "   ERROR Dependencias no instaladas" -ForegroundColor Red
    Write-Host "   Ejecutando npm install..." -ForegroundColor Yellow
    npm install
} else {
    Write-Host "   OK Dependencias instaladas" -ForegroundColor Green
}

# Verificar .env.local
Write-Host "5. Verificando configuracion..." -ForegroundColor Yellow
if (-not (Test-Path ".env.local")) {
    Write-Host "   Creando .env.local..." -ForegroundColor Yellow
    "# URL de la API backend`nNEXT_PUBLIC_API_URL=http://localhost:5001" | Out-File -FilePath ".env.local" -Encoding UTF8
    Write-Host "   OK Archivo .env.local creado" -ForegroundColor Green
} else {
    Write-Host "   OK Archivo .env.local existe" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Iniciando servidor..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "El servidor se iniciara en unos segundos." -ForegroundColor Yellow
Write-Host "Espera a ver el mensaje 'Ready in X.Xs'" -ForegroundColor Yellow
Write-Host "Luego abre: http://localhost:3000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Presiona Ctrl+C para detener el servidor" -ForegroundColor Gray
Write-Host ""

# Iniciar el servidor
npm run dev

