@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Ejecutando API Product, API Admin, Frontend Product y Frontend Admin GesFer
echo ========================================
echo.

REM Cambiar al directorio raiz del proyecto
cd /d "%~dp0"

REM Crear directorio de logs para persistencia estructurada (docs/operations/LOGS_SERVICES_REFERENCE.md)
if not exist "logs\services" mkdir "logs\services"
set "scriptsPath=%~dp0scripts"

REM 1. Detener procesos existentes en puertos y procesos dotnet/GesFer que bloquean DLLs
echo [1/6] Verificando y cerrando procesos existentes...

REM Script PS1 evita VariableNotWritable ($PID es variable automática de solo lectura)
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\cerrar-procesos-servicios.ps1"
echo.

REM 1b. Si falta Next en Product Front o en Admin Front, instalar dependencias en ambos
set "productFrontPath=%~dp0src\Product\Front"
set "adminFrontPath=%~dp0src\Admin\Front"
set "nextExeProduct=!productFrontPath!\node_modules\next\dist\bin\next"
set "nextExeAdmin=!adminFrontPath!\node_modules\next\dist\bin\next"
if not exist "!nextExeProduct!" goto :do_install_fronts
if not exist "!nextExeAdmin!" goto :do_install_fronts
goto :skip_install_fronts
:do_install_fronts
echo [1b/6] Instalando dependencias en Fronts - npm install...
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\install-front-dependencies.ps1"
echo.
:skip_install_fronts

REM 2. Verificar rutas
echo [2/6] Verificando rutas...
set "productApiPath=%~dp0src\Product\Back\Api"
set "adminApiPath=%~dp0src\Admin\Back\Api"
set "productFrontPath=%~dp0src\Product\Front"
set "adminFrontPath=%~dp0src\Admin\Front"

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

if not exist "!adminFrontPath!\package.json" (
    echo ERROR: No se encontro el proyecto del Frontend Admin
    pause
    exit /b 1
)

echo    Rutas verificadas
echo.

REM 3. Iniciar API Product (salida y errores en logs/services/ProductApi.log, formato estructurado)
echo [3/6] Iniciando API Product...
set "tempProductApiBat=%TEMP%\gesfer_product_api_%RANDOM%.bat"
echo @echo off > "!tempProductApiBat!"
echo chcp 65001 ^>nul >> "!tempProductApiBat!"
echo echo Iniciando API Product GesFer. Log: logs\services\ProductApi.log >> "!tempProductApiBat!"
echo echo. >> "!tempProductApiBat!"
echo powershell -ExecutionPolicy Bypass -NoProfile -File "!scriptsPath!\run-service-with-log.ps1" -ServiceName "ProductApi" -WorkingDir "!productApiPath!" -Command "dotnet run" >> "!tempProductApiBat!"
echo pause >> "!tempProductApiBat!"
start "GesFer API Product" cmd /k "!tempProductApiBat!"
ping -n 4 127.0.0.1 >nul
echo    API Product iniciada en nueva ventana
echo.

REM 4. Iniciar API Admin (salida y errores en logs/services/AdminApi.log, formato estructurado)
echo [4/6] Iniciando API Admin...
set "tempAdminApiBat=%TEMP%\gesfer_admin_api_%RANDOM%.bat"
echo @echo off > "!tempAdminApiBat!"
echo chcp 65001 ^>nul >> "!tempAdminApiBat!"
echo echo Iniciando API Admin GesFer. Log: logs\services\AdminApi.log >> "!tempAdminApiBat!"
echo echo. >> "!tempAdminApiBat!"
echo powershell -ExecutionPolicy Bypass -NoProfile -File "!scriptsPath!\run-service-with-log.ps1" -ServiceName "AdminApi" -WorkingDir "!adminApiPath!" -Command "dotnet run" >> "!tempAdminApiBat!"
echo pause >> "!tempAdminApiBat!"
start "GesFer API Admin" cmd /k "!tempAdminApiBat!"
ping -n 4 127.0.0.1 >nul
echo    API Admin iniciada en nueva ventana
echo.

REM 5. Iniciar Frontend Product (salida y errores en logs/services/ProductFront.log, formato estructurado)
echo [5/6] Iniciando Frontend Product...
set "tempProductFrontBat=%TEMP%\gesfer_product_front_%RANDOM%.bat"
echo @echo off > "!tempProductFrontBat!"
echo chcp 65001 ^>nul >> "!tempProductFrontBat!"
echo echo Iniciando Frontend Product GesFer. Log: logs\services\ProductFront.log >> "!tempProductFrontBat!"
echo echo. >> "!tempProductFrontBat!"
echo powershell -ExecutionPolicy Bypass -NoProfile -File "!scriptsPath!\run-service-with-log.ps1" -ServiceName "ProductFront" -WorkingDir "!productFrontPath!" -Command "npm run dev" >> "!tempProductFrontBat!"
echo pause >> "!tempProductFrontBat!"
start "GesFer Frontend Product" cmd /k "!tempProductFrontBat!"
ping -n 3 127.0.0.1 >nul
echo    Frontend Product iniciado en nueva ventana
echo.

REM 6. Iniciar Frontend Admin (puerto 3001, logs/services/AdminFront.log)
echo [6/6] Iniciando Frontend Admin...
set "tempAdminFrontBat=%TEMP%\gesfer_admin_front_%RANDOM%.bat"
echo @echo off > "!tempAdminFrontBat!"
echo chcp 65001 ^>nul >> "!tempAdminFrontBat!"
echo echo Iniciando Frontend Admin GesFer. Log: logs\services\AdminFront.log >> "!tempAdminFrontBat!"
echo echo. >> "!tempAdminFrontBat!"
echo powershell -ExecutionPolicy Bypass -NoProfile -File "!scriptsPath!\run-service-with-log.ps1" -ServiceName "AdminFront" -WorkingDir "!adminFrontPath!" -Command "npm run dev" >> "!tempAdminFrontBat!"
echo pause >> "!tempAdminFrontBat!"
start "GesFer Frontend Admin" cmd /k "!tempAdminFrontBat!"
ping -n 3 127.0.0.1 >nul
echo    Frontend Admin iniciado en nueva ventana
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
echo Frontend Admin disponible en:
echo   - http://localhost:3001
echo.
echo Las ventanas de los servicios estan abiertas.
echo Cierra las ventanas para detener los servicios.
echo.
echo Logs (formato estructurado: timestamp^|nivel^|servicio^|mensaje):
echo   - logs\services\ProductApi.log
echo   - logs\services\AdminApi.log
echo   - logs\services\ProductFront.log
echo   - logs\services\AdminFront.log
echo.
pause
