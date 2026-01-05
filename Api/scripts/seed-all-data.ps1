# Script para insertar todos los datos iniciales organizados
# Ejecuta los scripts en el orden correcto:
# 1. master-data.sql (datos maestros)
# 2. sample-data.sql (datos de muestra)
# 3. test-data.sql (datos de prueba, opcional)

param(
    [switch]$IncludeTestData = $false,
    [switch]$OnlyMaster = $false,
    [switch]$OnlySample = $false
)

Write-Host "=== Insertando Datos Iniciales en GesFer ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker esté corriendo
Write-Host "1. Verificando Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop." -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Docker está corriendo" -ForegroundColor Green

# Verificar que MySQL esté listo
Write-Host "2. Verificando MySQL..." -ForegroundColor Yellow
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

if (-not $mysqlReady) {
    Write-Host "ERROR: MySQL no está listo después de $maxAttempts intentos" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ MySQL está listo" -ForegroundColor Green

# Función para ejecutar un script SQL
function Execute-SqlScript {
    param(
        [string]$ScriptName,
        [string]$Description
    )
    
    $scriptPath = Join-Path $PSScriptRoot $ScriptName
    if (-not (Test-Path $scriptPath)) {
        Write-Host "   ⚠ ADVERTENCIA: No se encontró el archivo $ScriptName" -ForegroundColor Yellow
        Write-Host "     Buscado en: $scriptPath" -ForegroundColor Gray
        return $false
    }
    
    Write-Host "   Ejecutando $Description..." -ForegroundColor Yellow
    
    # Copiar el archivo al contenedor y ejecutarlo
    docker cp $scriptPath gesfer_api_db:/tmp/$ScriptName 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "     ✗ Error al copiar el archivo al contenedor" -ForegroundColor Red
        return $false
    }
    
    # Ejecutar el script
    $result = docker exec gesfer_api_db sh -c "mysql -u scrapuser -pscrappassword ScrapDb < /tmp/$ScriptName" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "     ✓ Script ejecutado correctamente" -ForegroundColor Green
        return $true
    } else {
        # Verificar si son errores de duplicados (que son normales si ya existen)
        if ($result -match "Duplicate entry") {
            Write-Host "     ⚠ Algunos datos ya existen (esto es normal si ejecutas el script varias veces)" -ForegroundColor Yellow
            return $true
        } else {
            Write-Host "     ⚠ Advertencia: Puede haber errores. Verifica los logs arriba." -ForegroundColor Yellow
            Write-Host "     Resultado: $result" -ForegroundColor Gray
            return $false
        }
    }
}

# Ejecutar scripts según los parámetros
$scriptsExecuted = @()

if ($OnlyMaster) {
    Write-Host "3. Insertando datos maestros..." -ForegroundColor Cyan
    if (Execute-SqlScript "master-data.sql" "Datos maestros") {
        $scriptsExecuted += "master-data.sql"
    }
} elseif ($OnlySample) {
    Write-Host "3. Insertando datos de muestra..." -ForegroundColor Cyan
    if (Execute-SqlScript "sample-data.sql" "Datos de muestra") {
        $scriptsExecuted += "sample-data.sql"
    }
} else {
    # Ejecutar en orden: master -> sample -> test (opcional)
    Write-Host "3. Insertando datos maestros..." -ForegroundColor Cyan
    if (Execute-SqlScript "master-data.sql" "Datos maestros") {
        $scriptsExecuted += "master-data.sql"
    }
    
    Write-Host "4. Insertando datos de muestra..." -ForegroundColor Cyan
    if (Execute-SqlScript "sample-data.sql" "Datos de muestra") {
        $scriptsExecuted += "sample-data.sql"
    }
    
    if ($IncludeTestData) {
        Write-Host "5. Insertando datos de prueba..." -ForegroundColor Cyan
        if (Execute-SqlScript "test-data.sql" "Datos de prueba") {
            $scriptsExecuted += "test-data.sql"
        }
    }
}

# Verificar que los datos se insertaron
Write-Host ""
Write-Host "6. Verificando datos insertados..." -ForegroundColor Yellow
$companyCheck = docker exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e 'SELECT COUNT(*) FROM Companies WHERE Name = "Empresa Demo" AND IsActive = 1;' 2>&1
$userCheck = docker exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e 'SELECT COUNT(*) FROM Users WHERE Username = "admin" AND IsActive = 1;' 2>&1
$languageCheck = docker exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e 'SELECT COUNT(*) FROM Languages WHERE IsActive = 1;' 2>&1

$companyCount = ($companyCheck | Select-String -Pattern '^\d+').Matches.Value
$userCount = ($userCheck | Select-String -Pattern '^\d+').Matches.Value
$languageCount = ($languageCheck | Select-String -Pattern '^\d+').Matches.Value

Write-Host "   Resultados:" -ForegroundColor White
Write-Host "     - Idiomas: $languageCount encontrado(s)" -ForegroundColor White
Write-Host "     - Empresa Demo: $companyCount encontrada(s)" -ForegroundColor White
Write-Host "     - Usuario admin: $userCount encontrado(s)" -ForegroundColor White

if ($companyCount -eq "1" -and $userCount -eq "1" -and $languageCount -ge "3") {
    Write-Host "   ✓ Datos verificados correctamente" -ForegroundColor Green
} else {
    Write-Host "   ⚠ Advertencia: Algunos datos no se encontraron" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Datos Iniciales Insertados ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Scripts ejecutados:" -ForegroundColor Yellow
foreach ($script in $scriptsExecuted) {
    Write-Host "  ✓ $script" -ForegroundColor Green
}

Write-Host ""
Write-Host "Credenciales de acceso:" -ForegroundColor Yellow
Write-Host "  Empresa: Empresa Demo" -ForegroundColor White
Write-Host "  Usuario: admin" -ForegroundColor White
Write-Host "  Contraseña: admin123" -ForegroundColor White
Write-Host ""
Write-Host "Puedes probar el login en:" -ForegroundColor Cyan
Write-Host "  http://localhost:5000/api/auth/login" -ForegroundColor White
Write-Host "  http://localhost:3000/login" -ForegroundColor White
Write-Host ""

