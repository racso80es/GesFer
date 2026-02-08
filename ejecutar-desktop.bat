@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul

:: --- CONFIGURACIÓN DE RUTAS ---
set "BASE_DIR=%~dp0"
set "APP_DIR=%BASE_DIR%src\Tools\Calma-Desktop"

echo ========================================
echo   INICIANDO CALMA-DESKTOP (ELECTRON)
echo ========================================

:: 1. Validación de NPM
where npm >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] No se detectó 'npm'. Instala Node.js.
    goto :error_exit
)

:: 2. Entrada al directorio (Punto Crítico Corregido)
if not exist "%APP_DIR%" (
    echo [ERROR] No existe la carpeta: "%APP_DIR%"
    goto :error_exit
)
pushd "%APP_DIR%"

:: 3. Limpieza de procesos para evitar bloqueos de archivos
echo [1/3] Liberando archivos de Electron...
taskkill /F /IM electron.exe /T >nul 2>&1
taskkill /F /IM "Calma Desktop.exe" /T >nul 2>&1

:: 4. Verificación de Dependencias
echo [2/3] Validando módulos...
set "NEEDS_INSTALL=0"
if not exist "node_modules\" set "NEEDS_INSTALL=1"
if not exist "package-lock.json" set "NEEDS_INSTALL=1"

if "%NEEDS_INSTALL%"=="1" (
    echo [!] Dependencias incompletas. Instalando...
    call npm install
    if !errorlevel! neq 0 (
        echo [ERROR] Falló la instalación de npm.
        goto :error_exit
    )
) else (
    echo    ✓ Módulos listos.
)

:: 5. Ejecución
echo [3/3] Lanzando Calma-Desktop...
echo.
call npm run dev

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] La aplicación falló al arrancar.
    goto :error_exit
)

:: --- SALIDA EXITOSA ---
popd
echo.
echo ========================================
echo   PROCESO FINALIZADO
echo ========================================
pause
exit /b 0

:error_exit
if exist "%APP_DIR%" popd
echo.
echo [!] El script se detuvo por un error.
pause
exit /b 1