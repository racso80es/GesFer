@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul

:: --- CONFIGURACIÓN DE RUTAS ---
set "BASE_DIR=%~dp0"
set "APP_DIR=%BASE_DIR%src\Tools\Calma-Desktop"

echo ========================================
echo   INICIANDO INTERFAZ DE USUARIO (CALMA-DESKTOP)
echo ========================================

:: 1. Validación de NPM
where npm >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] No se detectó 'npm'. Por favor instala Node.js.
    goto :error_exit
)

:: 2. Entrada al directorio
if not exist "%APP_DIR%" (
    echo [ERROR] No existe la carpeta del proyecto: "%APP_DIR%"
    goto :error_exit
)
pushd "%APP_DIR%"

:: 3. Limpieza de procesos previos
echo [1/3] Limpiando procesos previos...
taskkill /F /IM electron.exe /T >nul 2>&1
taskkill /F /IM "Calma Desktop.exe" /T >nul 2>&1

:: 4. Verificación de Dependencias
echo [2/3] Verificando dependencias...
set "NEEDS_INSTALL=0"
if not exist "node_modules\" set "NEEDS_INSTALL=1"
if not exist "package-lock.json" set "NEEDS_INSTALL=1"

if "%NEEDS_INSTALL%"=="1" (
    echo [!] Dependencias faltantes o incompletas. Instalando...
    call npm install
    if !errorlevel! neq 0 (
        echo [ERROR] Falló la instalación de dependencias.
        goto :error_exit
    )
) else (
    echo    ✓ Dependencias listas.
)

:: 5. Ejecución
echo [3/3] Ejecutando interfaz...
echo.
call npm run dev

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] La aplicación se cerró con errores.
    goto :error_exit
)

:: --- SALIDA EXITOSA ---
popd
echo.
echo ========================================
echo   SESIÓN FINALIZADA CORRECTAMENTE
echo ========================================
pause
exit /b 0

:error_exit
if exist "%APP_DIR%" popd
echo.
echo [!] El script se detuvo debido a un error.
pause
exit /b 1
