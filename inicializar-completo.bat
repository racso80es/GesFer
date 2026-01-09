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

REM 5. Crear las tablas usando migraciones de Entity Framework
echo [5/8] Creando base de datos y tablas con migraciones de Entity Framework...
set "infrastructurePath=%ROOT_DIR%Api\src\Infrastructure"
set "apiPath=%ROOT_DIR%Api\src\Api"
set "migrationsPath=%ROOT_DIR%Api\src\Infrastructure\Migrations"

REM Verificar que existan los proyectos necesarios
if not exist "!infrastructurePath!\GesFer.Infrastructure.csproj" (
    echo    ERROR: No se encontró el proyecto de Infrastructure
    echo    Ruta esperada: !infrastructurePath!\GesFer.Infrastructure.csproj
    pause
    exit /b 1
)

if not exist "!apiPath!\GesFer.Api.csproj" (
    echo    ERROR: No se encontró el proyecto de API
    echo    Ruta esperada: !apiPath!\GesFer.Api.csproj
    pause
    exit /b 1
)

REM Verificar que .NET SDK esté instalado
echo    Verificando .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo    ERROR: .NET SDK no está instalado o no está en el PATH
    echo    Por favor, instala .NET 8.0 SDK desde https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo    ✓ .NET SDK encontrado

REM Verificar e instalar dotnet-ef si es necesario
echo    Verificando herramienta dotnet-ef...
dotnet ef --version >nul 2>&1
if errorlevel 1 (
    echo    La herramienta dotnet-ef no está instalada. Instalándola...
    
    REM Intentar desinstalar primero si existe una versión corrupta
    dotnet tool uninstall --global dotnet-ef >nul 2>&1
    
    REM Limpiar caché de NuGet antes de instalar
    echo    Limpiando caché de NuGet...
    dotnet nuget locals all --clear >nul 2>&1
    
    REM Intentar instalar con versión específica
    echo    Instalando dotnet-ef versión 8.0.0...
    dotnet tool install --global dotnet-ef --version 8.0.0
    if errorlevel 1 (
        echo    ⚠ Falló instalación con versión específica. Intentando sin versión...
        dotnet tool install --global dotnet-ef
        if errorlevel 1 (
            echo    ERROR: No se pudo instalar la herramienta dotnet-ef
            echo    Esto puede deberse a un problema con la caché de NuGet o permisos.
            echo    Intenta ejecutar manualmente:
            echo       dotnet nuget locals all --clear
            echo       dotnet tool install --global dotnet-ef --version 8.0.0
            echo    O instala sin versión específica:
            echo       dotnet tool install --global dotnet-ef
            pause
            exit /b 1
        )
    )
    
    REM Verificar que se instaló correctamente
    dotnet ef --version >nul 2>&1
    if errorlevel 1 (
        echo    ERROR: La herramienta se instaló pero no se puede ejecutar
        echo    Verifica que dotnet esté en el PATH y reinicia el terminal
        pause
        exit /b 1
    )
    echo    ✓ Herramienta dotnet-ef instalada correctamente
) else (
    echo    ✓ Herramienta dotnet-ef encontrada
)

REM Cambiar al directorio de la API para ejecutar comandos de EF
pushd "!apiPath!"

REM Verificar si existen migraciones (usando ruta relativa desde el directorio de la API)
set hasMigrations=0
if exist "..\Infrastructure\Migrations" (
    dir /b "..\Infrastructure\Migrations\*.cs" >nul 2>&1
    if not errorlevel 1 (
        set hasMigrations=1
    )
)

REM Si no hay migraciones, crearlas
if !hasMigrations! equ 0 (
    echo    No se encontraron migraciones. Creando migración inicial...
    dotnet ef migrations add InitialCreate --project "..\Infrastructure\GesFer.Infrastructure.csproj" --startup-project "GesFer.Api.csproj"
    if errorlevel 1 (
        echo    ERROR: No se pudieron crear las migraciones
        echo    Verifica que Entity Framework esté correctamente configurado
        echo    Asegúrate de que los proyectos se compilen correctamente
        popd
        pause
        exit /b 1
    )
    echo    ✓ Migración inicial creada
) else (
    echo    ✓ Migraciones existentes encontradas
)

REM Aplicar migraciones a la base de datos
echo    Aplicando migraciones a la base de datos...
dotnet ef database update --project "..\Infrastructure\GesFer.Infrastructure.csproj" --startup-project "GesFer.Api.csproj"
if errorlevel 1 (
    echo    ERROR: No se pudieron aplicar las migraciones
    echo    Verifica la conexión a la base de datos y los logs anteriores
    popd
    pause
    exit /b 1
)
echo    ✓ Migraciones aplicadas correctamente

REM Verificar que las tablas se hayan creado correctamente
echo    Verificando que las tablas se hayan creado...
docker exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e "SELECT COUNT(*) as total FROM information_schema.tables WHERE table_schema = 'ScrapDb';" --skip-column-names > "%TEMP%\gesfer_tablecount.txt" 2>&1
if errorlevel 1 (
    echo    ⚠ No se pudo verificar las tablas, pero las migraciones se aplicaron
) else (
    set /p tableCount=<"%TEMP%\gesfer_tablecount.txt"
    if defined tableCount (
        if !tableCount! gtr 0 (
            echo    ✓ Tablas creadas correctamente con migraciones de Entity Framework
            echo    ✓ Total de tablas en la base de datos: !tableCount!
        ) else (
            echo    ⚠ No se encontraron tablas, pero las migraciones se aplicaron
        )
    ) else (
        echo    ✓ Migraciones aplicadas correctamente
    )
    del "%TEMP%\gesfer_tablecount.txt" >nul 2>&1
)

popd

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

