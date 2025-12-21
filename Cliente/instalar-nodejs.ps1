# Script para instalar Node.js automaticamente
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Instalador de Node.js para GesFer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Intentar instalar con winget (Windows Package Manager)
Write-Host "Intentando instalar Node.js con winget..." -ForegroundColor Yellow
try {
    $wingetCheck = winget --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK winget detectado. Instalando Node.js LTS..." -ForegroundColor Green
        winget install OpenJS.NodeJS.LTS --silent --accept-package-agreements --accept-source-agreements
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "OK Node.js instalado correctamente!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Por favor, CIERRA y REABRE esta terminal para que los cambios surtan efecto." -ForegroundColor Yellow
            Write-Host "Luego ejecuta: .\setup.ps1" -ForegroundColor White
            exit 0
        }
    }
} catch {
    Write-Host "winget no disponible" -ForegroundColor Yellow
}

# Intentar con Chocolatey
Write-Host ""
Write-Host "Intentando instalar Node.js con Chocolatey..." -ForegroundColor Yellow
try {
    $chocoCheck = choco --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK Chocolatey detectado. Instalando Node.js LTS..." -ForegroundColor Green
        choco install nodejs-lts -y
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "OK Node.js instalado correctamente!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Por favor, CIERRA y REABRE esta terminal para que los cambios surtan efecto." -ForegroundColor Yellow
            Write-Host "Luego ejecuta: .\setup.ps1" -ForegroundColor White
            exit 0
        }
    }
} catch {
    Write-Host "Chocolatey no disponible" -ForegroundColor Yellow
}

# Si no hay gestores de paquetes, proporcionar instrucciones
Write-Host ""
Write-Host "========================================" -ForegroundColor Red
Write-Host "  No se pudo instalar automaticamente" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "Por favor, instala Node.js manualmente:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Abre tu navegador y ve a: https://nodejs.org/" -ForegroundColor White
Write-Host "2. Descarga la version LTS (recomendada)" -ForegroundColor White
Write-Host "3. Ejecuta el instalador y sigue las instrucciones" -ForegroundColor White
Write-Host "4. CIERRA y REABRE esta terminal" -ForegroundColor White
Write-Host "5. Ejecuta: .\setup.ps1" -ForegroundColor White
Write-Host ""
Write-Host "O instala un gestor de paquetes:" -ForegroundColor Yellow
Write-Host "  - winget (incluido en Windows 10/11)" -ForegroundColor White
Write-Host "  - Chocolatey: https://chocolatey.org/install" -ForegroundColor White
Write-Host ""

