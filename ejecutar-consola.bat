@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Compilando y ejecutando GesFer.Console
echo ========================================
echo.

REM Guardar la ruta raíz del proyecto
set "ROOT_DIR=%~dp0"

REM Cambiar al directorio del proyecto de consola
cd /d "%ROOT_DIR%GesFer.Console"

REM Compilar el proyecto
echo [1/2] Compilando proyecto...
dotnet build --configuration Release
if errorlevel 1 (
    echo ERROR: No se pudo compilar el proyecto
    pause
    exit /b 1
)
echo    ✓ Proyecto compilado correctamente
echo.

REM Ejecutar el ejecutable
echo [2/2] Ejecutando aplicación...
echo.
set "EXE_PATH=%ROOT_DIR%GesFer.Console\bin\Release\net8.0\GesFer.Console.exe"
if exist "!EXE_PATH!" (
    "!EXE_PATH!"
) else (
    echo    Ejecutando con dotnet run...
    dotnet run --configuration Release --no-build
)
if errorlevel 1 (
    echo ERROR: Error al ejecutar la aplicación
    pause
    exit /b 1
)

echo.
echo ========================================
echo Ejecución completada
echo ========================================
pause
