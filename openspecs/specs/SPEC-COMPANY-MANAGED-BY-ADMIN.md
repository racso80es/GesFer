# SPEC-COMPANY-MANAGED-BY-ADMIN: Tablas Company y Logs gestionadas desde Admin

> **Canon:** `docs/Feature/company-managed-by-admin/SPEC-COMPANY-MANAGED-BY-ADMIN.md`

## 1. Información general

| Campo | Detalle |
| :--- | :--- |
| **ID** | SPEC-COMPANY-MANAGED-BY-ADMIN |
| **Rama sugerida** | feat/company-managed-by-admin |
| **Estado** | En implementación (Fase 3–4 completadas) |
| **Alcance** | Company (tabla y CRUD desde Admin) + Logs (tabla en Admin, Serilog, API) |

## 2. Objetivo y contexto

### 2.1 Objetivo
Establecer que las tablas **Companies** y **Logs** sean gestionadas íntegramente desde el dominio **Admin**: propiedad del esquema, migraciones, seeds (Company) y operaciones de lectura/escritura. Product no es dueño de ninguna de las dos tablas; para Company consume vía API de Admin; para Logs envía eventos a la API de Admin y no define ni crea la tabla.

### 2.2 Alcance
- **Incluido:** Company: responsabilidades Admin/Product, migraciones, seeds, API, UI. Logs: responsabilidades Admin/Product, creación de tabla en Admin (como AdminUsers/AuditLogs), relación con Serilog, migraciones existentes y futuras, API (GET/POST/purga).
- **Fuera de alcance:** Detalle de cada pantalla de UI; migración de datos entre bases distintas.

---

## PARTE A: TABLA COMPANY

## 3. Responsabilidades por dominio (Company)

### 3.1 Admin (dueño de la tabla y del CRUD)

| Aspecto | Responsabilidad |
|--------|------------------|
| **Entidad / modelo** | Usa la entidad `Company` de **Shared** (`GesFer.Shared.Back.Domain.Entities.Company`) en su contexto. No duplica la entidad. |
| **Tabla** | La tabla `Companies` es **propiedad lógica** de Admin. La creación del esquema (CreateTable) debe realizarse en migraciones de **Admin** (igual que AdminUsers y AuditLogs). Si hoy la tabla se crea en migraciones de Product por razones históricas, el objetivo es migrar esa responsabilidad a Admin. |
| **Migraciones** | Admin aplica migraciones que definen o alteran `Companies`. Product no añade migraciones que creen o modifiquen la tabla `Companies`. |
| **Seeds** | Los datos iniciales de empresas se cargan desde **Admin** (ej. `AdminJsonDataSeeder.SeedCompaniesAsync()`, seeds en `Admin/Back/Infrastructure/Data/Seeds/`). Product no inserta companies en seeds; puede referenciar IDs ya existentes. |
| **API** | CRUD completo expuesto en Admin: `GET/POST/PUT/DELETE /api/companies` (o ruta equivalente), protegido por política Admin y/o Shared Secret para llamadas desde Product. |
| **UI** | Pantallas de gestión de empresas (listado, alta, edición, baja) en el front de Admin. |

### 3.2 Product (consumidor, Company)

| Aspecto | Responsabilidad |
|--------|------------------|
| **Entidad** | Puede usar la entidad que extiende Shared (`GesFer.Product.Back.Domain.Entities.Company`) **solo para lectura y contexto** (ej. filtros por CompanyId). No debe exponer endpoints que creen, borren o alteren la fila de Company. |
| **Tabla** | No es dueño de la definición de `Companies`. No debe contener migraciones que creen o modifiquen la tabla `Companies`. |
| **API** | Solo "mi empresa": `GET /api/my-company`, `PUT /api/my-company`, delegando en `IAdminApiClient`. No expone `POST /api/companies` ni `DELETE /api/companies`. |
| **UI** | Solo "Mi organización" / "Mi empresa" para editar datos propios (proxy a Admin). |

