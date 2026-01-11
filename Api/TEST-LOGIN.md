# Cómo Probar el Endpoint de Login

## 📋 Datos de Prueba

Después de ejecutar el script de configuración, puedes usar estos datos:

### Credenciales de Prueba

```json
{
  "empresa": "Empresa Demo",
  "usuario": "admin",
  "contraseña": "admin123"
}
```

## 🚀 Métodos para Probar

### 1. Usando Swagger UI (Recomendado)

1. Inicia la API desde Visual Studio (F5)
2. Abre el navegador en: `http://localhost:5000`
3. Busca el endpoint `POST /api/auth/login`
4. Haz clic en "Try it out"
5. Ingresa el JSON:
```json
{
  "empresa": "Empresa Demo",
  "usuario": "admin",
  "contraseña": "admin123"
}
```
6. Haz clic en "Execute"
7. Deberías ver una respuesta 200 con los datos del usuario y sus permisos

### 2. Usando cURL

```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"empresa\":\"Empresa Demo\",\"usuario\":\"admin\",\"contraseña\":\"admin123\"}"
```

### 3. Usando PowerShell

```powershell
$body = @{
    empresa = "Empresa Demo"
    usuario = "admin"
    contraseña = "admin123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

### 4. Usando Postman

1. Crea una nueva petición POST
2. URL: `http://localhost:5000/api/auth/login`
3. Headers: `Content-Type: application/json`
4. Body (raw JSON):
```json
{
  "empresa": "Empresa Demo",
  "usuario": "admin",
  "contraseña": "admin123"
}
```

## ✅ Respuesta Exitosa (200 OK)

```json
{
  "userId": "99999999-9999-9999-9999-999999999999",
  "username": "admin",
  "firstName": "Administrador",
  "lastName": "Sistema",
  "companyId": "11111111-1111-1111-1111-111111111111",
  "companyName": "Empresa Demo",
  "permissions": [
    "users.read",
    "users.write",
    "articles.read",
    "articles.write",
    "purchases.read"
  ],
  "token": ""
}
```

## ❌ Respuesta de Error (401 Unauthorized)

```json
{
  "message": "Credenciales inválidas"
}
```

## 📝 Pasos Previos

### Opción 1: Script Automático (Recomendado)

```powershell
.\scripts\setup-database.ps1
```

Este script:
1. Verifica que Docker esté corriendo
2. Verifica que MySQL esté disponible
3. Verifica si las tablas existen
4. Inserta los datos de prueba

### Opción 2: Manual

1. **Asegúrate de que Docker esté corriendo:**
   ```powershell
   docker-compose ps
   ```

2. **Ejecuta las migraciones:**
   - Las migraciones se ejecutan automáticamente al iniciar la API en desarrollo
   - O manualmente:
   ```powershell
   cd src/Api
   dotnet ef database update --project ../Infrastructure/GesFer.Infrastructure.csproj
   ```

3. **Carga los datos iniciales:**
   - **Opción A (Recomendada)**: Ejecuta la opción 1 de la consola (Inicialización completa)
     - Esto aplicará migraciones y cargará datos desde archivos JSON automáticamente
   
   - **Opción B**: Inicia la API en modo Development
     - Los datos se cargarán automáticamente mediante `DbInitializer`
   
   - **Opción C**: Usa la opción 6 de la consola (Ejecutar seeds de datos)
     - Permite ejecutar solo datos maestros, solo datos de muestra, o todos los seeds
   
   **Nota**: Los datos ahora se gestionan mediante archivos JSON en `Api/src/Infrastructure/Data/Seeds/`
   - `master-data.json` - Datos maestros (idiomas, permisos, grupos, usuario admin)
   - `demo-data.json` - Datos de demostración (empresa, usuarios, clientes, proveedores)

## 🔍 Verificar Datos Insertados

```powershell
# Verificar empresa
docker exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e "SELECT * FROM Companies WHERE Name = 'Empresa Demo';"

# Verificar usuario
docker exec gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb -e "SELECT Username, FirstName, LastName FROM Users WHERE Username = 'admin';"
```

## 🛠️ Generar Nuevo Hash de Contraseña

Si necesitas crear un usuario con otra contraseña, puedes crear un endpoint temporal o usar este código C# en un proyecto de consola:

```csharp
using BCrypt.Net;

var password = "tu_contraseña";
var hash = BCrypt.HashPassword(password, BCrypt.GenerateSalt(11));
Console.WriteLine($"Hash: {hash}");
```
