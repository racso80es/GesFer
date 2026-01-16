#!/bin/bash
# Script de validación completa para Pull Request / Push
# Ejecuta todos los tests: Backend build, Frontend unitarios, E2E
# Si cualquier test falla, termina con exit 1

set -e  # Salir inmediatamente si cualquier comando falla

echo "=========================================="
echo "🔍 JUEZ DEL PROYECTO - Validación Completa"
echo "=========================================="
echo ""

ERROR_COUNT=0

# 1. Validar Backend - Build
echo "📦 [1/3] Compilando Backend (dotnet build)..."
if [ -d "Api" ]; then
    cd Api
    if dotnet build --no-restore; then
        echo "✅ Backend compilado correctamente"
    else
        echo "❌ ERROR: Falló la compilación del Backend"
        ERROR_COUNT=$((ERROR_COUNT + 1))
    fi
    cd ..
else
    echo "❌ ERROR: Directorio Api/ no encontrado"
    ERROR_COUNT=$((ERROR_COUNT + 1))
fi

echo ""

# 2. Tests unitarios Frontend
echo "🧪 [2/3] Ejecutando tests unitarios del Frontend (npm test)..."
if [ -d "Cliente" ]; then
    cd Cliente
    if npm run test -- --passWithNoTests; then
        echo "✅ Tests unitarios del Frontend pasados"
    else
        echo "❌ ERROR: Fallaron los tests unitarios del Frontend"
        ERROR_COUNT=$((ERROR_COUNT + 1))
    fi
    cd ..
else
    echo "❌ ERROR: Directorio Cliente/ no encontrado"
    ERROR_COUNT=$((ERROR_COUNT + 1))
fi

echo ""

# 3. Tests E2E Frontend (Playwright)
echo "🎭 [3/3] Ejecutando tests E2E (Playwright)..."
if [ -d "Cliente" ]; then
    cd Cliente
    if npx playwright test; then
        echo "✅ Tests E2E pasados"
    else
        echo "❌ ERROR: Fallaron los tests E2E"
        ERROR_COUNT=$((ERROR_COUNT + 1))
    fi
    cd ..
else
    echo "❌ ERROR: Directorio Cliente/ no encontrado"
    ERROR_COUNT=$((ERROR_COUNT + 1))
fi

echo ""
echo "=========================================="

# Resultado final
if [ $ERROR_COUNT -eq 0 ]; then
    echo "✅ TODAS LAS VALIDACIONES PASARON"
    echo "   El Juez del Proyecto aprueba el push."
    echo "=========================================="
    exit 0
else
    echo "❌ VALIDACIÓN FALLÓ con $ERROR_COUNT error(es)"
    echo "   El Juez del Proyecto BLOQUEA el push."
    echo "   Corrige los errores antes de intentar push nuevamente."
    echo "=========================================="
    exit 1
fi