### 3.3 Modelo de datos (Company)
- **Tabla:** `Companies`. **Dueño lógico:** Admin.
- **Campos:** Según la entidad `Company` en Shared (Id, Name, TaxId, Address, Phone, Email, PostalCodeId, CityId, StateId, CountryId, LanguageId, CreatedAt, UpdatedAt, DeletedAt, IsActive, etc.). Fuente de verdad: Shared + configuración en Admin (CompanyConfiguration).
- **Nomenclatura:** En código se usa **Company**; en UI "Empresa" / "Organización" según i18n.

---

## PARTE B: TABLA LOGS

## 4. Objetivo Logs
La tabla **Logs** debe ser propiedad de Admin (igual que AdminUsers y AuditLogs): creada por migraciones de Admin, con esquema completo definido por la entidad `Log`. Serilog y la API de Admin escriben en la misma tabla; Product envía logs a Admin por HTTP y no crea ni altera la tabla.

## 5. Responsabilidades por dominio (Logs)

### 5.1 Admin (dueño de la tabla y de la API de logs)

| Aspecto | Responsabilidad |
|--------|------------------|
| **Entidad / modelo** | Entidad `Log` en **Admin** (`src/Admin/Back/domain/Entities/Log.cs`). No hereda de BaseEntity (Id INT AUTO_INCREMENT por compatibilidad con Serilog.Sinks.MySQL). |
| **Tabla** | La tabla `Logs` es **propiedad lógica** de Admin. Debe crearse en migraciones de **Admin** (igual que AdminUsers y AuditLogs). Opción recomendada: migración con `CREATE TABLE IF NOT EXISTS Logs (...)` con esquema completo. |
| **Migraciones** | Admin aplica migraciones que crean o alteran `Logs`. Existente: `AddMissingColumnsToLogs` (añade Source, CompanyId, UserId cuando la tabla fue creada por Serilog). Objetivo: migración que cree la tabla con esquema completo para nuevos entornos. |
| **API** | `POST /api/admin/logs` (AuthorizeSystemOrAdmin): recibe logs de Product u otros sistemas; persiste en `_context.Logs`. `GET /api/admin/logs` (AdminOnly): consulta paginada con filtros (fecha, nivel, companyId, userId). `DELETE /api/admin/logs?dateLimit=...`: purga de logs antiguos (límite 7 días). |
| **UI** | Pantalla de Logs en el front de Admin (listado, filtros, detalle). |
| **Serilog** | Admin configura `WriteTo.MySQL(..., tableName: "Logs", ...)` en Program.cs. El sink **escribe** en la tabla; no debe ser el origen de creación de la tabla. Opcional: `autoCreateSqlTable: false` si el paquete lo permite. |

### 5.2 Product (emisor de logs, no dueño de la tabla)

| Aspecto | Responsabilidad |
|--------|------------------|
| **Tabla** | No crea ni altera la tabla `Logs`. No tiene migraciones que definan Logs. |
| **Escritura** | Envía logs a Admin vía `AdminApiLogSink` → `AsyncLogPublisher` → POST `/api/admin/logs` (con Shared Secret). No escribe directamente en MySQL con Serilog.Sinks.MySQL hacia la BD compartida. |
| **Lectura** | No expone endpoints de consulta de logs; la consulta es exclusiva de Admin (GET /api/admin/logs). |

### 5.3 Modelo de datos (Logs)
- **Tabla:** `Logs`. **Dueño lógico:** Admin.
- **Campos (entidad Log, Admin):** Id (int, AUTO_INCREMENT), Level, Message, Template, Exception, Properties, TimeStamp, Source, CompanyId, UserId, ClientInfo (todos según `src/Admin/Back/domain/Entities/Log.cs`).
- **Relación con Serilog:** El sink MySQL escribe en las columnas estándar (Id, Level, Message, Template, TimeStamp, Exception, Properties); las columnas Source, CompanyId, UserId, ClientInfo son extensiones GesFer y pueden quedar en NULL para eventos que solo trae Serilog. Una sola tabla; esquema único definido por la entidad Log.

