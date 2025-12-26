@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Ejecutando API y Cliente GesFer
echo ========================================
echo.

REM Cambiar al directorio raiz del proyecto
cd /d "%~dp0"

REM 1. Detener procesos existentes en los puertos
echo [1/4] Verificando y cerrando procesos existentes...

REM Usar PowerShell para cerrar procesos de forma mas confiable
powershell -Command "$ports = @(5000, 5001, 3000); foreach ($port in $ports) { $connections = netstat -ano | Select-String \":$port.*LISTENING\"; foreach ($conn in $connections) { $pid = ($conn -split '\s+')[-1]; if ($pid -match '^\d+$') { Write-Host \"Cerrando proceso en puerto $port (PID: $pid)...\"; taskkill /PID $pid /F 2>$null } } }"

echo    Verificacion completada
echo.

REM 2. Verificar rutas
echo [2/4] Verificando rutas...
set "apiPath=%~dp0Api\src\Api"
set "clientePath=%~dp0Cliente"

if not exist "!apiPath!\GesFer.Api.csproj" (
    echo ERROR: No se encontro el proyecto de la API
    pause
    exit /b 1
)

if not exist "!clientePath!\package.json" (
    echo ERROR: No se encontro el proyecto del Cliente
    pause
    exit /b 1
)

echo    Rutas verificadas
echo.

REM 3. Iniciar API
echo [3/4] Iniciando API...
set "tempApiBat=%TEMP%\gesfer_api_%RANDOM%.bat"
echo @echo off > "!tempApiBat!"
echo cd /d "!apiPath!" >> "!tempApiBat!"
echo echo Iniciando API GesFer... >> "!tempApiBat!"
echo echo. >> "!tempApiBat!"
echo dotnet run >> "!tempApiBat!"
echo pause >> "!tempApiBat!"
start "GesFer API" cmd /k "!tempApiBat!"
timeout /t 3 /nobreak >nul
echo    API iniciada en nueva ventana
echo.

REM 4. Iniciar Cliente
echo [4/4] Iniciando Cliente...
set "tempClienteBat=%TEMP%\gesfer_cliente_%RANDOM%.bat"
echo @echo off > "!tempClienteBat!"
echo cd /d "!clientePath!" >> "!tempClienteBat!"
echo echo Iniciando Cliente GesFer... >> "!tempClienteBat!"
echo echo. >> "!tempClienteBat!"
echo npm run dev >> "!tempClienteBat!"
echo pause >> "!tempClienteBat!"
start "GesFer Cliente" cmd /k "!tempClienteBat!"
timeout /t 2 /nobreak >nul
echo    Cliente iniciado en nueva ventana
echo.

echo ========================================
echo Servicios iniciados
echo ========================================
echo.
echo API disponible en:
echo   - HTTP: http://localhost:5000
echo   - HTTPS: https://localhost:5001
echo   - Swagger: http://localhost:5000/swagger
echo.
echo Cliente disponible en:
echo   - http://localhost:3000
echo.
echo Las ventanas de los servicios estan abiertas.
echo Cierra las ventanas para detener los servicios.
echo.
pause
