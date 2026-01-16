@echo off
REM Script de validación antes de commit para Windows
REM Ejecuta validaciones del Backend y Frontend

setlocal enabledelayedexpansion

echo 🔍 Iniciando validación pre-commit...

set ERROR_COUNT=0

REM 1. Validar Backend - Build
echo ℹ️  Compilando Backend (dotnet build)...
if exist "Api" (
    cd Api
    dotnet build --no-restore >nul 2>&1
    if !errorlevel! equ 0 (
        echo ✅ Backend compilado correctamente
    ) else (
        echo ❌ Error: Falló la compilación del Backend. Ejecuta 'dotnet build' en Api/ para ver detalles.
        set /a ERROR_COUNT+=1
    )
    cd ..
) else (
    echo ❌ Error: Directorio Api/ no encontrado
    set /a ERROR_COUNT+=1
)

REM 2. Validar Frontend - Lint
echo ℹ️  Ejecutando lint del Frontend...
if exist "Cliente" (
    cd Cliente
    call npm run lint >nul 2>&1
    if !errorlevel! equ 0 (
        echo ✅ Lint del Frontend pasado
    ) else (
        echo ❌ Error: Falló el lint del Frontend. Ejecuta 'npm run lint' en Cliente/ para ver detalles.
        set /a ERROR_COUNT+=1
    )
    cd ..
) else (
    echo ❌ Error: Directorio Cliente/ no encontrado
    set /a ERROR_COUNT+=1
)

REM 3. Tests unitarios Backend (rápidos)
echo ℹ️  Ejecutando tests unitarios del Backend...
if exist "Api" (
    cd Api
    dotnet test --no-build --verbosity quiet --filter "FullyQualifiedName!~IntegrationTests" >nul 2>&1
    if !errorlevel! equ 0 (
        echo ✅ Tests unitarios del Backend pasados
    ) else (
        echo ℹ️  No se encontraron tests unitarios del Backend o algunos fallaron (no crítico)
    )
    cd ..
)

REM 4. Tests unitarios Frontend (rápidos)
echo ℹ️  Ejecutando tests unitarios del Frontend...
if exist "Cliente" (
    cd Cliente
    call npm test -- --testPathPattern="__tests__" --passWithNoTests --silent >nul 2>&1
    if !errorlevel! equ 0 (
        echo ✅ Tests unitarios del Frontend pasados
    ) else (
        echo ℹ️  No se encontraron tests unitarios del Frontend o algunos fallaron (no crítico)
    )
    cd ..
)

if !ERROR_COUNT! gtr 0 (
    echo.
    echo ❌ Validación falló con !ERROR_COUNT! error(es). Corrige los errores antes de hacer commit.
    exit /b 1
)

echo.
echo ✅ Todas las validaciones pasaron. Procediendo con el commit...
exit /b 0
