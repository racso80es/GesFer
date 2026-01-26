#!/bin/bash
# Script para resetear completamente la base de datos MySQL
# Este script detiene y elimina los contenedores Docker, elimina volúmenes y datos persistentes,
# y reinicia todo desde cero para un desarrollo limpio

echo "=== Reset Completo de Base de Datos MySQL ==="
echo ""

# 1. Detener y eliminar contenedores y volúmenes
echo "1. Deteniendo y eliminando contenedores Docker..."
docker-compose down -v
if [ $? -eq 0 ]; then
    echo "   ✓ Contenedores y volúmenes eliminados"
else
    echo "   ⚠ Advertencia: Algunos recursos no se pudieron eliminar"
fi

# 2. Eliminar directorio de datos MySQL si existe (bind mount residual)
echo "2. Eliminando datos persistentes de MySQL..."
MYSQL_DATA_PATH="./docker_data/mysql"
if [ -d "$MYSQL_DATA_PATH" ]; then
    rm -rf "$MYSQL_DATA_PATH"
    if [ $? -eq 0 ]; then
        echo "   ✓ Directorio docker_data/mysql eliminado"
    else
        echo "   ⚠ No se pudo eliminar el directorio (puede estar en uso)"
        echo "   Intenta cerrar cualquier herramienta que esté usando MySQL"
    fi
else
    echo "   ✓ El directorio docker_data/mysql no existe (ya está limpio)"
fi

# 3. Limpiar volúmenes huérfanos
echo "3. Limpiando volúmenes Docker huérfanos..."
docker volume prune -f > /dev/null 2>&1
echo "   ✓ Volúmenes huérfanos eliminados"

# 4. Iniciar contenedores nuevamente
echo "4. Iniciando contenedores Docker..."
docker-compose up -d
if [ $? -eq 0 ]; then
    echo "   ✓ Contenedores iniciados"
else
    echo "   ✗ Error al iniciar contenedores"
    exit 1
fi

# 5. Esperar a que MySQL esté listo
echo "5. Esperando a que MySQL esté listo..."
MAX_ATTEMPTS=30
ATTEMPT=0
MYSQL_READY=false

while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    sleep 2
    ATTEMPT=$((ATTEMPT + 1))
    docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        MYSQL_READY=true
        break
    fi
    echo "   Intento $ATTEMPT/$MAX_ATTEMPTS..."
done

if [ "$MYSQL_READY" = true ]; then
    echo "   ✓ MySQL está listo"
else
    echo "   ✗ MySQL no está disponible después de $MAX_ATTEMPTS intentos"
    exit 1
fi

echo ""
echo "=== Reset Completado ==="
echo ""
echo "Base de datos MySQL reiniciada y lista para usar."
echo ""
echo "Próximos pasos:"
echo "  1. Inicia la API (dotnet run --project src/Api)"
echo "  2. Ejecuta el endpoint de inicialización:"
echo "     POST http://localhost:5000/api/setup/initialize"
echo "     O espera a que las migraciones se ejecuten automáticamente"
echo ""