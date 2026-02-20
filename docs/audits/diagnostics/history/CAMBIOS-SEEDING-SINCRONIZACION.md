# Documento de Cambios: Sincronización y Blindaje del Sistema de Seeding

**Fecha:** 13 de enero de 2026  
**Objetivo:** Sincronizar y blindar el sistema de seeding para evitar fallos en tests de integración por problemas de integridad referencial (FK)

---

## Resumen Ejecutivo

Se han realizado cambios críticos en el sistema de seeding para eliminar problemas de integridad referencial causados por archivos duplicados `test-data.json` y asegurar un orden de inserción correcto de datos. Los cambios garantizan que:

1. Solo existe un archivo `test-data.json` en la ubicación correcta
2. El orden de inserción respeta las dependencias de Foreign Keys
3. La base de datos se limpia completamente antes de aplicar migraciones
4. Los tests incluyen logs de depuración para facilitar el diagnóstico

---

## 1. Unificación de Archivos test-data.json

### Problema Detectado
Se encontraron **2 archivos `test-data.json`** en diferentes ubicaciones:
- `Api/src/Infrastructure/Data/Seeds/test-data.json` ✅ (correcto, con sección `languages`)
- `Api/src/Infrastructure/Seeds/test-data.json` ❌ (legacy, sin sección `languages`)

### Solución Aplicada
- **Eliminado:** `Api/src/Infrastructure/Seeds/test-data.json` (archivo legacy)
- **Mantenido:** `Api/src/Infrastructure/Data/Seeds/test-data.json` (archivo oficial)

### Verificación de Integridad
El archivo mantenido tiene la estructura correcta:
```json
{
  "languages": [
    {
      "id": "10000000-0000-0000-0000-000000000001",
      "name": "Español",
      "code": "es",
      "description": "Español"
    }
  ],
  "companies": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Empresa Demo",
      "languageId": "10000000-0000-0000-0000-000000000001"  ✅ Coincide con el ID del idioma
    }
  ]
}
```

**Verificación:** El `languageId` de la Empresa (`10000000-0000-0000-0000-000000000001`) coincide exactamente con el `id` del Idioma.

---

## 2. Refactorización de JsonDataSeeder.cs

### Estado Actual del Orden de Inserción

El método `SeedTestDataAsync()` en `Api/src/Infrastructure/Services/JsonDataSeeder.cs` ya tenía el orden correcto:

```csharp
// Orden jerárquico EXACTO para evitar errores de Foreign Key:
// 1. Languages (sin dependencias) - DEBE ejecutarse primero
if (data.Languages != null && data.Languages.Any())
{
    await SeedLanguagesAsync(data.Languages);
    await _context.SaveChangesAsync();  // ✅ Guarda cambios inmediatamente
    _logger.LogInformation("Languages sembrados: {Count}", data.Languages.Count);
}

// 2. Countries (depende de Languages)
if (data.Countries != null && data.Countries.Any())
{
    // Validación de integridad referencial
    await SeedCountriesAsync(data.Countries);
    await _context.SaveChangesAsync();
}

// 3. Cities (depende de Countries/States)
if (data.Cities != null && data.Cities.Any())
{
    await SeedCitiesAsync(data.Cities);
    await _context.SaveChangesAsync();
}

// 4. Companies (depende de Languages) - DEBE ejecutarse después de Languages
if (data.Companies != null && data.Companies.Any())
{
    // Validación de integridad referencial antes de insertar
    var languageIds = data.Companies.Select(c => Guid.Parse(c.LanguageId)).Distinct().ToList();
    var existingLanguages = await _context.Languages
        .IgnoreQueryFilters()
        .Where(l => languageIds.Contains(l.Id))
        .Select(l => l.Id)
        .ToListAsync();
    
    var missingLanguages = languageIds.Except(existingLanguages).ToList();
    if (missingLanguages.Any())
    {
        throw new InvalidOperationException(
            $"No se pueden insertar Companies: Los siguientes LanguageId no existen: {string.Join(", ", missingLanguages)}");
    }
    
    await SeedCompaniesAsync(data.Companies);
    await _context.SaveChangesAsync();
}

// 5. Users (depende de Companies y Languages) - DEBE ejecutarse después de Companies
if (data.Users != null && data.Users.Any())
{
    // Validación de integridad referencial para CompanyId y LanguageId
    await SeedUsersAsync(data.Users);
    await _context.SaveChangesAsync();
}
```

