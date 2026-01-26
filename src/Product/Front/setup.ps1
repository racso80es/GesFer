# Script de configuracion para GesFer Cliente
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configuracion de GesFer Cliente" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Node.js
Write-Host "Verificando Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "OK Node.js instalado: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR Node.js no esta instalado" -ForegroundColor Red
    Write-Host "  Por favor, instala Node.js desde https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

# Verificar npm
Write-Host "Verificando npm..." -ForegroundColor Yellow
try {
    $npmVersion = npm --version
    Write-Host "OK npm instalado: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR npm no esta disponible" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Instalando dependencias..." -ForegroundColor Yellow
Write-Host ""

# Instalar dependencias
npm install

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "OK Dependencias instaladas correctamente" -ForegroundColor Green
    Write-Host ""
    
    # Crear .env.local si no existe
    if (-not (Test-Path ".env.local")) {
        Write-Host "Creando archivo .env.local..." -ForegroundColor Yellow
        $envContent = "# URL de la API backend`nNEXT_PUBLIC_API_URL=http://localhost:5001"
        $envContent | Out-File -FilePath ".env.local" -Encoding UTF8
        Write-Host "OK Archivo .env.local creado" -ForegroundColor Green
    } else {
        Write-Host "OK Archivo .env.local ya existe" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Configuracion completada!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Para iniciar la aplicacion en modo desarrollo:" -ForegroundColor Yellow
    Write-Host "  npm run dev" -ForegroundColor White
    Write-Host ""
    Write-Host "La aplicacion estara disponible en:" -ForegroundColor Yellow
    Write-Host "  http://localhost:3000" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "ERROR al instalar dependencias" -ForegroundColor Red
    exit 1
}
