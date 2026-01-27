# Documentación de Afectaciones - Extracción Materia Admin desde Product Backend

**Fecha:** 2026-01-27  
**Rama:** feat/domain-separation  
**Modo:** Senior Software Architect / Tekton Governance

## Objetivo

Extraer toda la "materia Admin" desde el backend de Product hacia el dominio Admin, estableciendo comunicación vía API HTTP (proxy) cuando Product necesite registrar logs o auditoría.

## Invariante S+ (Regla de Oro)

**Product NO puede conocer las tablas de Admin. Solo se comunica vía API.**

---

## PASO 2: Análisis de Afectaciones Backend

### 2.1 Controladores a Extraer/Mover

#### LogController.cs
**Ubicación actual:** `src/Product/Back/Api/Controllers/LogController.cs`  
**Ubicación destino:** `src/Admin/Back/Api/Controllers/LogController.cs` (ya existe, pero necesita actualización)

**Afectaciones:**
- ✅ Ya existe `src/Admin/Back/Api/Controllers/LogController.cs` pero usa `ApplicationDbContext` directamente
- ⚠️ El LogController en Product usa comandos (`GetLogsCommand`, `PurgeLogsCommand`) que **NO EXISTEN** en el código actual
- El LogController en Product tiene dependencias:
  - `ICommandHandler<GetLogsCommand, LogsPagedResponseDto>`
  - `ICommandHandler<PurgeLogsCommand, PurgeLogsResponseDto>`

**Acciones requeridas:**
1. Verificar si los comandos existen o deben crearse
2. Mover/crear comandos y handlers a Admin si no existen
3. Actualizar LogController en Admin para usar comandos (o mantener acceso directo a DbContext)
4. Eliminar LogController de Product
5. Crear servicio proxy en Product para llamar a Admin API cuando se necesite registrar logs

#### SetupController.cs
**Ubicación actual:** `src/Product/Back/Api/Controllers/SetupController.cs`  
**Ubicación destino:** `src/Admin/Back/Api/Controllers/SetupController.cs`

**Afectaciones:**
- SetupController realiza inicialización global del sistema (Docker, BD, seeds)
- Usa `ISetupService` ubicado en `src/Product/Back/Api/Services/SetupService.cs`
- El servicio tiene dependencias de:
  - `ApplicationDbContext` (Product)
  - `JsonDataSeeder` (Product)
  - `MasterDataSeeder` (Product)
  - Crea `AdminUser` manualmente

**Acciones requeridas:**
1. Mover `SetupController.cs` a Admin
2. Mover `SetupService.cs` e `ISetupService.cs` a Admin
3. Ajustar referencias de `ApplicationDbContext` (Product) a contexto compartido o Admin
4. Verificar que la creación de AdminUser se mantenga en Admin
5. Eliminar SetupController de Product

### 2.2 Servicios a Extraer/Mover

#### AuditLogService
**Ubicación actual:** `src/Product/Back/Infrastructure/Services/AuditLogService.cs`  
**Ubicación destino:** `src/Admin/Back/Infrastructure/Services/AuditLogService.cs` (ya existe)

**Afectaciones:**
- ✅ Ya existe en Admin pero usa `GesFer.Admin.Back.Domain.Entities.AuditLog`
- ⚠️ El servicio en Product usa `GesFer.Admin.Back.Domain.Entities.AuditLog` (namespace correcto)
- ⚠️ El servicio en Product usa `ApplicationDbContext` de Product que tiene `DbSet<AuditLog>`
- Usado por `DashboardController` en Product

**Acciones requeridas:**
1. Verificar que AuditLogService en Admin esté completo
2. Eliminar AuditLogService de Product
3. Crear servicio proxy en Product para llamar a Admin API cuando se necesite registrar auditoría
4. Actualizar DashboardController en Product para usar el proxy

#### IAuditLogService
**Ubicación actual:** `src/Product/Back/Infrastructure/Services/IAuditLogService.cs`  
**Ubicación destino:** `src/Admin/Back/Infrastructure/Services/IAuditLogService.cs` (ya existe)

**Acciones requeridas:**
1. Verificar que la interfaz en Admin sea compatible
2. Eliminar IAuditLogService de Product
3. Crear interfaz proxy en Product que llame a Admin API

### 2.3 Entidades y DbContext

#### AuditLog Entity
**Ubicación actual:** 
- `src/Product/Back/domain/Entities/AuditLog.cs` (NO EXISTE - usa Admin)
- `src/Admin/Back/domain/Entities/AuditLog.cs` (✅ EXISTE)

**Afectaciones:**
- `ApplicationDbContext` en Product tiene `DbSet<AuditLog> AuditLogs`
- Configuración `AuditLogConfiguration.cs` en Product

**Acciones requeridas:**
1. Eliminar `DbSet<AuditLog>` de `ApplicationDbContext` en Product
2. Eliminar `AuditLogConfiguration.cs` de Product
3. Verificar que Admin tenga su propio DbContext con AuditLogs
4. Crear migración para separar tablas si es necesario

