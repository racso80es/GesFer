@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul

:: =================================================================================================
:: SCRIPT: ejecutar-electron.bat
:: DESCRIPCIÓN: Lanzador oficial para la interfaz de usuario Electron (Calma-Desktop).
:: AUTOR: Tekton Agent (Generado automáticamente)
:: SEGURIDAD: Validación estricta de rutas y dependencias.
:: =================================================================================================

:: --- 1. CONFIGURACIÓN DE ENTORNO ---
set "ROOT_DIR=%~dp0"
set "APP_DIR=%ROOT_DIR%src\Kalma2\Interface\Desktop"

echo ===============================================================================
echo   GESFER - LANZADOR DE INTERFAZ (ELECTRON)
echo ===============================================================================
echo.

:: --- 2. VALIDACIÓN DE HERRAMIENTAS ---
echo [1/4] Verificando herramientas...

where node >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] No se detectó 'node'. Por favor instala Node.js.
    goto :error_exit
)

where npm >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] No se detectó 'npm'. Por favor instala Node.js.
    goto :error_exit
)
echo    ✓ Node.js y npm detectados.

:: --- 3. VALIDACIÓN DE DIRECTORIO ---
echo [2/4] Validando directorio del proyecto...
if not exist "%APP_DIR%" (
    echo [ERROR] No existe la carpeta del proyecto:
    echo         "%APP_DIR%"
    goto :error_exit
)
pushd "%APP_DIR%"
echo    ✓ Directorio válido.

:: --- 4. LIMPIEZA DE PROCESOS ---
echo [3/4] Preparando entorno...
taskkill /F /IM electron.exe /T >nul 2>&1
taskkill /F /IM "Calma Desktop.exe" /T >nul 2>&1
echo    ✓ Procesos anteriores limpiados.

:: --- 5. GESTIÓN DE DEPENDENCIAS ---
echo [4/4] Verificando dependencias...
set "INSTALL_NEEDED=0"
if not exist "node_modules\" set "INSTALL_NEEDED=1"
if not exist "package-lock.json" set "INSTALL_NEEDED=1"

if "%INSTALL_NEEDED%"=="1" (
    echo    [!] Dependencias faltantes. Instalando...
    call npm install
    if !errorlevel! neq 0 (
        echo [ERROR] Falló la instalación de dependencias.
        goto :error_exit
    )
    echo    ✓ Instalación completada.
) else (
    echo    ✓ Dependencias listas.
)

:: --- 6. EJECUCIÓN ---
echo.
echo ===============================================================================
echo   INICIANDO APLICACIÓN...
echo ===============================================================================
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
echo ===============================================================================
echo   SESIÓN FINALIZADA CORRECTAMENTE
echo ===============================================================================
pause
exit /b 0

:error_exit
if exist "%APP_DIR%" popd
echo.
echo [!] El script se detuvo debido a un error crítico.
pause
exit /b 1
