@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Inicialización Completa GesFer
echo ========================================
echo.

REM Guardar la ruta raíz del proyecto antes de cambiar de directorio
set "ROOT_DIR=%~dp0"

REM Cambiar al directorio de la API para ejecutar docker-compose
cd /d "%ROOT_DIR%Api"

REM 1. Verificar Docker
echo [1/8] Verificando Docker...
docker info >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop.
    pause
    exit /b 1
)
echo    ✓ Docker está corriendo
echo.

REM 2. Detener y eliminar contenedores existentes
echo [2/8] Limpiando contenedores existentes...
docker-compose down -v >nul 2>&1
if errorlevel 1 (
    echo    ⚠ No se pudieron detener contenedores (puede que no existan)
) else (
    echo    ✓ Contenedores eliminados
)
echo.

REM 3. Crear contenedores
echo [3/8] Creando contenedores Docker...
docker-compose up -d
if errorlevel 1 (
    echo ERROR: No se pudieron crear los contenedores
    pause
    exit /b 1
)
echo    ✓ Contenedores creados
echo.

REM 4. Esperar a que MySQL esté listo
echo [4/8] Esperando a que MySQL esté listo...
set maxAttempts=30
set attempt=0
set mysqlReady=0

:wait_mysql
set /a attempt+=1
docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword >nul 2>&1
if errorlevel 1 (
    if !attempt! lss !maxAttempts! (
        echo    Intento !attempt!/!maxAttempts!...
        timeout /t 2 /nobreak >nul
        goto wait_mysql
    ) else (
        echo ERROR: MySQL no está listo después de !maxAttempts! intentos
        pause
        exit /b 1
    )
) else (
    set mysqlReady=1
)

if !mysqlReady! equ 1 (
    echo    ✓ MySQL está listo
    echo.
    REM Esperar un poco más para asegurar que MySQL esté completamente listo
    timeout /t 5 /nobreak >nul
)

REM 5. Crear las tablas
echo [5/8] Creando base de datos y tablas...
set "scriptsPath=%ROOT_DIR%Api\scripts"
set "infrastructurePath=%ROOT_DIR%Api\src\Infrastructure"
set "apiPath=%ROOT_DIR%Api\src\Api"

REM Intentar usar el proyecto InitDatabase para crear las tablas
if exist "!scriptsPath!\InitDatabase.csproj" (
    echo    Ejecutando InitDatabase para crear tablas...
    pushd "!scriptsPath!"
    dotnet run --project InitDatabase.csproj
    if errorlevel 1 (
        popd
        echo    ⚠ Error al ejecutar InitDatabase
        echo    Intentando con migraciones de Entity Framework...
        
        if exist "!infrastructurePath!\GesFer.Infrastructure.csproj" (
            pushd "!apiPath!"
            dotnet ef database update --project "!infrastructurePath!\GesFer.Infrastructure.csproj" >nul 2>&1
            
            if errorlevel 1 (
                echo    ⚠ No se pudieron crear las tablas automáticamente
                echo    Por favor, inicia la API y ejecuta el endpoint /api/setup/initialize
            ) else (
                echo    ✓ Tablas creadas con migraciones
            )
            popd
        ) else (
            echo    ⚠ No se encontró el proyecto de Infrastructure
            echo    Por favor, inicia la API y ejecuta el endpoint /api/setup/initialize
        )
    ) else (
        popd
        echo    ✓ Tablas creadas correctamente
    )
) else (
    echo    ⚠ No se encontró InitDatabase.csproj
    echo    Intentando con migraciones de Entity Framework...
    
    if exist "!infrastructurePath!\GesFer.Infrastructure.csproj" (
        pushd "!apiPath!"
        dotnet ef database update --project "!infrastructurePath!\GesFer.Infrastructure.csproj" >nul 2>&1
        
        if errorlevel 1 (
            echo    ⚠ No se pudieron crear las tablas automáticamente
            echo    Por favor, inicia la API y ejecuta el endpoint /api/setup/initialize
        ) else (
            echo    ✓ Tablas creadas con migraciones
        )
        popd
    ) else (
        echo    ⚠ No se encontró el proyecto de Infrastructure
        echo    Por favor, inicia la API y ejecuta el endpoint /api/setup/initialize
    )
)

REM 6. Insertar datos iniciales (maestros, muestra y prueba)
echo.
echo [6/8] Insertando datos maestros...
set "masterFile=%ROOT_DIR%Api\scripts\master-data.sql"

if exist "!masterFile!" (
    echo    Ejecutando master-data.sql...
    type "!masterFile!" | docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb >nul 2>&1
    
    if errorlevel 1 (
        echo    ⚠ Error al insertar datos maestros (puede que algunos ya existan)
    ) else (
        echo    ✓ Datos maestros insertados correctamente
    )
) else (
    echo    ⚠ No se encontró el archivo master-data.sql
    echo       Buscado en: !masterFile!
)

echo.
echo [7/8] Insertando datos de muestra...
set "sampleFile=%ROOT_DIR%Api\scripts\sample-data.sql"

if exist "!sampleFile!" (
    echo    Ejecutando sample-data.sql...
    type "!sampleFile!" | docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb >nul 2>&1
    
    if errorlevel 1 (
        echo    ⚠ Error al insertar datos de muestra (puede que algunos ya existan)
    ) else (
        echo    ✓ Datos de muestra insertados correctamente
    )
) else (
    echo    ⚠ No se encontró el archivo sample-data.sql
    echo       Buscado en: !sampleFile!
)

echo.
echo [8/8] Insertando datos de prueba...
set "testFile=%ROOT_DIR%Api\scripts\test-data.sql"

if exist "!testFile!" (
    echo    Ejecutando test-data.sql...
    type "!testFile!" | docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb >nul 2>&1
    
    if errorlevel 1 (
        echo    ⚠ Error al insertar datos de prueba (puede que algunos ya existan)
    ) else (
        echo    ✓ Datos de prueba insertados correctamente
    )
) else (
    echo    ⚠ No se encontró el archivo test-data.sql
    echo       Buscado en: !testFile!
)

echo.
echo ========================================
echo Inicialización completada
echo ========================================
echo.
echo Datos iniciales insertados:
echo   ✓ Datos maestros (idiomas, permisos, grupos)
echo   ✓ Datos de muestra (empresa, usuarios, clientes, proveedores)
echo   ✓ Datos de prueba (para tests de integración)
echo.
echo Credenciales de acceso:
echo   Empresa: Empresa Demo
echo   Usuario: admin
echo   Contraseña: admin123
echo.
echo Servicios disponibles:
echo   - MySQL: localhost:3306
echo   - Memcached: localhost:11211
echo   - Adminer: http://localhost:8080
echo.
echo Puedes probar el login en:
echo   - API: http://localhost:5000/api/auth/login
echo   - Cliente: http://localhost:3000/login
echo.
pause

