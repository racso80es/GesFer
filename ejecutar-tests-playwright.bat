@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

REM Obtener fecha y hora para el log
for /f "tokens=1-3 delims=/ " %%a in ('date /t') do set mydate=%%c-%%a-%%b
for /f "tokens=1-2 delims=: " %%a in ('time /t') do set mytime=%%a:%%b
set "fecha=%mydate% %mytime%"

REM Nombre del archivo de log
set "logFile=ejecutar-tests-playwright.log"

echo ======================================== >> "!logFile!"
echo Ejecucion de Tests Playwright GesFer >> "!logFile!"
echo Fecha: !fecha! >> "!logFile!"
echo ======================================== >> "!logFile!"
echo. >> "!logFile!"

echo ========================================
echo Ejecucion de Tests Playwright GesFer
echo Fecha: !fecha!
echo ========================================
echo.

REM Cambiar al directorio raiz del proyecto
cd /d "%~dp0"

REM 1. Verificar que los servicios esten iniciados
echo [1/3] Verificando servicios...
echo [1/3] Verificando servicios... >> "!logFile!"

REM Verificar puerto 3000 (Cliente)
echo    Verificando puerto 3000 (Cliente)... >> "!logFile!"
netstat -ano | findstr ":3000" | findstr "LISTENING" >nul
if errorlevel 1 (
    echo ERROR: El servicio Cliente no esta corriendo en el puerto 3000 >> "!logFile!"
    echo ERROR: El servicio Cliente no esta corriendo en el puerto 3000
    echo. >> "!logFile!"
    echo Por favor, ejecuta ejecutar-servicios.bat para iniciar los servicios. >> "!logFile!"
    echo Por favor, ejecuta ejecutar-servicios.bat para iniciar los servicios.
    pause
    exit /b 1
)
echo    OK: Cliente esta corriendo en el puerto 3000 >> "!logFile!"
echo    OK: Cliente esta corriendo en el puerto 3000

REM Verificar puerto 5000 (API)
echo    Verificando puerto 5000 (API)... >> "!logFile!"
netstat -ano | findstr ":5000" | findstr "LISTENING" >nul
if errorlevel 1 (
    echo ERROR: El servicio API no esta corriendo en el puerto 5000 >> "!logFile!"
    echo ERROR: El servicio API no esta corriendo en el puerto 5000
    echo. >> "!logFile!"
    echo Por favor, ejecuta ejecutar-servicios.bat para iniciar los servicios. >> "!logFile!"
    echo Por favor, ejecuta ejecutar-servicios.bat para iniciar los servicios.
    pause
    exit /b 1
)
echo    OK: API esta corriendo en el puerto 5000 >> "!logFile!"
echo    OK: API esta corriendo en el puerto 5000
echo. >> "!logFile!"
echo.

REM 2. Verificar rutas y dependencias
echo [2/3] Verificando rutas y dependencias...
echo [2/3] Verificando rutas y dependencias... >> "!logFile!"

set "clientePath=%~dp0Cliente"

if not exist "!clientePath!\package.json" (
    echo ERROR: No se encontro el proyecto del Cliente >> "!logFile!"
    echo ERROR: No se encontro el proyecto del Cliente
    pause
    exit /b 1
)

if not exist "!clientePath!\node_modules" (
    echo ERROR: node_modules no encontrado. Ejecuta 'npm install' en el directorio Cliente >> "!logFile!"
    echo ERROR: node_modules no encontrado. Ejecuta 'npm install' en el directorio Cliente
    pause
    exit /b 1
)

echo    Rutas verificadas >> "!logFile!"
echo    Rutas verificadas
echo. >> "!logFile!"
echo.

REM 3. Ejecutar tests de Playwright
echo [3/3] Ejecutando tests de Playwright...
echo [3/3] Ejecutando tests de Playwright... >> "!logFile!"
echo ======================================== >> "!logFile!"
echo TESTS DE PLAYWRIGHT >> "!logFile!"
echo ======================================== >> "!logFile!"
echo. >> "!logFile!"

cd /d "!clientePath!"

REM Ejecutar tests y redirigir salida al log
echo Ejecutando tests de Playwright... >> "!logFile!"
echo Ejecutando tests de Playwright...

npx playwright test >> "..\!logFile!" 2>&1
set "testExitCode=!errorlevel!"

REM Obtener fecha y hora de finalizacion
for /f "tokens=1-3 delims=/ " %%a in ('date /t') do set mydate=%%c-%%a-%%b
for /f "tokens=1-2 delims=: " %%a in ('time /t') do set mytime=%%a:%%b
set "fechaFin=%mydate% %mytime%"

echo. >> "!logFile!"
echo ======================================== >> "!logFile!"
if !testExitCode! equ 0 (
    echo Tests finalizados correctamente >> "!logFile!"
    echo Fecha fin: !fechaFin! >> "!logFile!"
    echo ======================================== >> "!logFile!"
    echo.
    echo ========================================
    echo Tests finalizados correctamente
    echo Fecha fin: !fechaFin!
    echo ========================================
) else (
    echo Tests finalizados con errores (Codigo: !testExitCode!) >> "!logFile!"
    echo Fecha fin: !fechaFin! >> "!logFile!"
    echo ======================================== >> "!logFile!"
    echo.
    echo ========================================
    echo Tests finalizados con errores (Codigo: !testExitCode!)
    echo Fecha fin: !fechaFin!
    echo ========================================
)

echo.
echo Resultado completo guardado en: !logFile!
echo.

pause
exit /b !testExitCode!

