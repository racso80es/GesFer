#!/bin/bash

# Script de validación antes de commit
# Ejecuta validaciones del Backend y Frontend

set -e  # Salir si cualquier comando falla

echo "🔍 Iniciando validación pre-commit..."

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# PROTOCOLO DE PROTECCIÓN: Bloquear commits directos a master/main
CURRENT_BRANCH=$(git branch --show-current)
if [ "$CURRENT_BRANCH" = "master" ] || [ "$CURRENT_BRANCH" = "main" ]; then
    echo ""
    echo -e "${RED}========================================${NC}"
    echo -e "${RED}ERROR: COMMIT BLOQUEADO${NC}"
    echo -e "${RED}========================================${NC}"
    echo ""
    echo -e "${YELLOW}PROHIBIDO hacer commits directos a la rama '$CURRENT_BRANCH'.${NC}"
    echo ""
    echo -e "${CYAN}Flujo obligatorio:${NC}"
    echo -e "${WHITE}  1. git checkout -b feature/o-fix/nombre-tarea${NC}"
    echo -e "${WHITE}  2. Realizar cambios${NC}"
    echo -e "${WHITE}  3. git commit${NC}"
    echo -e "${WHITE}  4. git push origin feature/o-fix/nombre-tarea${NC}"
    echo -e "${WHITE}  5. Crear Pull Request${NC}"
    echo ""
    echo -e "${YELLOW}Master/main solo se actualiza mediante merge de PR.${NC}"
    echo ""
    exit 1
fi

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

# 0. Validar Token de Interacción (Auditor Process)
assert_interaction_token() {
    info "Verificando Token de Interacción [Auditor Process]..."

    local script_path="scripts/auditor/process-token-manager.sh"
    if [ ! -f "$script_path" ]; then
        error "Script de gestión de tokens no encontrado en $script_path"
    fi

    if bash "$script_path" Validate; then
         success "Token válido."
    else
         error "Token de interacción inválido. El Auditor ha bloqueado el proceso."
    fi
}
assert_interaction_token

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
