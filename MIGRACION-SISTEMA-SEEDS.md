# Migración del Sistema de Seeds: SQL → JSON

**Fecha:** 11 de Enero de 2025  
**Estado:** ✅ **COMPLETADO**

## 📋 Resumen de Cambios

### ✅ Sistema Anterior (SQL - DEPRECADO)
- **Ubicación:** `Api/scripts/*.sql`
- **Archivos:** `master-data.sql`, `sample-data.sql`, `test-data.sql`, `seed-data.sql`
- **Uso:** Scripts SQL ejecutados directamente en MySQL vía `docker exec`
- **Problemas:**
  - No idempotente (duplicaba datos)
  - Difícil de mantener
  - No respetaba soft delete
  - Requería ejecución manual

### ✅ Sistema Nuevo (JSON - ACTIVO)
- **Ubicación:** `Api/src/Infrastructure/Data/Seeds/`
- **Archivos:** `master-data.json`, `demo-data.json`, `test-data.json`
- **Uso:** Archivos JSON procesados por `JsonDataSeeder` usando `System.Text.Json`
- **Ventajas:**
  - ✅ Completamente idempotente
  - ✅ Respeta soft delete
  - ✅ Fácil de editar (solo JSON)
  - ✅ Automático al arrancar en Development
  - ✅ Mismo sistema que la API

## 🔄 Cambios Implementados

### 1. Consola (GesFer.Console)

#### SeedService.cs - **COMPLETAMENTE REESCRITO**
- ❌ **ANTES:** Ejecutaba scripts SQL vía `docker exec`
- ✅ **AHORA:** Usa `JsonDataSeeder` y `DbInitializer`
- **Métodos actualizados:**
  - `ExecuteMasterDataAsync()` → Carga desde `master-data.json`
  - `ExecuteSampleDataAsync()` → Carga desde `demo-data.json`
  - `ExecuteTestDataAsync()` → Carga desde `test-data.json`
  - `ExecuteAllSeedsAsync()` → Usa el mismo flujo que `DbInitializer`

#### MenuService.cs - **ACTUALIZADO**
- **Opción 1 (Inicialización completa):**
  - ❌ **ANTES:** Pasos 8 y 9 separados (migraciones + seeds SQL)
  - ✅ **AHORA:** Paso 8 unificado usando `DbInitializer.InitializeAsync()`
  - **Nuevo método:** `ExecuteDatabaseInitializationAsync()` que usa `DbInitializer`

#### GesFer.Console.csproj - **ACTUALIZADO**
- ✅ Agregadas referencias necesarias:
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.Configuration.Json`
  - `Microsoft.Extensions.Configuration.EnvironmentVariables`
  - `Microsoft.Extensions.DependencyInjection`
  - `Microsoft.Extensions.Logging`
  - `Microsoft.Extensions.Logging.Console`
  - `Pomelo.EntityFrameworkCore.MySql`
- ✅ Agregada referencia al proyecto `GesFer.Infrastructure`

### 2. API (GesFer.Api)

#### Program.cs - **YA ACTUALIZADO**
- ✅ Usa `DbInitializer.InitializeAsync()` automáticamente en Development
- ✅ Aplica migraciones y carga datos desde JSON

#### DbInitializer.cs - **NUEVO**
- ✅ Clase estática profesionalizada
- ✅ Aplica migraciones automáticamente
- ✅ Carga datos desde JSON de forma idempotente
- ✅ Crea/verifica usuario administrativo

### 3. JsonDataSeeder.cs - **ACTUALIZADO**
- ✅ Prioriza ubicación `Data/Seeds/` sobre `Seeds/` (legacy)
- ✅ Mantiene compatibilidad con ubicación anterior
- ✅ Logging mejorado

## 🚫 Scripts SQL - Estado

### Scripts SQL Identificados (LEGACY - NO SE USAN)

Los siguientes archivos SQL **existen pero NO se usan** en el código activo:

1. `Api/scripts/master-data.sql` - **LEGACY**
2. `Api/scripts/sample-data.sql` - **LEGACY**
3. `Api/scripts/test-data.sql` - **LEGACY**
4. `Api/scripts/seed-data.sql` - **LEGACY**
5. `Api/scripts/seed-all-data.sql` - **LEGACY**

### Verificación de Uso

✅ **NO se encontraron referencias activas a scripts SQL en:**
- `GesFer.Console/Services/SeedService.cs` - ✅ Actualizado a JSON
- `GesFer.Console/Services/MenuService.cs` - ✅ Usa DbInitializer
- `Api/src/Api/Program.cs` - ✅ Usa DbInitializer
- `Api/src/Infrastructure/Data/DbInitializer.cs` - ✅ Solo usa JSON

⚠️ **Referencias encontradas (solo en documentación/comentarios):**
- `Api/scripts/*.ps1` - Scripts PowerShell que mencionan SQL (legacy)
- `Api/TEST-LOGIN.md` - Documentación que menciona SQL (legacy)
- `Api/scripts/README-DATOS-INICIALES.md` - Documentación legacy
- Varios archivos `.md` con referencias históricas

## ✅ Verificación Final

### Compilación
- ✅ `GesFer.Console` compila sin errores
- ✅ `GesFer.Api` compila sin errores
- ✅ Todas las referencias resueltas correctamente

### Funcionalidad
- ✅ Opción 1 de la consola usa `DbInitializer` (migraciones + seeding JSON)
- ✅ SeedService completamente reescrito para usar JSON
- ✅ No hay código activo que ejecute scripts SQL para seeding
- ✅ Sistema completamente idempotente

### Archivos Modificados

1. ✅ `GesFer.Console/Services/SeedService.cs` - **REESCRITO COMPLETAMENTE**
2. ✅ `GesFer.Console/Services/MenuService.cs` - **ACTUALIZADO** (opción 1)
3. ✅ `GesFer.Console/GesFer.Console.csproj` - **ACTUALIZADO** (referencias)
4. ✅ `Api/src/Infrastructure/Data/DbInitializer.cs` - **NUEVO**
5. ✅ `Api/src/Api/Program.cs` - **YA ACTUALIZADO** (anteriormente)
6. ✅ `Api/src/Infrastructure/Services/JsonDataSeeder.cs` - **YA ACTUALIZADO** (anteriormente)

## 📝 Notas Importantes

### Scripts SQL Legacy
- Los scripts SQL en `Api/scripts/*.sql` **NO se eliminan** por ahora
- Se mantienen por compatibilidad/historial
- **NO se usan** en el código activo
- Se pueden eliminar en el futuro cuando se confirme que no son necesarios

### Migración de Datos
Si tienes datos en scripts SQL que quieres migrar a JSON:
1. Abre el script SQL correspondiente
2. Extrae los datos INSERT
3. Convierte a formato JSON siguiendo el formato de `master-data.json` o `demo-data.json`
4. Añade al archivo JSON correspondiente en `Data/Seeds/`

### Compatibilidad
- El sistema mantiene compatibilidad con la ubicación legacy `Infrastructure/Seeds/`
- Prioriza `Data/Seeds/` pero puede usar la ubicación anterior si no encuentra la nueva
- Logging indica qué ubicación se está usando

## 🎯 Resultado Final

✅ **Sistema completamente migrado de SQL a JSON**
✅ **Opción 1 de la consola usa el nuevo sistema**
✅ **No hay código activo que use scripts SQL para seeding**
✅ **Sistema idempotente y profesionalizado**

---

**Estado:** ✅ **MIGRACIÓN COMPLETADA**
