@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo Iniciando Calma-Desktop (Electron)
echo ========================================
echo.

REM Guardar la ruta raíz del proyecto
set "ROOT_DIR=%~dp0"

REM Cambiar al directorio del proyecto Electron
cd /d "%ROOT_DIR%src\Tools\Calma-Desktop"
if errorlevel 1 (
    echo ERROR: No se encuentra el directorio src\Tools\Calma-Desktop
    pause
    exit /b 1
)

REM Verificar si existe package.json para confirmar que estamos en el directorio correcto
if not exist "package.json" (
    echo ERROR: No se encuentra package.json en el directorio actual.
    echo Asegurese de que src\Tools\Calma-Desktop contiene el proyecto Electron.
    pause
    exit /b 1
)

REM Verificar si existen módulos de node
if not exist "node_modules\" (
    echo [1/2] Instalando dependencias (npm install)...
    call npm install
    if errorlevel 1 (
        echo ERROR: No se pudieron instalar las dependencias
        pause
        exit /b 1
    )
    echo    ✓ Dependencias instaladas correctamente
    echo.
) else (
    echo [1/2] Dependencias ya instaladas.
    echo.
)

REM Ejecutar la aplicación
echo [2/2] Ejecutando aplicación (npm run dev)...
echo.
call npm run dev
if errorlevel 1 (
    echo ERROR: Error al ejecutar la aplicación
    pause
    exit /b 1
)

echo.
echo ========================================
echo Ejecución finalizada
echo ========================================
pause
