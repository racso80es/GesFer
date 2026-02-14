# Implementación: Análisis y propuesta (Company y Logs desde Admin)

**Plan:** PLAN-COMPANY-MANAGED-BY-ADMIN  
**Fecha:** 2026-02-14

---

## 1. Análisis del estado actual

### 1.1 Tabla Logs
| Aspecto | Estado |
|--------|--------|
| **Quién la crea** | En la práctica, **Serilog.Sinks.MySQL** (en runtime) o no existe en BD nueva. No hay `CreateTable("Logs")` en migraciones EF. |
| **Migraciones Admin** | Existe **AddMissingColumnsToLogs** (20260214120000): añade Source, CompanyId, UserId. Asume que la tabla Logs ya existe. |
| **Entidad** | `Log` en Admin (domain/Entities/Log.cs); AdminDbContext tiene DbSet\<Log\>. |
| **API** | LogController: POST (recepción), GET (paginado), DELETE (purga). Implementado. |
| **Design-time** | AdminDbContextFactory permite `dotnet ef database update` sin arrancar la API. |

**Gap:** En un entorno nuevo sin Serilog previo, la tabla Logs no existe y AddMissingColumnsToLogs fallaría. Falta una migración que cree la tabla con esquema completo de forma idempotente.

### 1.2 Tabla Companies
| Aspecto | Estado |
|--------|--------|
| **Quién la crea** | **Product.InitialCreate** (20260213141112_InitialCreate.cs) crea la tabla `Companies`. Admin.InitialAdmin **no** crea Companies. |
| **Seeds** | **Admin:** AdminJsonDataSeeder.SeedCompaniesAsync() desde companies.json. **Product:** JsonDataSeeder.SeedCompaniesAsync() desde demo-data/test-data; puede insertar companies si vienen en el JSON. |
| **API Product** | Solo **MyCompanyController** (GET/PUT proxy a Admin). No hay CompanyController ni POST/DELETE /api/companies en Product.Api. |
| **Handlers Product** | Existen CreateCompanyCommandHandler, DeleteCompanyCommandHandler, etc., pero **no están expuestos** por ningún controller en Product.Api; se usan en tests o código interno. |

**Gap:** La tabla Companies sigue siendo creada por Product. Seeds de companies en Product siguen activos (pueden duplicar o depender de orden de ejecución). Propuesta: documentar estado (opción B del plan) y condicionar/omitir seed de companies en Product cuando se use BD compartida (Admin ya sembró).

### 1.3 Tests
| Área | Estado |
|------|--------|
| **Admin Logs** | LogControllerTests: ReceiveLog, GetLogs (contrato), GetLogs 401, GetLogs vacío, PurgeLogs. Cobertura aceptable. |
| **Admin Company** | CompanyControllerTests existen; revisar cobertura CRUD + Shared Secret. |
| **Product MyCompany** | MyCompanyControllerTests (si existen) o mock Admin API. |

### 1.4 Validaciones
- LogController: pageNumber, pageSize, dateLimit (purga > 7 días) ya validados en código.
- Company: revisar DTOs (DataAnnotations/ValueObjects) en Admin.

---

## 2. Propuesta de implementación

### 2.1 Orden de trabajo
1. **Fase 1 – Logs:** Migración idempotente que cree la tabla Logs si no existe; hacer AddMissingColumnsToLogs idempotente (solo añadir columnas si no existen); documentar orden de migraciones.
2. **Fase 2 – Company:** Documentar propiedad (Companies creada por Product; Admin es dueño lógico). Condicionar seed de companies en Product (no insertar si BD compartida y Admin ya sembró, o documentar orden: Admin primero). Verificar que no se exponen POST/DELETE companies en Product.
3. **Fase 3 – Tests:** Revisar y completar tests Admin Company/Logs y Product MyCompany según plan.
4. **Fase 4 – Validaciones:** Revisar validaciones en DTOs y endpoints; documentar.

### 2.2 Decisiones
- **Companies:** No añadir migración CreateTable Companies en Admin en este ciclo (la tabla ya la crea Product y hay muchas FKs). Se documenta el estado y el objetivo a medio plazo (Admin dueño lógico; semilla desde Admin; Product solo consumidor).
- **Logs:** Sí implementar migración **CreateLogsTableIfNotExists** con esquema completo. Colocar **antes** de AddMissingColumnsToLogs en el orden de migraciones (timestamp anterior a 20260214120000) para que en BD nueva se cree la tabla primero. Hacer **AddMissingColumnsToLogs** idempotente (raw SQL que añade cada columna solo si no existe) para no fallar cuando la tabla ya tiene todas las columnas.

### 2.3 Archivos a tocar (Fase 1)
- **Nuevo:** `Admin/Back/Infrastructure/Data/Migrations/20260214110000_CreateLogsTableIfNotExists.cs` (raw SQL `CREATE TABLE IF NOT EXISTS Logs (...)`).
- **Modificar:** `Admin/Back/Infrastructure/Data/Migrations/20260214120000_AddMissingColumnsToLogs.cs` → convertir Up() en idempotente (solo ADD COLUMN si la columna no existe, vía SQL condicional).
- **Nuevo o modificar:** README en `Admin/Back/Infrastructure/Data/Migrations/` o en `docs/` indicando que en nuevos entornos las migraciones de Admin deben ejecutarse antes del primer arranque de la API.

---

## 3. Implementación Fase 1 (resumen)

- [x] Crear migración **20260214110000_CreateLogsTableIfNotExists** con `migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS ...")` (esquema Log completo en MySQL).
- [x] Hacer **AddMissingColumnsToLogs** idempotente: en Up(), usar procedimiento almacenado que añade Source, CompanyId, UserId solo si no existen (consultando information_schema).
- [x] Añadir **README** en `Infrastructure/Data/Migrations/README.md`: orden de ejecución, comando sin --startup-project, tablas creadas por Admin.

**Nota:** Si al ejecutar la migración AddMissingColumnsToLogs falla por el procedimiento (p. ej. el conector divide por `;`), valorar revertir a AddColumn estándar y documentar que en BD nueva CreateLogsTableIfNotExists crea la tabla completa y AddMissingColumnsToLogs puede marcarse como aplicada manualmente si da error de columna duplicada.

---

## 4. Fase 2 documentada (Companies)

- **Tabla Companies:** Creada por **Product.InitialCreate** (20260213141112). Admin no crea la tabla; es **dueño lógico** (seeds en Admin, CRUD en Admin, Product solo consumidor). No se añade migración CreateTable Companies en Admin en este ciclo (muchas FKs en Product referencian Companies).
- **Seeds:** Admin ejecuta SeedCompaniesAsync desde companies.json. Product puede seguir sembrando companies desde JSON en entornos de prueba; en BD compartida se recomienda ejecutar seeds de Admin primero (ver Seeds/README.md).
- **Product API:** No expone POST/DELETE companies; solo MyCompanyController (GET/PUT proxy a Admin). Cumple SPEC.

---

## 5. Trazabilidad
- Plan: `docs/Feature/company-managed-by-admin/PLAN-COMPANY-MANAGED-BY-ADMIN.md`
- SPEC: `docs/Feature/company-managed-by-admin/SPEC-COMPANY-MANAGED-BY-ADMIN.md`
