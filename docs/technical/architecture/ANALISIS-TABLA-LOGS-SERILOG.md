# Análisis: Creación de la tabla Logs y relación con Serilog

**Rol:** Arquitecto  
**Fecha:** 2026-02-14  
**Objetivo:** Dónde se crea la tabla `Logs`, con qué campos y cómo se relaciona con Serilog.

---

## 1. Dónde se crea la tabla Logs

### 1.1 No hay `CreateTable("Logs")` en migraciones EF del proyecto

- **Admin (InitialAdmin):** La migración `20260213154125_InitialAdmin.cs` crea solo `AdminUsers` y `AuditLogs`. El comentario indica: *"Companies, Logs, etc. ya existen por las migraciones de Product (ApplicationDbContext)."*
- **Product:** No existe ninguna migración en el backend Product que cree la tabla `Logs` (búsqueda en `*Migration*` sin resultados para `CreateTable`/`Logs`).

**Conclusión:** La tabla `Logs` **no se crea por migraciones EF** en el repositorio actual. Queda abierto si en algún momento existió una migración en Product que la creara o si la creación es externa.

### 1.2 Creación real en tiempo de ejecución: Serilog.Sinks.MySQL

La tabla `Logs` puede ser creada en runtime por el sink de Serilog cuando escribe por primera vez:

| Dónde | Archivo | Qué hace |
|-------|---------|----------|
| Admin API | `src/Admin/Back/Api/Program.cs` | `builder.Host.UseSerilog(...)` con `WriteTo.MySQL(connectionString, tableName: "Logs", ...)`. No se pasa `autoCreateSqlTable`; el paquete **Serilog.Sinks.MySQL** puede crear la tabla automáticamente según su implementación (p. ej. si por defecto crea la tabla en el primer write). |

- **Development:** El sink MySQL es opcional (`Serilog:UseMySql` en `appsettings.Development.json`, por defecto `false`).
- **No-Development:** Siempre se configura `WriteTo.MySQL(..., tableName: "Logs", ...)` hacia la misma BD (ScrapDb).

Por tanto, en entornos donde el sink MySQL está activo, la **creación efectiva** de la tabla suele deberse a **Serilog.Sinks.MySQL** (esquema por defecto del paquete), no a EF.

### 1.3 Alineación posterior con el modelo EF: migración Admin

Para que el modelo EF (entidad `Log`) y la tabla física coincidan cuando la tabla fue creada por Serilog (con menos columnas), se añadió la migración:

- **Archivo:** `src/Admin/Back/Infrastructure/Data/Migrations/20260214120000_AddMissingColumnsToLogs.cs`
- **Acción:** Añade a la tabla `Logs` las columnas `Source`, `CompanyId` y `UserId` (nullable), de forma que el esquema quede alineado con la entidad `Log` del dominio Admin.

---

## 2. Campos: modelo EF (entidad Log) vs Serilog

### 2.1 Entidad `Log` (Admin) – SSOT del modelo

**Ubicación:** `src/Admin/Back/domain/Entities/Log.cs`

| Campo | Tipo C# | Uso / Origen |
|-------|---------|----------------|
| `Id` | `int` | PK, AUTO_INCREMENT (requisito Serilog.Sinks.MySQL; la entidad no usa Guid/BaseEntity). |
| `Level` | `string` | Nivel (Debug, Information, Warning, Error, Fatal). Serilog. |
| `Message` | `string` | Mensaje renderizado. Serilog. |
| `Template` | `string?` | Template con placeholders. Serilog. |
| `Exception` | `string?` | Excepción si existe. Serilog. |
| `Properties` | `string?` | Propiedades en JSON. Serilog. |
| `TimeStamp` | `DateTime` | UTC; nombre con mayúscula por Serilog. |
| `Source` | `string?` | Fuente (ej. SourceContext). Extensión GesFer. |
| `CompanyId` | `Guid?` | Tenant. Extensión GesFer. |
| `UserId` | `Guid?` | Usuario. Extensión GesFer. |
| `ClientInfo` | `string?` | Cliente (User-Agent, IP, etc.) en JSON. Extensión GesFer. |

La entidad está documentada como *"diseñada específicamente para ser compatible con Serilog"* y no hereda de `BaseEntity` por el requisito de `Id` INT AUTO_INCREMENT del sink MySQL.

### 2.2 Esquema típico de Serilog.Sinks.MySQL

El paquete suele crear tablas con un subconjunto de columnas estándar, por ejemplo:

- `Id` (INT, AUTO_INCREMENT)
- `Message` / `MessageTemplate`
- `Level`
- `TimeStamp`
- `Exception`
- `Properties`

