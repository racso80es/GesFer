# Script para detener los servicios de Docker
Write-Host "Deteniendo servicios de Docker..." -ForegroundColor Yellow

docker-compose down

if ($LASTEXITCODE -eq 0) {
    Write-Host "Servicios detenidos correctamente." -ForegroundColor Green
} else {
    Write-Host "Error al detener los servicios." -ForegroundColor Red
    exit 1
}

