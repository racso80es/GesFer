@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Ejecutando API Product, API Admin y Frontend Product GesFer
echo ========================================
echo.

REM Cambiar al directorio raiz del proyecto
cd /d "%~dp0"

REM Crear directorio de logs para persistencia estructurada (docs/operations/LOGS_SERVICES_REFERENCE.md)
if not exist "logs\services" mkdir "logs\services"
set "scriptsPath=%~dp0scripts"

REM 1. Detener procesos existentes en puertos y procesos dotnet/GesFer que bloquean DLLs
echo [1/5] Verificando y cerrando procesos existentes...

REM Script PS1 evita VariableNotWritable ($PID es variable automática de solo lectura)
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\cerrar-procesos-servicios.ps1"
echo.

REM 1b. Si falta el ejecutable de Next en Product Front, instalar dependencias (docs/operations/FIX_PROCEDURE_SERVICES_OBJECTIVES.md)
set "productFrontPath=%~dp0src\Product\Front"
if not exist "!productFrontPath!\node_modules\next\dist\bin\next" (
    echo [1b/5] Instalando dependencias en Fronts \(npm install\)...
    powershell -ExecutionPolicy Bypass -File "%~dp0scripts\install-front-dependencies.ps1"
    echo.
)

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

REM 3. Iniciar API Product (salida y errores en logs/services/ProductApi.log, formato estructurado)
echo [3/5] Iniciando API Product...
set "tempProductApiBat=%TEMP%\gesfer_product_api_%RANDOM%.bat"
echo @echo off > "!tempProductApiBat!"
echo chcp 65001 ^>nul >> "!tempProductApiBat!"
echo echo Iniciando API Product GesFer. Log: logs\services\ProductApi.log >> "!tempProductApiBat!"
echo echo. >> "!tempProductApiBat!"
echo powershell -ExecutionPolicy Bypass -NoProfile -File "!scriptsPath!\run-service-with-log.ps1" -ServiceName "ProductApi" -WorkingDir "!productApiPath!" -Command "dotnet run" >> "!tempProductApiBat!"
echo pause >> "!tempProductApiBat!"
start "GesFer API Product" cmd /k "!tempProductApiBat!"
timeout /t 3 /nobreak >nul
echo    API Product iniciada en nueva ventana
echo.

REM 4. Iniciar API Admin (salida y errores en logs/services/AdminApi.log, formato estructurado)
echo [4/5] Iniciando API Admin...
set "tempAdminApiBat=%TEMP%\gesfer_admin_api_%RANDOM%.bat"
echo @echo off > "!tempAdminApiBat!"
echo chcp 65001 ^>nul >> "!tempAdminApiBat!"
echo echo Iniciando API Admin GesFer. Log: logs\services\AdminApi.log >> "!tempAdminApiBat!"
echo echo. >> "!tempAdminApiBat!"
echo powershell -ExecutionPolicy Bypass -NoProfile -File "!scriptsPath!\run-service-with-log.ps1" -ServiceName "AdminApi" -WorkingDir "!adminApiPath!" -Command "dotnet run" >> "!tempAdminApiBat!"
echo pause >> "!tempAdminApiBat!"
start "GesFer API Admin" cmd /k "!tempAdminApiBat!"
timeout /t 3 /nobreak >nul
echo    API Admin iniciada en nueva ventana
echo.

REM 5. Iniciar Frontend Product (salida y errores en logs/services/ProductFront.log, formato estructurado)
echo [5/5] Iniciando Frontend Product...
set "tempProductFrontBat=%TEMP%\gesfer_product_front_%RANDOM%.bat"
echo @echo off > "!tempProductFrontBat!"
echo chcp 65001 ^>nul >> "!tempProductFrontBat!"
echo echo Iniciando Frontend Product GesFer. Log: logs\services\ProductFront.log >> "!tempProductFrontBat!"
echo echo. >> "!tempProductFrontBat!"
echo powershell -ExecutionPolicy Bypass -NoProfile -File "!scriptsPath!\run-service-with-log.ps1" -ServiceName "ProductFront" -WorkingDir "!productFrontPath!" -Command "npm run dev" >> "!tempProductFrontBat!"
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
echo Logs (formato estructurado: timestamp^|nivel^|servicio^|mensaje):
echo   - logs\services\ProductApi.log
echo   - logs\services\AdminApi.log
echo   - logs\services\ProductFront.log
echo.
pause