No incluye por defecto `Source`, `CompanyId`, `UserId` ni `ClientInfo`. Esas columnas son extensiones del modelo GesFer y se añaden con la migración `AddMissingColumnsToLogs` cuando la tabla ya existe (creada por Serilog).

### 2.3 Configuración EF del modelo

**Ubicación:** `src/Admin/Back/Infrastructure/Data/AdminDbContext.cs`

- `DbSet<Log> Logs` y configuración manual para la entidad `Log`:
  - `ToTable("Logs")`
  - `HasKey(e => e.Id)` y `ValueGeneratedOnAdd()`
- No existe `LogConfiguration.cs` en el proyecto Admin; la configuración está en `OnModelCreating`.

La documentación en `docs/technical/architecture/database-schema.md` (sección 7) describe la tabla con los mismos campos que la entidad; la ruta allí a `Api/src/...` es genérica; en el código actual el modelo y la configuración están en **Admin** (domain + Infrastructure).

---

## 3. Relación con Serilog

### 3.1 Escritura (Serilog → tabla Logs)

| Componente | Rol |
|-----------|-----|
| **Admin API** | Configura Serilog con `WriteTo.MySQL(..., tableName: "Logs", ...)` en `Program.cs`. Escribe en la tabla `Logs` de la BD configurada (DefaultConnection). |
| **Product API** | No escribe directamente en MySQL con Serilog.Sinks.MySQL en el código actual; envía logs a la Admin API vía `AdminApiLogSink` → `AsyncLogPublisher` → POST `/api/admin/logs`. |
| **LogController (Admin)** | Expone POST `/api/admin/logs` (AuthorizeSystemOrAdmin); recibe `CreateLogDto` e inserta en `_context.Logs` (entidad `Log`). Así, los logs enviados por Product se persisten en la misma tabla `Logs`. |

Flujo resumido:

1. **Admin:** Serilog (si UseMySql está activo) escribe en `Logs` con el esquema que el sink use (por defecto, menos columnas).
2. **Product:** Serilog → AdminApiLogSink → HTTP POST a Admin → LogController → EF `Logs.Add(...)` (modelo completo, incl. Source, CompanyId, UserId si se envían).

### 3.2 Lectura (EF → API → UI)

- **LogController:** GET `/api/admin/logs` (AdminOnly) usa `_context.Logs` con LINQ, proyección a `LogDto` (Id, Level, Message, Exception, TimeStamp, Source, CompanyId, UserId).
- La tabla debe tener esas columnas para que la consulta EF no falle; de ahí la migración que añade `Source`, `CompanyId` y `UserId` cuando la tabla fue creada solo por Serilog.

### 3.3 Invarianzas

- **Una sola tabla `Logs`:** Tanto el sink de Serilog (Admin) como el LogController (recepciones vía API y consultas) usan la misma tabla en la misma BD.
- **Esquema único:** El esquema objetivo es el de la entidad `Log` (Admin); Serilog solo aporta el subconjunto estándar; las columnas extra se aseguran con la migración de Admin.
- **Dominio:** La entidad `Log` y la tabla `Logs` son responsabilidad del **dominio Admin** (lectura/escritura, purga, API); Product solo envía eventos a la API Admin.

---

## 4. Resumen y recomendaciones

| Pregunta | Respuesta |
|----------|-----------|
| **¿Dónde se crea la tabla Logs?** | En la práctica, por **Serilog.Sinks.MySQL** en el primer write (Admin API), no por migraciones EF en el repo. La migración InitialAdmin de Admin no crea `Logs`. |
| **¿Con qué campos?** | Según el **modelo EF** (entidad `Log`): Id, Level, Message, Template, Exception, Properties, TimeStamp, Source, CompanyId, UserId, ClientInfo. El sink Serilog suele crear solo un subconjunto; la migración `AddMissingColumnsToLogs` añade Source, CompanyId, UserId. |
| **Relación con Serilog** | Serilog (sink MySQL en Admin) **escribe** en `Logs`; el **LogController** también escribe (logs recibidos por API) y **lee** (GET con filtros/paginación). Misma tabla, mismo esquema objetivo definido por la entidad `Log`. |

**Recomendaciones:**

1. **SSOT:** Mantener `src/Admin/Back/domain/Entities/Log.cs` y `docs/technical/architecture/database-schema.md` (sección 7) alineados; la ruta en la doc puede concretarse a Admin (domain + Infrastructure).
2. **Despliegue:** Aplicar la migración `AddMissingColumnsToLogs` en toda BD donde la tabla `Logs` exista con el esquema mínimo de Serilog (para evitar errores de lectura EF).
3. **Opcional:** Si se desea que la tabla sea 100% responsabilidad de EF, valorar una migración que cree `Logs` con el esquema completo y desactivar la auto-creación del sink (si el paquete lo permite), documentando el orden de ejecución (migración antes de arrancar la API).

