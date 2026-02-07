@echo off
:: Configuración de codificación para caracteres especiales
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo INICIANDO CALMA-DESKTOP (ELECTRON)
echo ========================================

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

REM Intentar cerrar instancias previas de Electron para liberar archivos
echo [0/2] Limpiando procesos previos...
taskkill /F /IM electron.exe /T >nul 2>&1
taskkill /F /IM "Calma Desktop.exe" /T >nul 2>&1

REM Verificar integridad de dependencias
set "INSTALL_NEEDED=0"
if not exist "node_modules\" set "INSTALL_NEEDED=1"
if not exist "node_modules\.bin\vite.cmd" set "INSTALL_NEEDED=1"
if not exist "node_modules\.bin\electron.cmd" set "INSTALL_NEEDED=1"

if "!INSTALL_NEEDED!"=="1" (
    echo [1/2] Instalando dependencias (npm install)...
    call npm install
    if errorlevel 1 (
        echo.
        echo [WARN] Fallo la instalacion inicial.
        echo [INFO] Intentando limpieza profunda y reinstalacion...

        REM Limpiar node_modules corrupto
        if exist "node_modules\" (
            echo Eliminando node_modules...
            rmdir /s /q "node_modules"
            if errorlevel 1 (
                 echo ERROR: No se pudo eliminar node_modules. Archivos bloqueados?
                 pause
                 exit /b 1
            )
        )

        REM Reintentar instalación
        echo Reintentando npm install...
        call npm install
        if errorlevel 1 (
            echo.
            echo ERROR CRITICO: No se pudieron instalar las dependencias.
            echo Posibles causas:
            echo  - Problemas de conexion a internet (proxy/vpn)
            echo  - Permisos de escritura bloqueados
            echo.
            pause
            exit /b 1
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