# Script para corregir el hash de la contraseña del usuario admin
# Este script actualiza el hash de la contraseña para que coincida con el correcto

Write-Host "=== Corrección de Hash de Contraseña del Usuario Admin ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker esté corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
docker ps | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Docker está corriendo" -ForegroundColor Green

# Verificar que MySQL esté disponible
Write-Host "2. Verificando MySQL..." -ForegroundColor Yellow
docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: MySQL no está disponible." -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ MySQL está disponible" -ForegroundColor Green

# Ejecutar script SQL para corregir el hash
Write-Host "3. Corrigiendo hash de contraseña..." -ForegroundColor Yellow

$scriptPath = Join-Path $PSScriptRoot "fix-admin-password.sql"
if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: No se encontró el archivo fix-admin-password.sql" -ForegroundColor Red
    exit 1
}

$result = Get-Content $scriptPath | docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✓ Hash de contraseña corregido correctamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "Verificación:" -ForegroundColor Cyan
    $result | ForEach-Object { Write-Host $_ -ForegroundColor White }
} else {
    Write-Host "   ⚠ Advertencia: Puede haber errores. Verifica los logs arriba." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Resultado:" -ForegroundColor Cyan
    $result | ForEach-Object { Write-Host $_ -ForegroundColor White }
}

Write-Host ""
Write-Host "=== Corrección completada ===" -ForegroundColor Green
Write-Host ""
Write-Host "Credenciales de prueba:" -ForegroundColor Cyan
Write-Host "  Empresa: Empresa Demo" -ForegroundColor White
Write-Host "  Usuario: admin" -ForegroundColor White
Write-Host "  Password: admin123" -ForegroundColor White
Write-Host ""