---

## 5. Valoración: crear la tabla Logs en Admin (como AdminUsers y AuditLogs)

### 5.1 ¿Es más eficiente?

**Sí.** Crear la tabla `Logs` en Admin con una migración EF (igual que `AdminUsers` y `AuditLogs`) es más eficiente y coherente con la arquitectura:

| Aspecto | Situación actual | Con tabla creada en Admin |
|--------|-------------------|---------------------------|
| **Responsabilidad** | Serilog crea la tabla (esquema mínimo); Admin añade columnas después. | Admin es dueño del esquema desde el inicio (una migración, esquema completo). |
| **Consistencia** | Dos orígenes (sink + migración AddMissingColumnsToLogs). | Un solo origen: migraciones Admin (AdminUsers, AuditLogs, **Logs**). |
| **Nuevos entornos** | Depende del primer write de Serilog o de aplicar dos migraciones (y orden correcto). | `dotnet ef database update` crea todas las tablas Admin de una vez; Serilog solo escribe en una tabla ya existente. |
| **Compatibilidad Serilog** | El sink puede crear tabla con sus columnas; luego hay que alinear. | La tabla ya existe con el esquema de la entidad `Log`; el sink solo hace INSERT (columnas extra en NULL si no las usa). |

La entidad `Log` ya está pensada para Serilog (Id INT, TimeStamp, etc.). El sink escribe en las columnas que conoce; las demás (Source, CompanyId, UserId, ClientInfo) pueden quedar en NULL. No hay conflicto.

### 5.2 Opciones de implementación

**Opción A – Migración que crea la tabla (recomendada para nuevos entornos)**  
- Añadir una migración en Admin que ejecute **`CREATE TABLE IF NOT EXISTS Logs (...)`** en SQL bruto con el esquema completo (todos los campos de la entidad `Log`).  
- **Ventaja:** Idempotente. Si la tabla no existe (nuevo entorno), se crea con todas las columnas. Si ya existe (p. ej. por Serilog), no se hace nada.  
- **Compatibilidad:** Bases ya existentes con `Logs` creada por Serilog siguen necesitando `AddMissingColumnsToLogs` (o un script equivalente) para añadir Source, CompanyId, UserId. Esa migración debe seguir siendo idempotente (solo añadir columnas si no existen) o aplicarse solo donde corresponda.

**Opción B – Incluir Logs en InitialAdmin (solo para nuevos despliegues)**  
- No modificar la migración histórica `InitialAdmin` (ya aplicada en muchos entornos).  
- Para nuevos proyectos o un “squash” futuro, una única migración inicial de Admin podría crear AdminUsers, AuditLogs y Logs.  
- En el estado actual del repo, la vía práctica es **Opción A** (nueva migración con `CREATE TABLE IF NOT EXISTS`).

**Opción C – Migración EF `CreateTable("Logs", ...)` sin IF NOT EXISTS**  
- Una migración que use `migrationBuilder.CreateTable("Logs", ...)` con el esquema completo.  
- **Problema:** En bases donde `Logs` ya existe (creada por Serilog), la migración fallaría. Solo sería válida para instalaciones nuevas donde nunca se haya creado `Logs` antes.  
- Menos flexible que Opción A.

### 5.3 Recomendación final

- **Sí, es más eficiente y deseable** que la tabla `Logs` se cree en Admin, igual que AdminUsers y AuditLogs.  
- **Implementación sugerida:**  
  1. Añadir una **nueva migración** en Admin que ejecute `CREATE TABLE IF NOT EXISTS Logs (...)` con el esquema completo de la entidad `Log` (tipos MySQL alineados con el snapshot/entidad).  
  2. **Mantener** `AddMissingColumnsToLogs` para bases ya existentes donde la tabla fue creada por Serilog (hasta que todas estén alineadas o se unifique con un script de actualización).  
  3. **Serilog:** No es necesario desactivar la auto-creación del sink si el paquete la tiene; si la tabla ya existe, el sink hará INSERT. Si se puede configurar `autoCreateSqlTable: false`, mejor: así la única fuente de creación es la migración de Admin.  
  4. **Documentar** en operaciones que, en nuevos entornos, las migraciones de Admin (incluida la que crea `Logs`) deben ejecutarse antes del primer arranque de la API Admin.

Con esto, Admin concentra la definición y creación de sus tablas (AdminUsers, AuditLogs, Logs) y Serilog queda como **consumidor** de una tabla ya definida por el dominio Admin.