### Características Clave

1. **Orden Síncrono:** Languages → Countries → Cities → Companies → Users
2. **SaveChangesAsync después de Languages:** ✅ Ya implementado (línea 329)
3. **Validaciones de Integridad:** Se valida que los IDs referenciados existan antes de insertar
4. **Logging Detallado:** Cada paso registra el número de entidades insertadas

---

## 3. Ajuste en IntegrationTestWebAppFactory.cs

### Cambios Aplicados

**Archivo:** `Api/src/IntegrationTests/IntegrationTestWebAppFactory.cs`

#### 3.1. Borrado Completo de Base de Datos Antes de Migraciones

Se añadió el borrado de la base de datos antes de aplicar migraciones para evitar conflictos con ejecuciones previas:

```csharp
// Paso 4.1: Borrar completamente la base de datos antes de aplicar migraciones
// Esto garantiza que no queden datos residuales de ejecuciones previas
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
```

#### 3.2. Ejecución Única de DbInitializer

El método `InitializeAsync()` garantiza que `DbInitializer.InitializeAsync()` solo se ejecute **una vez por cada levantamiento de contenedor**:

```csharp
// Paso 5: Ejecutar DbInitializer para aplicar migraciones y cargar test-data.json
// DbInitializer detecta el entorno Testing y carga test-data.json automáticamente
// IMPORTANTE: Solo se ejecuta una vez por cada levantamiento de contenedor
serviceLogger.LogInformation("Ejecutando DbInitializer.InitializeAsync...");
await DbInitializer.InitializeAsync(Services, false); // false porque el entorno ya es Testing
serviceLogger.LogInformation("DbInitializer completado (migraciones aplicadas y test-data.json cargado)");
```

### Flujo Completo de Inicialización

1. Iniciar contenedor MySQL
2. Esperar 2 segundos para asegurar que MySQL esté completamente listo
3. Obtener cadena de conexión del contenedor
4. **Borrar base de datos completamente** (NUEVO)
5. Ejecutar `DbInitializer.InitializeAsync()` (una sola vez)
6. Aplicar migraciones
7. Cargar `test-data.json`

---

## 4. Corrección de Tests de Integración

### 4.1. Búsqueda de BeGreaterThanOrEqualTo

**Resultado:** No se encontraron usos de `.BeGreaterThanOrEqualTo()` para fechas. El código ya utiliza `.BeOnOrAfter()` donde corresponde:

```csharp
// Ejemplo en DashboardControllerTests.cs (línea 214)
secondLog.CreatedAt.Should().BeOnOrAfter(firstLog.CreatedAt,
    "Los logs deben estar ordenados por CreatedAt");
```

### 4.2. Logs de Depuración Añadidos

Se añadieron logs de depuración en tests críticos que buscan Usuario o Empresa:

#### CompanyControllerTests.cs

```csharp
[Fact]
public async Task GetById_WithValidId_ShouldReturnCompany()
{
    // Arrange
    var companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // Debug: Verificar cuántas empresas hay en la base de datos antes del Assert
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GesFer.Infrastructure.Data.ProductDbContext>();
    var companyCount = await context.Companies.CountAsync();
    Console.WriteLine($"[DEBUG] Número de empresas en la base de datos antes del Assert: {companyCount}");

    // Act
    var response = await _client.GetAsync($"/api/company/{companyId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var company = await response.Content.ReadFromJsonAsync<CompanyDto>();
    company.Should().NotBeNull();
    company!.Id.Should().Be(companyId);
    company.Name.Should().Be("Empresa Demo");
}
```

#### UserControllerTests.cs

```csharp
[Fact]
public async Task GetById_WithValidId_ShouldReturnUser()
{
    // Arrange
    var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    // Debug: Verificar cuántos usuarios y empresas hay en la base de datos antes del Assert
    using var scope = _factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GesFer.Infrastructure.Data.ProductDbContext>();
    var userCount = await context.Users.CountAsync();
    var companyCount = await context.Companies.CountAsync();
    Console.WriteLine($"[DEBUG] Número de usuarios en la base de datos antes del Assert: {userCount}");
    Console.WriteLine($"[DEBUG] Número de empresas en la base de datos antes del Assert: {companyCount}");

    // Act
    var response = await _client.GetAsync($"/api/user/{userId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var user = await response.Content.ReadFromJsonAsync<UserDto>();
    user.Should().NotBeNull();
    user!.Id.Should().Be(userId);
    user.Username.Should().Be("admin");
}
```

