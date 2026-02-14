# Plan de implementación: Company y Logs gestionados desde Admin

> **Canon:** `docs/Feature/company-managed-by-admin/PLAN-COMPANY-MANAGED-BY-ADMIN.md`

**SPEC:** SPEC-COMPANY-MANAGED-BY-ADMIN  
**Clarificaciones:** SPEC-COMPANY-MANAGED-BY-ADMIN_CLARIFICATIONS.md  
**Rama sugerida:** feat/company-managed-by-admin

---

## Resumen

Este plan ordena las tareas para cumplir la SPEC (tablas Company y Logs en Admin, Product consumidor) y las clarificaciones (testeos, validaciones, deuda técnica). Se asume que Admin ya expone CRUD de Company y API de logs (POST/GET/DELETE); el foco está en migraciones de tabla, verificación de responsabilidades, tests y validaciones.

---

## Fase 1: Tabla Logs (Admin)

### 1.1 Migración: crear tabla Logs en Admin (idempotente)
- [ ] Añadir migración en Admin que ejecute **`CREATE TABLE IF NOT EXISTS Logs (...)`** con el esquema completo de la entidad `Log` (Id, Level, Message, Template, Exception, Properties, TimeStamp, Source, CompanyId, UserId, ClientInfo), tipos MySQL alineados con el snapshot/entidad.
- [ ] Mantener la migración existente **AddMissingColumnsToLogs** para bases donde la tabla fue creada por Serilog (no eliminar).
- [ ] Documentar en operaciones (o en README de migraciones) que en **nuevos entornos** las migraciones de Admin deben ejecutarse **antes** del primer arranque de la API Admin.
- **Entregable:** Nueva migración aplicable con `dotnet ef database update --project .../GesFer.Admin.Infra.csproj` (sin --startup-project, usando AdminDbContextFactory).

### 1.2 Serilog (verificación)
- [ ] Comprobar que el sink MySQL no crea la tabla si ya existe (comportamiento por defecto). Opcional: si el paquete lo permite, configurar `autoCreateSqlTable: false` para que la única creación sea vía migración.
- **Entregable:** Sin cambios obligatorios; documentado en SPEC/plan.

---

## Fase 2: Tabla Company (Admin)

### 2.1 Propiedad de la tabla Companies
- [ ] Verificar en el repositorio **quién crea la tabla Companies** (Admin vs Product migraciones). Si la crea Product, documentar el estado y definir opción: (A) migración en Admin `CREATE TABLE IF NOT EXISTS Companies` con esquema completo, o (B) plan de transición documentado para unificar en Admin en una iteración futura.
- [ ] Si se implementa (A): añadir migración en Admin (raw SQL o EF) que cree la tabla Companies si no existe, con esquema alineado a la entidad Company (Shared) y a CompanyConfiguration en Admin.
- **Entregable:** Tabla Companies creada o alterada solo por Admin, o plan de unificación documentado en la feature.

### 2.2 Seeds
- [ ] Confirmar que los seeds de companies se ejecutan **solo desde Admin** (`AdminJsonDataSeeder.SeedCompaniesAsync()`). Si Product aún inserta companies en sus seeds, eliminar o condicionar para no duplicar (Product solo debe referenciar IDs existentes).
- **Entregable:** Seeds de companies únicamente en Admin; Product sin inserción de companies.

### 2.3 Product: solo consumidor
- [ ] Verificar que Product **no** expone `POST /api/companies` ni `DELETE /api/companies`; solo `GET /api/my-company` y `PUT /api/my-company` vía `IAdminApiClient`.
- [ ] Si existieran endpoints o handlers de CRUD de Company en Product (crear/borrar empresas), eliminarlos o marcarlos como obsoletos y redirigir a Admin.
- **Entregable:** Product sin creación/borrado de empresas; solo proxy "mi empresa".

---

## Fase 3: Tests

### 3.1 Admin – Company
- [ ] Tests **unitarios** de handlers: CreateCompany, UpdateCompany, DeleteCompany, GetCompanyById, GetAllCompanies (cuando no estén cubiertos).
- [ ] Tests de **integración** de `CompanyController`: CRUD con auth Admin (JWT); Get/Update con Shared Secret (X-Internal-Secret). Validación de respuestas 200/201/404/401.
- **Entregable:** Cobertura de Company en Admin mantenida o mejorada; tests en verde.

