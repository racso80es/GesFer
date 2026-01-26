# Script completo de configuracion para GesFer Cliente
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configuracion Completa de GesFer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Node.js
Write-Host "Paso 1: Verificando Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK Node.js instalado: $nodeVersion" -ForegroundColor Green
        $nodeInstalled = $true
    } else {
        $nodeInstalled = $false
    }
} catch {
    $nodeInstalled = $false
}

if (-not $nodeInstalled) {
    Write-Host "ERROR Node.js no esta instalado" -ForegroundColor Red
    Write-Host ""
    Write-Host "Ejecutando instalador automatico..." -ForegroundColor Yellow
    Write-Host ""
    
    # Ejecutar script de instalacion
    & ".\instalar-nodejs.ps1"
    
    Write-Host ""
    Write-Host "Despues de instalar Node.js, ejecuta este script nuevamente." -ForegroundColor Yellow
    exit 1
}

# Verificar npm
Write-Host ""
Write-Host "Paso 2: Verificando npm..." -ForegroundColor Yellow
try {
    $npmVersion = npm --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK npm instalado: $npmVersion" -ForegroundColor Green
    } else {
        Write-Host "ERROR npm no esta disponible" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "ERROR npm no esta disponible" -ForegroundColor Red
    exit 1
}

# Instalar dependencias
Write-Host ""
Write-Host "Paso 3: Instalando dependencias..." -ForegroundColor Yellow
Write-Host ""

npm install

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR al instalar dependencias" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "OK Dependencias instaladas correctamente" -ForegroundColor Green

# Crear .env.local
Write-Host ""
Write-Host "Paso 4: Configurando variables de entorno..." -ForegroundColor Yellow

if (-not (Test-Path ".env.local")) {
    $envContent = "# URL de la API backend`nNEXT_PUBLIC_API_URL=http://localhost:5000"
    $envContent | Out-File -FilePath ".env.local" -Encoding UTF8
    Write-Host "OK Archivo .env.local creado" -ForegroundColor Green
} else {
    Write-Host "OK Archivo .env.local ya existe" -ForegroundColor Green
}

# Resumen final
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configuracion Completada!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para iniciar la aplicacion en modo desarrollo:" -ForegroundColor Yellow
Write-Host "  npm run dev" -ForegroundColor White
Write-Host ""
Write-Host "La aplicacion estara disponible en:" -ForegroundColor Yellow
Write-Host "  http://localhost:3000" -ForegroundColor White
Write-Host ""
Write-Host "Credenciales de prueba:" -ForegroundColor Yellow
Write-Host "  Empresa: Empresa Demo" -ForegroundColor White
Write-Host "  Usuario: admin" -ForegroundColor White
Write-Host "  Contrasena: admin123" -ForegroundColor White
Write-Host ""

