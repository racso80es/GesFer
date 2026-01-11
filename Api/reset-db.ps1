# Script para resetear completamente la base de datos MySQL
# Este script detiene y elimina los contenedores Docker, elimina volúmenes y datos persistentes,
# y reinicia todo desde cero para un desarrollo limpio

Write-Host "=== Reset Completo de Base de Datos MySQL ===" -ForegroundColor Cyan
Write-Host ""

# 1. Detener y eliminar contenedores y volúmenes
Write-Host "1. Deteniendo y eliminando contenedores Docker..." -ForegroundColor Yellow
docker-compose down -v
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Contenedores y volúmenes eliminados" -ForegroundColor Green
} else {
    Write-Host "   ⚠ Advertencia: Algunos recursos no se pudieron eliminar" -ForegroundColor Yellow
}

# 2. Eliminar directorio de datos MySQL si existe (bind mount residual)
Write-Host "2. Eliminando datos persistentes de MySQL..." -ForegroundColor Yellow
$mysqlDataPath = Join-Path $PSScriptRoot "docker_data\mysql"
if (Test-Path $mysqlDataPath) {
    try {
        Remove-Item -Path $mysqlDataPath -Recurse -Force -ErrorAction Stop
        Write-Host "   ✓ Directorio docker_data/mysql eliminado" -ForegroundColor Green
    } catch {
        Write-Host "   ⚠ No se pudo eliminar el directorio (puede estar en uso)" -ForegroundColor Yellow
        Write-Host "   Intenta cerrar cualquier herramienta que esté usando MySQL" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✓ El directorio docker_data/mysql no existe (ya está limpio)" -ForegroundColor Green
}

# 3. Limpiar volúmenes huérfanos
Write-Host "3. Limpiando volúmenes Docker huérfanos..." -ForegroundColor Yellow
docker volume prune -f | Out-Null
Write-Host "   ✓ Volúmenes huérfanos eliminados" -ForegroundColor Green

# 4. Iniciar contenedores nuevamente
Write-Host "4. Iniciando contenedores Docker..." -ForegroundColor Yellow
docker-compose up -d
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Contenedores iniciados" -ForegroundColor Green
} else {
    Write-Host "   ✗ Error al iniciar contenedores" -ForegroundColor Red
    exit 1
}

# 5. Esperar a que MySQL esté listo
Write-Host "5. Esperando a que MySQL esté listo..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
$mysqlReady = $false

do {
    Start-Sleep -Seconds 2
    $attempt++
    $result = docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword 2>&1
    if ($LASTEXITCODE -eq 0) {
        $mysqlReady = $true
        break
    }
    Write-Host "   Intento $attempt/$maxAttempts..." -ForegroundColor Gray
} while ($attempt -lt $maxAttempts)

if ($mysqlReady) {
    Write-Host "   ✓ MySQL está listo" -ForegroundColor Green
} else {
    Write-Host "   ✗ MySQL no está disponible después de $maxAttempts intentos" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Reset Completado ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Base de datos MySQL reiniciada y lista para usar." -ForegroundColor Green
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Yellow
Write-Host "  1. Inicia la API (F5 en Visual Studio o: dotnet run --project src/Api)" -ForegroundColor White
Write-Host "  2. Ejecuta el endpoint de inicialización:" -ForegroundColor White
Write-Host "     POST http://localhost:5000/api/setup/initialize" -ForegroundColor White
Write-Host "     O espera a que las migraciones se ejecuten automáticamente" -ForegroundColor White
Write-Host ""