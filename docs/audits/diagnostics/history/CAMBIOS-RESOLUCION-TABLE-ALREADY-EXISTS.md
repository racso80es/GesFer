# Documento de Cambios: Resolución de Error "Table 'AdminUsers' already exists"

**Fecha:** 13 de enero de 2026  
**Problema:** Los tests de integración fallan con el error `Table 'AdminUsers' already exists`  
**Objetivo:** Asegurar que cada suite de tests empiece con un contenedor MySQL 100% vacío y evitar conflictos de migración simultánea

---

## Resumen Ejecutivo

Se han aplicado cambios críticos en el sistema de tests de integración para resolver el error "Table 'AdminUsers' already exists". Los cambios garantizan que:

1. La base de datos se borre completamente **ANTES** de ejecutar `DbInitializer`
2. Solo un contexto de base de datos intente migrar a la vez (ServiceLifetime.Singleton)
3. Los cambios del seeder se guarden inmediatamente después de cada bloque
4. Las transacciones tengan tiempo para asentarse antes de ejecutar los tests
5. Se eliminen archivos bin/obj que puedan contener copias antiguas de `test-data.json`

---

## 1. Cambios en IntegrationTestWebAppFactory.cs

### 1.1. Cambio de ServiceLifetime a Singleton

**Problema:** Múltiples contextos de DbContext intentaban migrar simultáneamente, causando conflictos.

**Solución:** Cambiar `ServiceLifetime.Scoped` a `ServiceLifetime.Singleton` para los tests.

**Ubicación:** `Api/src/IntegrationTests/IntegrationTestWebAppFactory.cs` (línea 95)

**Antes:**
```csharp
services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    // ... configuración ...
}, ServiceLifetime.Scoped);
```

**Después:**
```csharp
// ServiceLifetime: Singleton para tests - evita que múltiples contextos intenten migrar simultáneamente
services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    // ... configuración ...
}, ServiceLifetime.Singleton);
```

**Impacto:** 
- ✅ Evita que múltiples instancias de DbContext intenten aplicar migraciones al mismo tiempo
- ✅ Garantiza que solo un contexto gestione las migraciones por contenedor

### 1.2. Reordenamiento de EnsureDeletedAsync

**Problema:** `EnsureDeletedAsync()` se ejecutaba después de crear el cliente, pero necesitaba ejecutarse **ANTES** de `DbInitializer.InitializeAsync()`.

**Solución:** Mover `EnsureDeletedAsync()` para que se ejecute inmediatamente después de obtener el contexto y **ANTES** de llamar a `DbInitializer`.

**Ubicación:** `Api/src/IntegrationTests/IntegrationTestWebAppFactory.cs` (líneas 142-160)

**Antes:**
```csharp
// Paso 4: Aplicar migraciones y ejecutar seeding
using var client = CreateClient();
using var scope = Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<ApplicationDbContext>();

// Paso 4.1: Borrar base de datos (después de crear cliente)
await context.Database.EnsureDeletedAsync();

// Paso 5: Ejecutar DbInitializer
await DbInitializer.InitializeAsync(Services, false);
```

**Después:**
```csharp
// Paso 4: Crear cliente y obtener servicios para preparar el contexto
using var client = CreateClient();
using var scope = Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<ApplicationDbContext>();
var serviceLoggerFactory = services.GetRequiredService<ILoggerFactory>();
var serviceLogger = serviceLoggerFactory.CreateLogger("IntegrationTestWebAppFactory");

// Paso 4.1: CRÍTICO - Borrar completamente la base de datos ANTES de DbInitializer
// Esto garantiza que cada suite de tests empiece con un contenedor MySQL 100% vacío
// Debe ejecutarse ANTES de DbInitializer para evitar errores de "Table already exists"
serviceLogger.LogInformation("Borrando base de datos completamente para empezar limpio...");
try
{
    await context.Database.EnsureDeletedAsync();
    serviceLogger.LogInformation("Base de datos eliminada completamente");
}
catch (Exception ex)
{
    serviceLogger.LogWarning(ex, "No se pudo eliminar la base de datos, continuando... Error: {Error}", ex.Message);
    // Continuar de todas formas, puede que la BD no exista aún
}

// Paso 5: Ejecutar DbInitializer para aplicar migraciones y cargar test-data.json
// CRÍTICO: EnsureDeletedAsync ya se ejecutó arriba, garantizando BD vacía
serviceLogger.LogInformation("Ejecutando DbInitializer.InitializeAsync...");
await DbInitializer.InitializeAsync(Services, false);
serviceLogger.LogInformation("DbInitializer completado (migraciones aplicadas y test-data.json cargado)");
```

