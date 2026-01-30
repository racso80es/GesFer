# Script para iniciar los servicios de Docker
Write-Host "Iniciando servicios de Docker para GesFer API..." -ForegroundColor Green

# Verificar si Docker está corriendo
$dockerRunning = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop." -ForegroundColor Red
    exit 1
}

# Iniciar los servicios
Write-Host "Iniciando contenedores..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nServicios iniciados correctamente!" -ForegroundColor Green
    Write-Host "`nServicios disponibles:" -ForegroundColor Cyan
    Write-Host "  - MySQL: localhost:3306" -ForegroundColor White
    Write-Host "  - Memcached: localhost:11211" -ForegroundColor White
    Write-Host "  - Adminer: http://localhost:8080" -ForegroundColor White
    Write-Host "`nEsperando a que MySQL esté listo..." -ForegroundColor Yellow
    
    # Esperar a que MySQL esté listo
    $maxAttempts = 30
    $attempt = 0
    do {
        Start-Sleep -Seconds 2
        $attempt++
        $mysqlReady = docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "MySQL está listo!" -ForegroundColor Green
            break
        }
        Write-Host "  Intento $attempt/$maxAttempts..." -ForegroundColor Gray
    } while ($attempt -lt $maxAttempts)
    
    if ($attempt -ge $maxAttempts) {
        Write-Host "ADVERTENCIA: MySQL puede tardar más en iniciarse. Verifica con: docker-compose logs db" -ForegroundColor Yellow
    }
    
    Write-Host "`nPara ver los logs: docker-compose logs -f" -ForegroundColor Cyan
    Write-Host "Para detener: docker-compose down" -ForegroundColor Cyan
} else {
    Write-Host "ERROR al iniciar los servicios. Verifica los logs con: docker-compose logs" -ForegroundColor Red
    exit 1
}

