#!/bin/bash
# Script de validación completa para Pull Request / Push
# Ejecuta todos los tests: Backend build, Frontend unitarios, E2E
# ORQUESTA servicios automáticamente: levanta Backend y Frontend antes de tests E2E
# Limpia procesos al finalizar

set -euo pipefail  # Salir inmediatamente si cualquier comando falla

echo "=========================================="
echo "🔍 JUEZ DEL PROYECTO - Validación Completa"
echo "=========================================="
echo ""

ERROR_COUNT=0
BACKEND_PID=""
FRONTEND_PID=""

# Función para esperar que un puerto esté disponible
wait_for_port() {
    local port=$1
    local max_attempts=${2:-60}
    local service_name=${3:-"Servicio"}
    
    echo "   Esperando que $service_name esté disponible en puerto $port..." >&2
    local attempt=0
    
    while [ $attempt -lt $max_attempts ]; do
        if nc -z localhost $port 2>/dev/null; then
            echo "   ✅ $service_name está disponible en puerto $port" >&2
            return 0
        fi
        
        sleep 1
        attempt=$((attempt + 1))
        if [ $((attempt % 10)) -eq 0 ]; then
            echo "   ⏳ Intento $attempt/$max_attempts..." >&2
        fi
    done
    
    echo "   ⚠️  Timeout esperando $service_name en puerto $port" >&2
    return 1
}

# Función para matar procesos en un puerto específico
stop_process_on_port() {
    local port=$1
    local service_name=${2:-"Servicio"}
    
    # Intentar encontrar y matar procesos en el puerto
    if command -v lsof >/dev/null 2>&1; then
        local pids=$(lsof -ti:$port 2>/dev/null || true)
        if [ -n "$pids" ]; then
            echo "$pids" | xargs kill -9 2>/dev/null || true
            echo "   🛑 Proceso(es) ($service_name) en puerto $port terminado(s)" >&2
        fi
    elif command -v netstat >/dev/null 2>&1; then
        local pids=$(netstat -tulpn 2>/dev/null | grep ":$port " | awk '{print $7}' | cut -d'/' -f1 | grep -v '-' || true)
        if [ -n "$pids" ]; then
            echo "$pids" | xargs kill -9 2>/dev/null || true
            echo "   🛑 Proceso(es) ($service_name) en puerto $port terminado(s)" >&2
        fi
    fi
}

# Función de limpieza al finalizar
cleanup_services() {
    echo ""
    echo "🧹 Limpiando servicios..." >&2
    
    # Matar procesos de Backend y Frontend si existen
    if [ -n "$BACKEND_PID" ] && kill -0 "$BACKEND_PID" 2>/dev/null; then
        kill -9 "$BACKEND_PID" 2>/dev/null || true
        echo "   ✅ Proceso Backend terminado" >&2
    fi
    
    if [ -n "$FRONTEND_PID" ] && kill -0 "$FRONTEND_PID" 2>/dev/null; then
        kill -9 "$FRONTEND_PID" 2>/dev/null || true
        echo "   ✅ Proceso Frontend terminado" >&2
    fi
    
    # Matar procesos en puertos 5000 y 3000 por si acaso
    stop_process_on_port 5000 "Backend API"
    stop_process_on_port 3000 "Frontend Next.js"
    
    echo "   ✅ Limpieza completada" >&2
}

# Registrar cleanup al finalizar (éxito o error)
trap cleanup_services EXIT

# 1. Validar Backend - Build
echo "📦 [1/4] Compilando Backend (dotnet build)..."
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
echo "🧪 [2/4] Ejecutando tests unitarios del Frontend (npm test)..."
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

# Solo continuar con tests E2E si no hay errores previos
if [ $ERROR_COUNT -eq 0 ]; then
    # 3. Orquestación de Servicios para Tests E2E
    echo "🚀 [3/4] Orquestando servicios para tests E2E..."
    
    # Limpiar puertos por si están ocupados
    stop_process_on_port 5000 "Backend API"
    stop_process_on_port 3000 "Frontend Next.js"
    sleep 2
    
    # 3.1. Iniciar Backend
    echo "   🔧 Iniciando Backend API en puerto 5000..."
    if [ -d "Api/src/Api" ]; then
        cd Api/src/Api
        # Iniciar Backend en background
        dotnet run --urls http://localhost:5000 > /dev/null 2>&1 &
        BACKEND_PID=$!
        echo "   ✅ Proceso Backend iniciado (PID: $BACKEND_PID)"
        cd ../../..
        
        # Esperar a que el puerto 5000 esté disponible
        if ! wait_for_port 5000 90 "Backend API"; then
            echo "   ❌ ERROR: Backend no respondió en puerto 5000"
            ERROR_COUNT=$((ERROR_COUNT + 1))
        fi
    else
        echo "   ❌ ERROR: Directorio Api/src/Api no encontrado"
        ERROR_COUNT=$((ERROR_COUNT + 1))
    fi
    
    # 3.2. Iniciar Frontend (solo si Backend inició correctamente)
    if [ $ERROR_COUNT -eq 0 ] && [ -n "$BACKEND_PID" ] && kill -0 "$BACKEND_PID" 2>/dev/null; then
        echo "   🔧 Iniciando Frontend Next.js en puerto 3000..."
        if [ -d "Cliente" ]; then
            cd Cliente
            # Iniciar Frontend en background
            PORT=3000 npm run dev > /dev/null 2>&1 &
            FRONTEND_PID=$!
            echo "   ✅ Proceso Frontend iniciado (PID: $FRONTEND_PID)"
            cd ..
            
            # Esperar a que el puerto 3000 esté disponible
            if ! wait_for_port 3000 120 "Frontend Next.js"; then
                echo "   ⚠️  ADVERTENCIA: Frontend puede tardar más en iniciarse" >&2
                echo "   Continuando con tests E2E..." >&2
            fi
            
            # Esperar adicional para que Next.js compile
            sleep 5
        else
            echo "   ❌ ERROR: Directorio Cliente/ no encontrado"
            ERROR_COUNT=$((ERROR_COUNT + 1))
        fi
    fi
    
    echo ""
    
    # 4. Tests E2E Frontend (Playwright)
    echo "🎭 [4/4] Ejecutando tests E2E (Playwright)..."
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
else
    echo "⏭️  [3/4] Saltando tests E2E debido a errores previos"
    echo "⏭️  [4/4] Saltando tests E2E debido a errores previos"
fi

echo ""
echo "=========================================="

# Resultado final (cleanup se ejecuta automáticamente por trap)
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