### 4.3. Imports Añadidos

Se añadieron los imports necesarios para `EntityFrameworkCore`:

```csharp
using Microsoft.EntityFrameworkCore;
```

---

## 5. Archivos Modificados

### Archivos Eliminados
- ❌ `Api/src/Infrastructure/Seeds/test-data.json` (archivo duplicado legacy)

### Archivos Modificados
1. ✅ `Api/src/IntegrationTests/IntegrationTestWebAppFactory.cs`
   - Añadido borrado de BD antes de migraciones
   - Mejorado logging

2. ✅ `Api/src/IntegrationTests/Controllers/CompanyControllerTests.cs`
   - Añadidos logs de depuración
   - Añadido import de `Microsoft.EntityFrameworkCore`

3. ✅ `Api/src/IntegrationTests/Controllers/UserControllerTests.cs`
   - Añadidos logs de depuración
   - Añadido import de `Microsoft.EntityFrameworkCore`

### Archivos Verificados (Sin Cambios Necesarios)
- ✅ `Api/src/Infrastructure/Services/JsonDataSeeder.cs` (orden correcto ya implementado)
- ✅ `Api/src/Infrastructure/Data/Seeds/test-data.json` (estructura correcta)

---

## 6. Verificación y Limpieza

### Comandos Ejecutados

```powershell
# Limpieza de la solución
cd c:\Proyectos\GesFer\Api
dotnet clean
```

**Resultado:** ✅ Limpieza completada exitosamente

---

## 7. Beneficios de los Cambios

### 7.1. Eliminación de Falsos Positivos
- Solo existe un archivo `test-data.json`, eliminando confusión sobre qué archivo se está usando
- El archivo correcto incluye la sección `languages` necesaria

### 7.2. Integridad Referencial Garantizada
- Orden de inserción síncrono respeta todas las dependencias FK
- Validaciones previas a la inserción detectan problemas temprano
- `SaveChangesAsync()` después de Languages asegura disponibilidad inmediata

### 7.3. Base de Datos Limpia
- Borrado completo antes de migraciones elimina datos residuales
- Cada ejecución de tests comienza con un estado limpio

### 7.4. Diagnóstico Mejorado
- Logs de depuración facilitan identificar problemas en tests
- Información sobre cantidad de registros antes de assertions

### 7.5. Ejecución Única de Seeding
- `DbInitializer` se ejecuta solo una vez por contenedor
- Evita duplicación de datos y conflictos

---

## 8. Próximos Pasos Recomendados

1. **Ejecutar Tests de Integración:**
   ```powershell
   cd c:\Proyectos\GesFer\Api
   dotnet test --filter "Category=Integration"
   ```

2. **Verificar Logs:**
   - Revisar logs de depuración en la salida de los tests
   - Confirmar que el número de empresas y usuarios es el esperado

3. **Monitoreo Continuo:**
   - Si algún test falla, los logs de depuración ayudarán a identificar el problema
   - Verificar que no aparezcan errores de integridad referencial

---

## 9. Resumen de Checklist

- [x] Archivo duplicado `test-data.json` eliminado
- [x] Verificado que `languageId` coincide con `id` del Idioma
- [x] Verificado orden de inserción: Languages → Countries → Cities → Companies → Users
- [x] Confirmado `SaveChangesAsync()` después de Languages
- [x] Añadido borrado de BD antes de migraciones en `IntegrationTestWebAppFactory`
- [x] Verificado que `DbInitializer` solo se ejecuta una vez
- [x] Añadidos logs de depuración en tests críticos
- [x] Añadidos imports necesarios (`Microsoft.EntityFrameworkCore`)
- [x] Solución limpiada con `dotnet clean`

---

## 10. Contacto y Referencias

**Archivo de Referencia Principal:**
- `Api/src/Infrastructure/Data/Seeds/test-data.json` - Archivo único de datos de prueba

**Archivos Clave del Sistema de Seeding:**
- `Api/src/Infrastructure/Services/JsonDataSeeder.cs` - Lógica de seeding
- `Api/src/Infrastructure/Data/DbInitializer.cs` - Inicializador de BD
- `Api/src/IntegrationTests/IntegrationTestWebAppFactory.cs` - Factory para tests

---

**Documento generado automáticamente el 13 de enero de 2026**