### 5.4 Implementación recomendada (Logs)
1. **Nueva migración en Admin:** `CREATE TABLE IF NOT EXISTS Logs (...)` con el esquema completo (idempotente). Así en nuevos entornos la tabla existe antes del primer write de Serilog.
2. **Mantener** `AddMissingColumnsToLogs` para bases donde la tabla fue creada por Serilog (añade Source, CompanyId, UserId).
3. **Serilog:** Si la tabla ya existe, el sink solo hace INSERT. Documentar que las migraciones de Admin se ejecutan antes del primer arranque de la API Admin en nuevos entornos.
4. **Design-time:** `IDesignTimeDbContextFactory<AdminDbContext>` ya permite ejecutar `dotnet ef database update` sin arrancar la API (evitar fallos por Serilog/JWT).

---

## 6. Seguridad y autorización

- **Admin API (CRUD companies):** Solo rol Admin (JWT) o Shared Secret (Product backend).
- **Admin API (logs):** POST logs: AuthorizeSystemOrAdmin (Shared Secret o Admin). GET/DELETE logs: AdminOnly.
- **Product API (my-company):** Usuario autenticado; CompanyId del token; modificaciones delegadas a Admin.

## 7. Criterios de aceptación

### Company
- [ ] La tabla `Companies` está creada o alterada únicamente por migraciones del contexto **Admin** (o existe un plan documentado para unificar en Admin).
- [ ] Los seeds de companies se ejecutan desde **Admin** (AdminJsonDataSeeder o equivalente).
- [ ] **Admin** expone CRUD completo de Company y la UI de Admin permite gestionar empresas.
- [ ] **Product** no expone creación/borrado de empresas; solo "mi empresa" vía proxy a Admin.

### Logs
- [ ] La tabla `Logs` está creada o alterada únicamente por migraciones del contexto **Admin** (CreateTable o CREATE TABLE IF NOT EXISTS con esquema completo; AddMissingColumnsToLogs para bases existentes con tabla Serilog).
- [ ] **Admin** expone POST /api/admin/logs (recepción), GET /api/admin/logs (consulta paginada), DELETE /api/admin/logs (purga con dateLimit).
- [ ] Serilog en Admin escribe en la tabla `Logs`; la tabla existe antes del primer write (migraciones aplicadas antes del arranque) o se mantiene AddMissingColumnsToLogs donde la tabla fue creada por Serilog.
- [ ] **Product** envía logs a Admin vía POST (AsyncLogPublisher / AdminApiLogSink); no crea ni modifica la tabla Logs.
- [ ] UI de Admin permite consultar y filtrar logs.

### General
- [ ] Compilación y tests existentes pasan; no se introducen dependencias Admin → Product ni Product → Admin.

## 8. Trazabilidad

- **Feature previa:** `docs/Feature/separate-company-management/` (Admin SSOT para Company, Product consumidor).
- **Clarificaciones:** `docs/Feature/company-managed-by-admin/SPEC-COMPANY-MANAGED-BY-ADMIN_CLARIFICATIONS.md` (deuda técnica Company no compartida, testeos, validaciones).
- **Plan:** `docs/Feature/company-managed-by-admin/PLAN-COMPANY-MANAGED-BY-ADMIN.md` (fases Logs, Company, tests, validaciones, deuda).
- **Deuda técnica:** `docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md` (Company no ha de estar en Shared; Admin y Product con dominios propios).
- **Análisis Logs:** `docs/technical/architecture/ANALISIS-TABLA-LOGS-SERILOG.md` (creación tabla, campos, relación Serilog, valoración crear tabla en Admin).
- **Acción:** `openspecs/actions/feature.md`.
