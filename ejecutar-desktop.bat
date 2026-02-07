@echo off
:: Configuración de codificación para caracteres especiales
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo   INICIANDO CALMA-DESKTOP (ELECTRON)
echo ========================================
echo.

:: 1. Definición de rutas seguras
:: %~dp0 siempre termina en \, por lo que no añadimos una extra antes de 'src'
set "BASE_DIR=%~dp0"
set "APP_DIR=%BASE_DIR%src\Tools\Calma-Desktop"

:: 2. Validación de entorno
where npm >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] No se detectó 'npm' en el sistema.
    echo Por favor, instala Node.js antes de continuar.
    goto :error_exit
)

:: 3. Cambio de directorio con validación de existencia
if not exist "%APP_DIR%" (
    echo [ERROR] No se encuentra la ruta: "%APP_DIR%"
    goto :error_exit
)

pushd "%APP_DIR%"

:: 4. Gestión de dependencias (Estructura de bloque lineal para evitar fallos de salto)
if not exist "node_modules\" (
    echo [1/2] Módulos no encontrados. Iniciando instalación...
    call npm install
    if %errorlevel% neq 0 (
        echo [ERROR] La instalación de dependencias falló.
        popd
        goto :error_exit
    )
    echo    ✓ Dependencias instaladas.
) else (
    echo [1/2] Dependencias ya presentes. Saltando instalación.
)

:: 5. Ejecución de la aplicación
echo [2/2] Lanzando aplicación (npm run dev)...
echo.
call npm run dev

:: 6. Verificación de salida
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] La aplicación se cerró con un código de error: %errorlevel%
    popd
    goto :error_exit
)

echo.
echo ========================================
echo   EJECUCIÓN FINALIZADA CORRECTAMENTE
echo ========================================
popd
pause
exit /b 0

:error_exit
echo.
echo [!] El proceso no pudo completarse.
pause
exit /b 1