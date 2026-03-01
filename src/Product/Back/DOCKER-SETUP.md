# Configuración de Docker para GesFer API

## ✅ Configuración Verificada

El archivo `docker-compose.yml` está correctamente configurado con:

### Servicios Configurados

1. **MySQL 8.0** (`gesfer_api_db`)
   - Puerto: `3306`
   - Base de datos: `ScrapDb`
   - Usuario: `scrapuser`
   - Contraseña: `scrappassword`
   - Root password: `rootpassword`
   - Charset: `utf8mb4_unicode_ci`
   - Healthcheck configurado
   - Persistencia en `./docker_data/mysql`

2. **Memcached** (`gesfer_api_cache`)
   - Puerto: `11211`
   - Memoria: 128MB
   - Healthcheck configurado

3. **Adminer** (`gesfer_api_adminer`)
   - Puerto: `8080`
   - Interfaz web para gestión de MySQL
   - Espera a que MySQL esté saludable antes de iniciar

### Red Docker

Todos los servicios están en la red `gesfer_network` (bridge).

## 🚀 Iniciar Servicios

### Opción 1: Script PowerShell (Recomendado)

```powershell
.\docker-start.ps1
```

### Opción 2: Docker Compose Manual

```bash
docker-compose up -d
```

### Verificar Estado

```bash
docker-compose ps
```

### Ver Logs

```bash
# Todos los servicios
docker-compose logs -f

# Solo MySQL
docker-compose logs -f db

# Solo Memcached
docker-compose logs -f cache
```

## 🛑 Detener Servicios

### Opción 1: Script PowerShell

```powershell
.\docker-stop.ps1
```

### Opción 2: Docker Compose Manual

```bash
docker-compose down
```

### Detener y Eliminar Volúmenes (⚠️ Elimina datos)

```bash
docker-compose down -v
```

## 🔍 Verificar Conexión a MySQL

### Desde PowerShell

```powershell
docker exec -it gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e "SELECT 1;"
```

### Desde Adminer

1. Abrir navegador en: `http://localhost:8080`
2. Sistema: `MySQL`
3. Servidor: `db`
4. Usuario: `scrapuser`
5. Contraseña: `scrappassword`
6. Base de datos: `ScrapDb`

## 📝 Cadena de Conexión

La cadena de conexión configurada en `appsettings.json` es:

```
Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;
```

## ⚙️ Configuración de EF Core

El `ProductDbContext` está configurado con:
- ✅ Reintentos automáticos en caso de fallo (5 intentos, 30s de delay)
- ✅ Logging detallado en desarrollo
- ✅ String comparison habilitado
- ✅ Versión MySQL 8.0

## 🔧 Solución de Problemas

### MySQL no inicia

1. Verificar logs: `docker-compose logs db`
2. Verificar que el puerto 3306 no esté en uso
3. Eliminar volúmenes y reiniciar: `docker-compose down -v && docker-compose up -d`

### Error de conexión desde la API

1. Verificar que MySQL esté corriendo: `docker-compose ps`
2. Verificar que el healthcheck esté OK: `docker inspect gesfer_api_db | grep -A 10 Health`
3. Probar conexión manual con Adminer
4. Verificar que la cadena de conexión en `appsettings.json` sea correcta

### Puerto ya en uso

Si el puerto 3306 está ocupado, cambiar en `docker-compose.yml`:

```yaml
ports:
  - "3307:3306"  # Usar 3307 externamente
```

Y actualizar `appsettings.json`:

```json
"DefaultConnection": "Server=localhost;Port=3307;..."
```

## 📊 Comandos Útiles

```bash
# Ver uso de recursos
docker stats

# Entrar al contenedor MySQL
docker exec -it gesfer_api_db bash

# Backup de la base de datos
docker exec gesfer_api_db mysqldump -u scrapuser -pscrappassword ScrapDb > backup.sql

# Restaurar base de datos
docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb < backup.sql

# Ver variables de entorno
docker exec gesfer_api_db env
```

## ✅ Checklist de Verificación

- [ ] Docker Desktop está corriendo
- [ ] `docker-compose up -d` ejecutado sin errores
- [ ] MySQL healthcheck está OK: `docker inspect gesfer_api_db | grep -A 5 Health`
- [ ] Puedo conectar desde Adminer: `http://localhost:8080`
- [ ] La API puede conectarse (verificar logs de la aplicación)
- [ ] Las migraciones se aplican correctamente