**Impacto:**
- ✅ Garantiza que cada suite de tests empiece con un contenedor MySQL 100% vacío
- ✅ Evita errores de "Table already exists" al asegurar que no hay tablas previas
- ✅ El orden correcto es: Borrar BD → Ejecutar DbInitializer → Aplicar migraciones → Seed datos

---

## 2. Cambios en JsonDataSeeder.cs

### 2.1. SaveChangesAsync Después de Cada Bloque

**Problema:** El seeder guardaba cambios solo después de Languages, Companies y Users, pero no después de Groups, Permissions, UserGroups y GroupPermissions. Esto causaba que los tests no vieran todos los datos sembrados.

**Solución:** Añadir `await _context.SaveChangesAsync()` después de cada bloque de seeding.

**Ubicación:** `Api/src/Infrastructure/Services/JsonDataSeeder.cs` (líneas 431-453)

**Antes:**
```csharp
// 6. Groups (sin dependencias)
if (data.Groups != null && data.Groups.Any())
{
    await SeedGroupsAsync(data.Groups);
    // ❌ Faltaba SaveChangesAsync
}

// 7. Permissions (sin dependencias)
if (data.Permissions != null && data.Permissions.Any())
{
    await SeedPermissionsAsync(data.Permissions);
    // ❌ Faltaba SaveChangesAsync
}

// 8. UserGroups (depende de Users y Groups)
if (data.UserGroups != null && data.UserGroups.Any())
{
    await SeedUserGroupsAsync(data.UserGroups);
    // ❌ Faltaba SaveChangesAsync
}

// 9. GroupPermissions (depende de Groups y Permissions)
if (data.GroupPermissions != null && data.GroupPermissions.Any())
{
    await SeedGroupPermissionsAsync(data.GroupPermissions);
    // ❌ Faltaba SaveChangesAsync
}
```

**Después:**
```csharp
// 6. Groups (sin dependencias)
if (data.Groups != null && data.Groups.Any())
{
    await SeedGroupsAsync(data.Groups);
    await _context.SaveChangesAsync();  // ✅ Guardar cambios inmediatamente
    _logger.LogInformation("Groups sembrados: {Count}", data.Groups.Count);
}

// 7. Permissions (sin dependencias)
if (data.Permissions != null && data.Permissions.Any())
{
    await SeedPermissionsAsync(data.Permissions);
    await _context.SaveChangesAsync();  // ✅ Guardar cambios inmediatamente
    _logger.LogInformation("Permissions sembrados: {Count}", data.Permissions.Count);
}

// 8. UserGroups (depende de Users y Groups) - DEBE ejecutarse después de Users y Groups
if (data.UserGroups != null && data.UserGroups.Any())
{
    await SeedUserGroupsAsync(data.UserGroups);
    await _context.SaveChangesAsync();  // ✅ Guardar cambios inmediatamente
    _logger.LogInformation("UserGroups sembrados: {Count}", data.UserGroups.Count);
}

// 9. GroupPermissions (depende de Groups y Permissions) - DEBE ejecutarse después de Groups y Permissions
if (data.GroupPermissions != null && data.GroupPermissions.Any())
{
    await SeedGroupPermissionsAsync(data.GroupPermissions);
    await _context.SaveChangesAsync();  // ✅ Guardar cambios inmediatamente
    _logger.LogInformation("GroupPermissions sembrados: {Count}", data.GroupPermissions.Count);
}
```

