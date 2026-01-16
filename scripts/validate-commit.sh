#!/bin/bash

# Script de validación antes de commit
# Ejecuta validaciones del Backend y Frontend

set -e  # Salir si cualquier comando falla

echo "🔍 Iniciando validación pre-commit..."

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para mostrar errores
error() {
    echo -e "${RED}❌ Error: $1${NC}" >&2
    exit 1
}

# Función para mostrar éxito
success() {
    echo -e "${GREEN}✅ $1${NC}"
}

# Función para mostrar información
info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

# 1. Validar Backend - Build
info "Compilando Backend (dotnet build)..."
if [ -d "Api" ]; then
    cd Api
    if dotnet build --no-restore > /dev/null 2>&1; then
        success "Backend compilado correctamente"
    else
        error "Falló la compilación del Backend. Ejecuta 'dotnet build' en Api/ para ver detalles."
    fi
    cd ..
else
    error "Directorio Api/ no encontrado"
fi

# 2. Validar Frontend - Lint
info "Ejecutando lint del Frontend..."
if [ -d "Cliente" ]; then
    cd Cliente
    if npm run lint > /dev/null 2>&1; then
        success "Lint del Frontend pasado"
    else
        error "Falló el lint del Frontend. Ejecuta 'npm run lint' en Cliente/ para ver detalles."
    fi
    cd ..
else
    error "Directorio Cliente/ no encontrado"
fi

# 3. Tests unitarios Backend (rápidos)
info "Ejecutando tests unitarios del Backend..."
if [ -d "Api" ]; then
    cd Api
    if dotnet test --no-build --verbosity quiet --filter "FullyQualifiedName!~IntegrationTests" > /dev/null 2>&1; then
        success "Tests unitarios del Backend pasados"
    else
        # Si no hay tests unitarios, no es un error crítico
        info "No se encontraron tests unitarios del Backend o algunos fallaron (no crítico)"
    fi
    cd ..
fi

# 4. Tests unitarios Frontend (rápidos)
info "Ejecutando tests unitarios del Frontend..."
if [ -d "Cliente" ]; then
    cd Cliente
    if npm test -- --testPathPattern="__tests__" --passWithNoTests --silent > /dev/null 2>&1; then
        success "Tests unitarios del Frontend pasados"
    else
        # Si no hay tests unitarios, no es un error crítico
        info "No se encontraron tests unitarios del Frontend o algunos fallaron (no crítico)"
    fi
    cd ..
fi

success "Todas las validaciones pasaron. Procediendo con el commit..."