#### Log Entity
**Ubicación actual:** 
- `src/Product/Back/domain/Entities/Log.cs` (verificar si existe)
- `src/Admin/Back/domain/Entities/Log.cs` (✅ EXISTE según LogController en Admin)

**Afectaciones:**
- `ApplicationDbContext` en Product tiene `DbSet<Log> Logs`
- Configuración `LogConfiguration.cs` en Product

**Acciones requeridas:**
1. **DECISIÓN ARQUITECTÓNICA:** ¿Los Logs (Serilog) son de Admin o compartidos?
   - Si son compartidos: mantener en Product pero Admin puede leerlos
   - Si son de Admin: mover a Admin completamente
2. Según decisión, eliminar o mantener `DbSet<Log>` en Product
3. Si se mueve a Admin, eliminar `LogConfiguration.cs` de Product

### 2.4 Dependencias en DependencyInjection

**Ubicación:** `src/Product/Back/Api/DependencyInjection.cs`

**Registros a eliminar:**
```csharp
services.AddScoped<IAuditLogService, AuditLogService>(); // Línea 65
```

**Registros a agregar:**
```csharp
// Servicio proxy para llamar a Admin API
services.AddHttpClient<IAdminLogProxyService, AdminLogProxyService>();
services.AddHttpClient<IAdminAuditLogProxyService, AdminAuditLogProxyService>();
```

### 2.5 Comandos y Handlers (Verificar Existencia)

**Comandos mencionados en LogController pero no encontrados:**
- `GetLogsCommand`
- `PurgeLogsCommand`
- `LogsPagedResponseDto`
- `PurgeLogsResponseDto`

**Acciones requeridas:**
1. Verificar si estos comandos existen en algún lugar
2. Si no existen, crear en Admin:
   - `src/Admin/Back/application/Commands/Log/GetLogsCommand.cs`
   - `src/Admin/Back/application/Commands/Log/PurgeLogsCommand.cs`
   - `src/Admin/Back/application/DTOs/Log/LogsPagedResponseDto.cs`
   - `src/Admin/Back/application/DTOs/Log/PurgeLogsResponseDto.cs`
   - Handlers correspondientes

### 2.6 Tests de Integración

**Archivos afectados:**
- `src/Product/Back/IntegrationTests/Controllers/SetupControllerTests.cs`
- `src/Product/Back/IntegrationTests/Controllers/DashboardControllerTests.cs` (usa AuditLogService)

**Acciones requeridas:**
1. Mover `SetupControllerTests.cs` a Admin
2. Actualizar `DashboardControllerTests.cs` para usar proxy en lugar de servicio directo
3. Crear tests para los servicios proxy

---

## PASO 2.1: Plan de Implementación

### Fase 1: Preparación
1. ✅ Verificar estructura de rutas (PASO 1)
2. ✅ Generar esta documentación
3. ⏳ Verificar existencia de comandos Log
4. ⏳ Verificar DbContext de Admin

### Fase 2: Extracción de Servicios
1. Crear servicios proxy en Product:
   - `IAdminLogProxyService` / `AdminLogProxyService`
   - `IAdminAuditLogProxyService` / `AdminAuditLogProxyService`
2. Mover AuditLogService a Admin (verificar si ya está completo)
3. Actualizar DashboardController en Product para usar proxy

### Fase 3: Extracción de Controladores
1. Mover SetupController y SetupService a Admin
2. Actualizar LogController en Admin (si usa comandos, crearlos)
3. Eliminar LogController de Product

### Fase 4: Limpieza de DbContext
1. Eliminar `DbSet<AuditLog>` de ApplicationDbContext en Product
2. Eliminar configuraciones de AuditLog y Log de Product (si aplica)
3. Crear migración si es necesario

### Fase 5: Actualización de Dependencias
1. Actualizar DependencyInjection en Product
2. Actualizar DependencyInjection en Admin
3. Actualizar tests

### Fase 6: Compilación y Verificación
1. `dotnet build` en Product/Back
2. `dotnet build` en Admin/Back
3. Verificar que no haya referencias cruzadas

---

## Notas Importantes

1. **Proxy API Pattern:** Product debe usar `IHttpClientFactory` para llamar a Admin API cuando necesite registrar logs/auditoría
2. **Configuración:** Agregar URL base de Admin API en `appsettings.json` de Product
3. **Autenticación:** Los proxies deben incluir tokens de autenticación si Admin API lo requiere
4. **Resiliencia:** Considerar circuit breaker o retry policies para las llamadas HTTP

---

## Checklist de Verificación

- [ ] Comandos Log creados en Admin (si no existen)
- [ ] Servicios proxy creados en Product
- [ ] AuditLogService movido/verificado en Admin
- [ ] SetupController y SetupService movidos a Admin
- [ ] LogController actualizado/verificado en Admin
- [ ] LogController eliminado de Product
- [ ] DbSet<AuditLog> eliminado de ApplicationDbContext en Product
- [ ] DependencyInjection actualizado en ambos dominios
- [ ] Tests actualizados
- [ ] Compilación exitosa en ambos dominios
- [ ] Sin referencias cruzadas entre dominios