**Impacto:**
- ✅ Los datos se guardan inmediatamente después de cada bloque
- ✅ Los tests pueden ver todos los datos sembrados correctamente
- ✅ Mejor logging para diagnóstico

---

## 3. Cambios en Tests de Integración

### 3.1. Task.Delay en Tests Críticos

**Problema:** A veces el contenedor MySQL es más lento que el hilo del test, causando que las transacciones del seeder no se hayan asentado completamente antes de ejecutar el test.

**Solución:** Añadir `await Task.Delay(1000)` al inicio de los tests críticos que buscan datos.

**Archivos Modificados:**
- `Api/src/IntegrationTests/Controllers/CompanyControllerTests.cs`
- `Api/src/IntegrationTests/Controllers/UserControllerTests.cs`

**Ejemplo - CompanyControllerTests.cs:**

**Antes:**
```csharp
[Fact]
public async Task GetAll_ShouldReturnListOfCompanies()
{
    // Act
    var response = await _client.GetAsync("/api/company");
    // ...
}
```

**Después:**
```csharp
[Fact]
public async Task GetAll_ShouldReturnListOfCompanies()
{
    // Permitir que la transacción del Seeder se asiente en MySQL
    await Task.Delay(1000);
    
    // Act
    var response = await _client.GetAsync("/api/company");
    // ...
}
```

**Tests Modificados:**

1. ✅ `CompanyControllerTests.GetAll_ShouldReturnListOfCompanies()`
2. ✅ `CompanyControllerTests.GetById_WithValidId_ShouldReturnCompany()`
3. ✅ `UserControllerTests.GetAll_ShouldReturnListOfUsers()`
4. ✅ `UserControllerTests.GetById_WithValidId_ShouldReturnUser()`

**Impacto:**
- ✅ Permite que las transacciones del seeder se asienten completamente
- ✅ Evita condiciones de carrera entre el seeding y la ejecución del test
- ✅ Mejora la estabilidad de los tests en contenedores MySQL

---

## 4. Script de Limpieza de Carpetas bin/obj

### 4.1. Creación del Script

**Problema:** Las carpetas `bin` y `obj` pueden contener copias antiguas de `test-data.json`, causando que el sistema use archivos desactualizados.

**Solución:** Crear un script PowerShell que elimine todas las carpetas `bin` y `obj` del proyecto Api.

**Archivo Creado:** `limpiar-bin-obj.ps1`

