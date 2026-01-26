# Solución de Problemas - GesFer API

## Problema: La página no carga al ejecutar

### Pasos de Diagnóstico

1. **Verificar que la aplicación esté corriendo**
   ```powershell
   # Ver procesos de .NET
   Get-Process | Where-Object {$_.ProcessName -like "*GesFer*" -or $_.ProcessName -like "*dotnet*"}
   ```

2. **Verificar que los puertos estén disponibles**
   ```powershell
   # Verificar puerto 5000
   netstat -ano | findstr :5000
   
   # Verificar puerto 5001
   netstat -ano | findstr :5001
   ```

3. **Verificar logs de la aplicación**
   - En Visual Studio: Ver la ventana de "Output" o "Debug Output"
   - Buscar errores relacionados con:
     - Conexión a MySQL
     - Migraciones
     - Inicio de la aplicación

4. **Probar acceso directo**
   - Abrir navegador en: `http://localhost:5000`
   - O: `http://localhost:5000/swagger`
   - O: `http://localhost:5000/api/health`

### Soluciones Comunes

#### 1. La aplicación no inicia por error de migraciones

**Síntoma:** La aplicación se queda colgada al iniciar

**Solución:** 
- Las migraciones ahora no bloquean el inicio
- Si hay error, la aplicación continuará sin aplicar migraciones
- Verificar logs para ver el error específico

#### 2. Puerto ya en uso

**Síntoma:** Error "Address already in use"

**Solución:**
```powershell
# Encontrar proceso usando el puerto
netstat -ano | findstr :5000

# Matar el proceso (reemplazar PID con el número encontrado)
taskkill /PID <PID> /F
```

#### 3. MySQL no está disponible

**Síntoma:** Error de conexión a la base de datos

**Solución:**
```powershell
# Verificar que Docker esté corriendo
docker-compose ps

# Si no está corriendo, iniciarlo
.\docker-start.ps1
```

#### 4. Certificado HTTPS no confiable

**Síntoma:** Error al acceder a `https://localhost:5001`

**Solución:**
```powershell
# Confiar en el certificado de desarrollo
dotnet dev-certs https --trust
```

#### 5. Swagger no carga

**Síntoma:** Página en blanco o error 404

**Solución:**
- Verificar que esté en modo Development
- Acceder directamente a: `http://localhost:5000/swagger/v1/swagger.json`
- Verificar logs para errores de Swagger

### Verificación Rápida

```powershell
# 1. Verificar Docker
docker-compose ps

# 2. Verificar que MySQL responda
docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword

# 3. Probar endpoint de health
curl http://localhost:5000/api/health

# 4. Ver logs de la aplicación (si está corriendo)
# En Visual Studio: Ver Output window
```

### URLs de Acceso

- **Swagger UI:** `http://localhost:5000` o `https://localhost:5001`
- **Health Check:** `http://localhost:5000/api/health`
- **Login:** `POST http://localhost:5000/api/auth/login`
- **Adminer (MySQL):** `http://localhost:8080`

### Logs Importantes a Revisar

1. **Al iniciar la aplicación:**
   - "Verificando conexión a la base de datos..."
   - "Aplicando migraciones..."
   - "Now listening on: http://localhost:5000"

2. **Errores comunes:**
   - "Unable to connect to any of the specified MySQL hosts"
   - "Database 'ScrapDb' does not exist"
   - "Access denied for user"

### Si Nada Funciona

1. Detener todos los procesos:
   ```powershell
   Get-Process | Where-Object {$_.ProcessName -like "*GesFer*"} | Stop-Process -Force
   ```

2. Limpiar y reconstruir:
   ```powershell
   dotnet clean
   dotnet build
   ```

3. Reiniciar Docker:
   ```powershell
   docker-compose down
   docker-compose up -d
   ```

4. Ejecutar desde línea de comandos para ver errores:
   ```powershell
   cd src/Api
   dotnet run
   ```

