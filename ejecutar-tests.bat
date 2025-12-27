@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Ejecutando Tests GesFer
echo ========================================
echo.

REM Cambiar al directorio raiz del proyecto
cd /d "%~dp0"

REM Inicializar archivo de log
set "logFile=%~dp0ejecutar-tests.log"
echo ======================================== > "!logFile!"
echo Ejecucion de Tests GesFer >> "!logFile!"
echo Fecha: %date% %time% >> "!logFile!"
echo ======================================== >> "!logFile!"
echo. >> "!logFile!"

REM 1. Verificar rutas
echo [1/3] Verificando rutas...
echo [1/3] Verificando rutas... >> "!logFile!"

set "apiTestsPath=%~dp0Api\src\IntegrationTests"
set "clientePath=%~dp0Cliente"

if not exist "!apiTestsPath!\GesFer.IntegrationTests.csproj" (
    echo ERROR: No se encontro el proyecto de tests de la API >> "!logFile!"
    echo ERROR: No se encontro el proyecto de tests de la API
    pause
    exit /b 1
)

if not exist "!clientePath!\package.json" (
    echo ERROR: No se encontro el proyecto del Cliente >> "!logFile!"
    echo ERROR: No se encontro el proyecto del Cliente
    pause
    exit /b 1
)

echo    Rutas verificadas
echo    Rutas verificadas >> "!logFile!"
echo. >> "!logFile!"
echo.

REM 2. Ejecutar tests del Cliente
echo [2/3] Ejecutando tests del Cliente...
echo [2/3] Ejecutando tests del Cliente... >> "!logFile!"
echo ======================================== >> "!logFile!"
echo TESTS DEL CLIENTE >> "!logFile!"
echo ======================================== >> "!logFile!"
echo. >> "!logFile!"

cd /d "!clientePath!"

REM Verificar que node_modules existe
if not exist "node_modules" (
    echo    Instalando dependencias...
    echo    Instalando dependencias... >> "!logFile!"
    call npm install >> "!logFile!" 2>&1
    if errorlevel 1 (
        echo ERROR: Fallo la instalacion de dependencias del Cliente >> "!logFile!"
        echo ERROR: Fallo la instalacion de dependencias del Cliente
        cd /d "%~dp0"
        pause
        exit /b 1
    )
)

REM Ejecutar todos los tests del cliente (incluyendo integridad)
echo    Ejecutando todos los tests (incluyendo integridad)...
echo    Ejecutando todos los tests (incluyendo integridad)... >> "!logFile!"
echo. >> "!logFile!"

call npm run test:all >> "!logFile!" 2>&1
set "clienteTestResult=!errorlevel!"

if !clienteTestResult! equ 0 (
    echo    Tests del Cliente: EXITOSOS
    echo    Tests del Cliente: EXITOSOS >> "!logFile!"
) else (
    echo    Tests del Cliente: FALLIDOS (codigo: !clienteTestResult!)
    echo    Tests del Cliente: FALLIDOS (codigo: !clienteTestResult!) >> "!logFile!"
)

echo. >> "!logFile!"
echo. >> "!logFile!"

REM 3. Ejecutar tests de la API
echo [3/3] Ejecutando tests de la API...
echo [3/3] Ejecutando tests de la API... >> "!logFile!"
echo ======================================== >> "!logFile!"
echo TESTS DE LA API >> "!logFile!"
echo ======================================== >> "!logFile!"
echo. >> "!logFile!"

cd /d "!apiTestsPath!"

REM Verificar que dotnet esta disponible
where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet no esta disponible en el PATH >> "!logFile!"
    echo ERROR: dotnet no esta disponible en el PATH
    cd /d "%~dp0"
    pause
    exit /b 1
)

echo    Ejecutando tests de integracion...
echo    Ejecutando tests de integracion... >> "!logFile!"
echo. >> "!logFile!"

dotnet test --verbosity normal --logger "console;verbosity=detailed" >> "!logFile!" 2>&1
set "apiTestResult=!errorlevel!"

if !apiTestResult! equ 0 (
    echo    Tests de la API: EXITOSOS
    echo    Tests de la API: EXITOSOS >> "!logFile!"
) else (
    echo    Tests de la API: FALLIDOS (codigo: !apiTestResult!)
    echo    Tests de la API: FALLIDOS (codigo: !apiTestResult!) >> "!logFile!"
)

echo. >> "!logFile!"
echo. >> "!logFile!"

REM Volver al directorio raiz
cd /d "%~dp0"

REM Resumen final
echo ========================================
echo Resumen de Ejecucion
echo ========================================
echo.

echo ======================================== >> "!logFile!"
echo RESUMEN FINAL >> "!logFile!"
echo ======================================== >> "!logFile!"
echo Fecha de finalizacion: %date% %time% >> "!logFile!"
echo. >> "!logFile!"

if !clienteTestResult! equ 0 (
    echo Tests del Cliente: EXITOSOS
    echo Tests del Cliente: EXITOSOS >> "!logFile!"
) else (
    echo Tests del Cliente: FALLIDOS
    echo Tests del Cliente: FALLIDOS >> "!logFile!"
)

if !apiTestResult! equ 0 (
    echo Tests de la API: EXITOSOS
    echo Tests de la API: EXITOSOS >> "!logFile!"
) else (
    echo Tests de la API: FALLIDOS
    echo Tests de la API: FALLIDOS >> "!logFile!"
)

echo.
echo Log completo guardado en: !logFile!
echo Log completo guardado en: !logFile! >> "!logFile!"
echo.

REM Determinar codigo de salida
if !clienteTestResult! neq 0 exit /b !clienteTestResult!
if !apiTestResult! neq 0 exit /b !apiTestResult!

echo Todos los tests se ejecutaron correctamente.
echo Todos los tests se ejecutaron correctamente. >> "!logFile!"
echo.

pause