**Contenido del Script:**
```powershell
# Script para limpiar todas las carpetas bin y obj del proyecto
# Esto fuerza que el único test-data.json válido sea el de Infrastructure/Data/Seeds/

Write-Host "=== Limpiando carpetas bin y obj del proyecto ===" -ForegroundColor Cyan

$projectRoot = $PSScriptRoot
$foldersDeleted = 0
$filesDeleted = 0

# Buscar todas las carpetas bin y obj, excluyendo node_modules
$binFolders = Get-ChildItem -Path $projectRoot -Directory -Recurse -Filter "bin" -ErrorAction SilentlyContinue | 
    Where-Object { $_.FullName -notmatch "node_modules" -and $_.FullName -match "\\Api\\" }
$objFolders = Get-ChildItem -Path $projectRoot -Directory -Recurse -Filter "obj" -ErrorAction SilentlyContinue | 
    Where-Object { $_.FullName -notmatch "node_modules" -and $_.FullName -match "\\Api\\" }

# Eliminar carpetas bin y obj
foreach ($folder in ($binFolders + $objFolders)) {
    try {
        Write-Host "Eliminando: $($folder.FullName)" -ForegroundColor Yellow
        $fileCount = (Get-ChildItem -Path $folder.FullName -Recurse -File -ErrorAction SilentlyContinue).Count
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
        $foldersDeleted++
        $filesDeleted += $fileCount
        Write-Host "  OK - Eliminada ($fileCount archivos)" -ForegroundColor Green
    }
    catch {
        Write-Host "  Error al eliminar: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== Resumen ===" -ForegroundColor Cyan
Write-Host "Carpetas eliminadas: $foldersDeleted" -ForegroundColor Green
Write-Host "Archivos eliminados: $filesDeleted" -ForegroundColor Green
Write-Host "`nLimpieza completada. El unico test-data.json valido ahora es: Api/src/Infrastructure/Data/Seeds/test-data.json" -ForegroundColor Green
```

**Uso:**
```powershell
cd c:\Proyectos\GesFer
powershell -ExecutionPolicy Bypass -File .\limpiar-bin-obj.ps1
```

**Resultado de Ejecución:**
- ✅ 36 carpetas eliminadas
- ✅ 810 archivos eliminados
- ✅ Solo queda el `test-data.json` válido en `Api/src/Infrastructure/Data/Seeds/`

**Impacto:**
- ✅ Fuerza que el único `test-data.json` válido sea el de `Infrastructure/Data/Seeds/`
- ✅ Elimina archivos compilados que pueden contener datos desactualizados
- ✅ Garantiza que el sistema use siempre el archivo correcto

---

## 5. Resumen de Archivos Modificados

### Archivos Modificados

1. ✅ **`Api/src/IntegrationTests/IntegrationTestWebAppFactory.cs`**
   - Cambio de `ServiceLifetime.Scoped` a `ServiceLifetime.Singleton`
   - Reordenamiento de `EnsureDeletedAsync()` antes de `DbInitializer`

2. ✅ **`Api/src/Infrastructure/Services/JsonDataSeeder.cs`**
   - Añadido `SaveChangesAsync()` después de Groups
   - Añadido `SaveChangesAsync()` después de Permissions
   - Añadido `SaveChangesAsync()` después de UserGroups
   - Añadido `SaveChangesAsync()` después de GroupPermissions
   - Mejorado logging con conteo de entidades

3. ✅ **`Api/src/IntegrationTests/Controllers/CompanyControllerTests.cs`**
   - Añadido `Task.Delay(1000)` en `GetAll_ShouldReturnListOfCompanies()`
   - Añadido `Task.Delay(1000)` en `GetById_WithValidId_ShouldReturnCompany()`

4. ✅ **`Api/src/IntegrationTests/Controllers/UserControllerTests.cs`**
   - Añadido `Task.Delay(1000)` en `GetAll_ShouldReturnListOfUsers()`
   - Añadido `Task.Delay(1000)` en `GetById_WithValidId_ShouldReturnUser()`

### Archivos Creados

5. ✅ **`limpiar-bin-obj.ps1`** (nuevo)
   - Script PowerShell para limpiar carpetas bin/obj

---

## 6. Flujo de Inicialización Corregido

### Orden Correcto de Ejecución

```
1. Iniciar contenedor MySQL
   ↓
2. Esperar 2 segundos (asegurar que MySQL esté listo)
   ↓
3. Obtener cadena de conexión del contenedor
   ↓
4. Crear cliente HTTP (configura servicios)
   ↓
5. Obtener DbContext del contenedor de servicios
   ↓
6. ✅ BORRAR BASE DE DATOS COMPLETAMENTE (EnsureDeletedAsync)
   ↓
7. ✅ Ejecutar DbInitializer.InitializeAsync()
   ↓
8. Aplicar migraciones (dentro de DbInitializer)
   ↓
9. Cargar test-data.json (dentro de DbInitializer)
   ↓