### 3.2 Admin – Logs
- [ ] Tests de **integración** de `LogController`: GET /api/admin/logs (200, contrato: TotalCount, PageNumber, PageSize, TotalPages, Logs); GET sin auth (401); POST con Shared Secret (200); DELETE purga con dateLimit (200 y límite 7 días); DELETE con fecha < 7 días (400).
- [ ] No reducir cobertura existente (LogControllerTests actuales).
- **Entregable:** Tests de LogController verdes; contrato y reglas de negocio cubiertos.

### 3.3 Product – MyCompany y logs
- [ ] Tests de **integración** de `MyCompanyController`: GET /api/my-company y PUT /api/my-company (delegación a Admin API; mock o API real según configuración). Comportamiento ante 401 o Admin API no disponible.
- [ ] Verificar tests de envío de logs a Admin (AsyncLogPublisher / AdminApiLogSink) si existen; añadir si faltan.
- **Entregable:** Tests de MyCompany y flujo de logs Product→Admin en verde.

---

## Fase 4: Validaciones

### 4.1 Company (Admin)
- [ ] Definir y aplicar **validación de entrada** en DTOs y endpoints: nombre obligatorio, TaxId/Email formatos válidos (ValueObjects o DataAnnotations/FluentValidation), direcciones, etc.
- [ ] Documentar reglas de negocio validadas (ej. en clarificaciones o en código).
- **Entregable:** Validaciones explícitas en creación/actualización de Company; reglas documentadas.

### 4.2 Logs (Admin)
- [ ] Validación de parámetros de GET (pageNumber, pageSize, fechas); de DELETE (dateLimit, regla > 7 días); de POST (nivel, mensaje, timestamp).
- [ ] Reglas de negocio (purga solo > 7 días) ya implementadas; cubiertas por tests (Fase 3.2).
- **Entregable:** Parámetros de logs validados; sin regresión en purga.

---

## Fase 5: Deuda técnica (registro y opcional)

### 5.1 No endurecer Shared.Company
- [ ] Durante la implementación, **no** introducir nuevas dependencias de Product o Admin hacia Shared que refuercen el uso de `Shared.Company` como modelo único. Preferir DTOs y contratos de API.
- [ ] Cualquier refactor que mueva Company de Shared a Admin.Domain debe planificarse en una **iteración posterior** (deuda registrada en DEBT-COMPANY-NO-COMPARTIDA.md).

### 5.2 Opcional (iteración futura)
- [ ] Valorar pasos hacia **saldar la deuda**: entidad Company en Admin.Domain; Product sin referencia a Shared.Company, solo DTOs/contratos. No forma parte obligatoria de este plan.
- **Entregable:** Deuda documentada; decisiones de no avanzar en este ciclo anotadas si aplica.

---

## Orden de ejecución sugerido

| Orden | Fase | Dependencias |
|-------|------|--------------|
| 1 | Fase 1 – Tabla Logs | Ninguna |
| 2 | Fase 2 – Tabla Company | Ninguna (paralelizable con 1) |
| 3 | Fase 3 – Tests | Fases 1 y 2 verificadas |
| 4 | Fase 4 – Validaciones | Especificaciones de validación; puede solaparse con 3 |
| 5 | Fase 5 – Deuda técnica | Durante todo el desarrollo |

---

## Criterios de cierre del plan

- [ ] Todas las tareas de Fases 1–4 con checkbox cumplido o justificado (ej. "plan documentado" en 2.1).
- [ ] Build de Admin y Product sin errores.
- [ ] Tests existentes y nuevos en verde (Admin: Company, Logs; Product: MyCompany, logs).
- [ ] No se introducen dependencias Admin → Product ni Product → Admin.
- [ ] Documentación de la feature actualizada (OBJETIVO, SPEC, Clarificaciones, este Plan) y referenciada en Evolution Log al cierre de la feature.

---

## Trazabilidad

- **SPEC:** `docs/Feature/company-managed-by-admin/SPEC-COMPANY-MANAGED-BY-ADMIN.md`
- **Clarificaciones:** `docs/Feature/company-managed-by-admin/SPEC-COMPANY-MANAGED-BY-ADMIN_CLARIFICATIONS.md`
- **Deuda:** `docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md`
- **Acción feature:** `openspecs/actions/feature.md` (Fase 4 – Planificación).
