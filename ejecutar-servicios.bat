@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Ejecutando API Product, API Admin y Frontend Product GesFer
echo ========================================
echo.

REM Cambiar al directorio raiz del proyecto
cd /d "%~dp0"

REM 1. Detener procesos existentes en puertos y procesos dotnet/GesFer que bloquean DLLs
echo [1/5] Verificando y cerrando procesos existentes...

REM Script PS1 evita VariableNotWritable ($PID es variable automática de solo lectura)
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\cerrar-procesos-servicios.ps1"
echo.

REM 2. Verificar rutas
echo [2/5] Verificando rutas...
set "productApiPath=%~dp0src\Product\Back\Api"
set "adminApiPath=%~dp0src\Admin\Back\Api"
set "productFrontPath=%~dp0src\Product\Front"

if not exist "!productApiPath!\GesFer.Api.csproj" (
    echo ERROR: No se encontro el proyecto de la API Product
    pause
    exit /b 1
)

if not exist "!adminApiPath!\GesFer.Admin.Api.csproj" (
    echo ERROR: No se encontro el proyecto de la API Admin
    pause
    exit /b 1
)

if not exist "!productFrontPath!\package.json" (
    echo ERROR: No se encontro el proyecto del Frontend Product
    pause
    exit /b 1
)

echo    Rutas verificadas
echo.

REM 3. Iniciar API Product
echo [3/5] Iniciando API Product...
set "tempProductApiBat=%TEMP%\gesfer_product_api_%RANDOM%.bat"
echo @echo off > "!tempProductApiBat!"
echo cd /d "!productApiPath!" >> "!tempProductApiBat!"
echo echo Iniciando API Product GesFer... >> "!tempProductApiBat!"
echo echo. >> "!tempProductApiBat!"
echo dotnet run >> "!tempProductApiBat!"
echo pause >> "!tempProductApiBat!"
start "GesFer API Product" cmd /k "!tempProductApiBat!"
timeout /t 3 /nobreak >nul
echo    API Product iniciada en nueva ventana
echo.

REM 4. Iniciar API Admin
echo [4/5] Iniciando API Admin...
set "tempAdminApiBat=%TEMP%\gesfer_admin_api_%RANDOM%.bat"
echo @echo off > "!tempAdminApiBat!"
echo cd /d "!adminApiPath!" >> "!tempAdminApiBat!"
echo echo Iniciando API Admin GesFer... >> "!tempAdminApiBat!"
echo echo. >> "!tempAdminApiBat!"
echo dotnet run >> "!tempAdminApiBat!"
echo pause >> "!tempAdminApiBat!"
start "GesFer API Admin" cmd /k "!tempAdminApiBat!"
timeout /t 3 /nobreak >nul
echo    API Admin iniciada en nueva ventana
echo.

REM 5. Iniciar Frontend Product
echo [5/5] Iniciando Frontend Product...
set "tempProductFrontBat=%TEMP%\gesfer_product_front_%RANDOM%.bat"
echo @echo off > "!tempProductFrontBat!"
echo cd /d "!productFrontPath!" >> "!tempProductFrontBat!"
echo echo Iniciando Frontend Product GesFer... >> "!tempProductFrontBat!"
echo echo. >> "!tempProductFrontBat!"
echo npm run dev >> "!tempProductFrontBat!"
echo pause >> "!tempProductFrontBat!"
start "GesFer Frontend Product" cmd /k "!tempProductFrontBat!"
timeout /t 2 /nobreak >nul
echo    Frontend Product iniciado en nueva ventana
echo.

echo ========================================
echo Servicios iniciados
echo ========================================
echo.
echo API Product disponible en:
echo   - HTTP: http://localhost:5000
echo   - HTTPS: https://localhost:5001
echo   - Swagger: http://localhost:5000/swagger
echo.
echo API Admin disponible en:
echo   - HTTP: http://localhost:5010
echo   - HTTPS: https://localhost:5011
echo   - Swagger: http://localhost:5010/swagger
echo.
echo Frontend Product disponible en:
echo   - http://localhost:3000
echo.
echo Las ventanas de los servicios estan abiertas.
echo Cierra las ventanas para detener los servicios.
echo.
pause
