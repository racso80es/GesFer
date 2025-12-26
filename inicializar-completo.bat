@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Inicialización Completa GesFer
echo ========================================
echo.

REM Cambiar al directorio raíz del proyecto
cd /d "%~dp0"

REM Cambiar al directorio de la API para ejecutar docker-compose
cd /d "%~dp0Api"

REM 1. Verificar Docker
echo [1/6] Verificando Docker...
docker info >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop.
    pause
    exit /b 1
)
echo    ✓ Docker está corriendo
echo.

REM 2. Detener y eliminar contenedores existentes
echo [2/6] Limpiando contenedores existentes...
docker-compose down -v >nul 2>&1
if errorlevel 1 (
    echo    ⚠ No se pudieron detener contenedores (puede que no existan)
) else (
    echo    ✓ Contenedores eliminados
)
echo.

REM 3. Crear contenedores
echo [3/6] Creando contenedores Docker...
docker-compose up -d
if errorlevel 1 (
    echo ERROR: No se pudieron crear los contenedores
    pause
    exit /b 1
)
echo    ✓ Contenedores creados
echo.

REM 4. Esperar a que MySQL esté listo
echo [4/6] Esperando a que MySQL esté listo...
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
echo [5/6] Creando base de datos y tablas...
set "scriptsPath=%~dp0Api\scripts"
set "infrastructurePath=%~dp0Api\src\Infrastructure"
set "apiPath=%~dp0Api\src\Api"

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

REM 6. Insertar datos de prueba
echo.
echo [6/6] Insertando datos de prueba...
set "seedFile=%~dp0Api\scripts\seed-data.sql"

if exist "!seedFile!" (
    echo    Ejecutando seed-data.sql...
    type "!seedFile!" | docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb
    
    if errorlevel 1 (
        echo    ⚠ Error al insertar datos (puede que algunos ya existan)
    ) else (
        echo    ✓ Datos de prueba insertados correctamente
    )
) else (
    echo    ⚠ No se encontró el archivo seed-data.sql en scripts\
)

echo.
echo ========================================
echo Inicialización completada
echo ========================================
echo.
echo Datos de prueba creados:
echo   Empresa: Empresa Demo
echo   Usuario: admin
echo   Contraseña: admin123
echo.
echo Servicios disponibles:
echo   - MySQL: localhost:3306
echo   - Memcached: localhost:11211
echo   - Adminer: http://localhost:8080
echo.
pause

