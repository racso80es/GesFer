@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo INICIANDO CALMA-DESKTOP (ELECTRON)
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
    )
    echo    ✓ Dependencias instaladas correctamente
    echo.
) else (
    echo [1/2] Dependencias verificadas.
    echo.
)

REM Ejecutar la aplicación
echo [2/2] Ejecutando aplicación (npm run dev)...
echo.
call npm run dev
if errorlevel 1 (
    echo.
    echo ERROR: La aplicacion se cerro inesperadamente.
    pause
    exit /b 1
)

echo.
echo ========================================
echo Ejecución finalizada
echo ========================================
pause