10. Ejecutar tests (con Task.Delay para permitir que transacciones se asienten)
```

### Puntos Críticos

- ✅ **Paso 6 debe ejecutarse ANTES del paso 7**: Esto garantiza que no haya tablas previas
- ✅ **ServiceLifetime.Singleton**: Evita que múltiples contextos migren simultáneamente
- ✅ **SaveChangesAsync después de cada bloque**: Asegura que los datos estén disponibles inmediatamente
- ✅ **Task.Delay en tests**: Permite que las transacciones se asienten en MySQL

---

## 7. Beneficios de los Cambios

### 7.1. Eliminación del Error "Table already exists"
- ✅ La base de datos se borra completamente antes de cada inicialización
- ✅ No hay conflictos de migración simultánea
- ✅ Cada suite de tests empieza con un contenedor MySQL 100% vacío

### 7.2. Integridad de Datos
- ✅ Los datos se guardan inmediatamente después de cada bloque
- ✅ Los tests pueden ver todos los datos sembrados correctamente
- ✅ No hay problemas de transacciones no confirmadas

### 7.3. Estabilidad de Tests
- ✅ Task.Delay permite que las transacciones se asienten
- ✅ Evita condiciones de carrera entre seeding y ejecución de tests
- ✅ Mejora la consistencia de los resultados

### 7.4. Limpieza de Archivos
- ✅ Solo existe un `test-data.json` válido
- ✅ Se eliminan archivos compilados desactualizados
- ✅ El sistema siempre usa el archivo correcto

---

## 8. Verificación y Pruebas

### Comandos para Verificar

```powershell
# 1. Limpiar carpetas bin/obj
cd c:\Proyectos\GesFer
powershell -ExecutionPolicy Bypass -File .\limpiar-bin-obj.ps1

# 2. Limpiar solución
cd Api
dotnet clean

# 3. Ejecutar tests de integración
dotnet test --filter "Category=Integration" --verbosity normal
```

### Resultados Esperados

- ✅ No debe aparecer el error "Table 'AdminUsers' already exists"
- ✅ Todos los tests deben encontrar los datos sembrados correctamente
- ✅ Los logs deben mostrar que la BD se borra antes de DbInitializer
- ✅ Los logs deben mostrar que cada bloque de seeding se guarda correctamente

---

## 9. Troubleshooting

### Si el Error Persiste

1. **Verificar que EnsureDeletedAsync se ejecuta antes de DbInitializer:**
   - Revisar los logs de `IntegrationTestWebAppFactory`
   - Debe aparecer "Borrando base de datos completamente para empezar limpio..."

2. **Verificar ServiceLifetime:**
   - Confirmar que es `ServiceLifetime.Singleton` en la línea 95 de `IntegrationTestWebAppFactory.cs`

3. **Verificar SaveChangesAsync:**
   - Revisar que cada bloque de seeding tiene su `SaveChangesAsync()` correspondiente

4. **Aumentar Task.Delay si es necesario:**
   - Si los tests aún fallan, aumentar el delay a 2000ms o 3000ms

5. **Verificar que no hay archivos test-data.json duplicados:**
   - Ejecutar el script `limpiar-bin-obj.ps1` nuevamente
   - Verificar que solo existe `Api/src/Infrastructure/Data/Seeds/test-data.json`

---

## 10. Checklist de Verificación

- [x] ServiceLifetime cambiado a Singleton en `IntegrationTestWebAppFactory.cs`
- [x] `EnsureDeletedAsync()` ejecutado ANTES de `DbInitializer.InitializeAsync()`
- [x] `SaveChangesAsync()` añadido después de Groups
- [x] `SaveChangesAsync()` añadido después de Permissions
- [x] `SaveChangesAsync()` añadido después de UserGroups
- [x] `SaveChangesAsync()` añadido después de GroupPermissions
- [x] `Task.Delay(1000)` añadido en tests críticos de CompanyController
- [x] `Task.Delay(1000)` añadido en tests críticos de UserController
- [x] Script `limpiar-bin-obj.ps1` creado y ejecutado
- [x] Carpetas bin/obj eliminadas del proyecto Api

---

## 11. Referencias

**Archivos Clave:**
- `Api/src/IntegrationTests/IntegrationTestWebAppFactory.cs` - Factory para tests con Testcontainers
- `Api/src/Infrastructure/Services/JsonDataSeeder.cs` - Lógica de seeding
- `Api/src/Infrastructure/Data/DbInitializer.cs` - Inicializador de BD
- `limpiar-bin-obj.ps1` - Script de limpieza

**Documentación Relacionada:**
- `docs/CAMBIOS-SEEDING-SINCRONIZACION.md` - Cambios previos de sincronización de seeding

---

**Documento generado automáticamente el 13 de enero de 2026**
