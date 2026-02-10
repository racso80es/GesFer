# Valoración de calidad (Auditoría 3) — PR feat/separate-company-management

**Fecha:** 2026-02-10  
**Rama:** feat/separate-company-management-2466795083738940271  
**Alcance de esta auditoría:** Revisión de los tres agentes (Arquitecto, Seguridad, Documentación) + **Cobertura de tests sobre el nuevo código** + **Validación de seeds de empresas (ajuste a Admin)**.  
**Referencias:** V1 `PR_feat-separate-company-management_VALORACION_AGENTES.md`, V2 `PR_feat-separate-company-management_VALORACION_AGENTES_V2.md`.

**Actualización (post-correcciones):** Se ha comprobado que la deuda técnica Admin→Product ha sido **eliminada**. Admin.Api ya no referencia Product.Infrastructure; el Dashboard de Admin obtiene métricas de Product vía **HTTP** (`IProductApiClient` / `ProductApiClient` → `GET api/dashboard/stats` en Product API). Sección 1 y resumen actualizados en consecuencia.

---

## 1. Valoración — Agente Arquitecto

**Resultado:** **APROBADO** (deuda técnica Admin→Product resuelta).

- **Product no importa Admin:** DTOs locales (`AdminCompanyDto`, `AdminUpdateCompanyDto`) en Product.Infrastructure; comunicación con Admin solo por HTTP.
- **Admin no importa Product:** Se ha eliminado la referencia de proyecto `GesFer.Infrastructure` (Product) desde `GesFer.Admin.Api`. El `DashboardController` de Admin usa:
  - `AdminDbContext` (propio de Admin) para métricas de Companies.
  - `IProductApiClient` / `ProductApiClient` (en Admin.Infrastructure) para obtener métricas remotas (Users, Articles, etc.) vía **HTTP** contra la Product API (`GET api/dashboard/stats`). No hay dependencia en tiempo de compilación hacia Product.
- **Admin.Infra** solo referencia Admin.Domain y Shared.Back.Domain; no referencia Product.
- Shared, mapa de directorios y Value Objects conformes.

---

## 2. Valoración — Agente Seguridad

**Resultado:** **APROBADO** (sin cambios respecto a V2).

- Autorización Admin/Product y SharedSecret conformes.
- Zod en formularios; si se expone DELETE en UI, usar confirmación explícita.

---

## 3. Valoración — Agente Documentación

**Resultado:** **APROBADO** (sin cambios respecto a V2).

- `docs/Feature/separate-company-management/` contiene OBJECTIVE.md, SPEC, CLARIFICATIONS y PLAN. Cumple acción feature.

---

## 4. Cobertura de tests sobre el nuevo código

**Criterio:** Existencia de tests (unitarios o integración) que cubran el código añadido o modificado por la feature: Admin Company (CRUD), Product MyCompany (proxy), AdminApiClient, frontends Admin/Product companies.

### Resultado: **INSUFICIENTE**

| Componente | Tipo de test esperado | Estado | Comentario |
|------------|------------------------|--------|------------|
| **Admin: CompanyController** | Integración (Admin API) | ❌ No existe | No hay tests en `Admin.Back.IntegrationTests` que llamen a `/api/company` (GET/POST/PUT/DELETE). |
| **Admin: Handlers Company** (Create, Get, Update, Delete, GetAll) | Unitarios | ❌ No existe | En `Admin.UnitTests` solo hay tests de Auth y AuditLog; no hay tests para los handlers de Company. |
| **Admin: AuthorizeSystemOrAdminAttribute** | Unitario / integración | ❌ No existe | No se ha verificado el comportamiento con SharedSecret vs JWT Admin. |
| **Product: MyCompanyController** | Integración (Product API) | ❌ No existe | No hay tests que llamen a `/api/my-company` (GET/PUT) con usuario autenticado. |
| **Product: AdminApiClient** | Unitario (mock HttpClient) o integración | ❌ No existe | No hay tests que verifiquen la llamada a Admin API y el mapeo a AdminCompanyDto. |
| **Product: CompanyControllerTests** (existentes) | Integración Product API | ⚠️ Obsoletos | Los tests en `Product.Back.IntegrationTests/Controllers/CompanyControllerTests.cs` llaman a `/api/company` sobre la **Product API** (`GesFer.Api.Program`). En la feature, el `CompanyController` fue **eliminado** de Product y movido a Admin. Esos tests ejercitan endpoints que **ya no existen** en Product (devolverían 404). Deben migrarse a Admin.IntegrationTests o eliminarse/adaptarse. |
| **Admin Front / Product Front companies** | E2E o component | No exigido en esta auditoría | — |

**Recomendaciones (Tests):**

1. **Admin:** Añadir tests de integración para `CompanyController` (lista, get by id, create, update, delete) usando `AdminWebAppFactory` y, si aplica, SharedSecret o JWT de test. Añadir tests unitarios para al menos un handler representativo (p. ej. `CreateCompanyHandler`, `GetCompanyByIdHandler`).
2. **Product:** Añadir tests de integración para `MyCompanyController` (GET/PUT con token que incluya company_id). Añadir tests unitarios o de integración para `AdminApiClient` (mock de `HttpClient` o servidor de test que simule Admin API).
3. **Product.IntegrationTests:** Decidir el destino de `CompanyControllerTests`: (a) moverlos a Admin.IntegrationTests y adaptar a la API de Admin (`/api/company`, autenticación Admin/SharedSecret), o (b) eliminarlos si la cobertura se cubre con nuevos tests en Admin.

---

## 5. Validación de seeds de empresas (ajuste a Admin)

**Criterio:** Las seeds de empresas (antes en Product) deben estar ajustadas a Admin debidamente: ubicación, responsable de la carga y consistencia con el modelo de Admin como SSOT de Company.

### Resultado: **PARCIALMENTE AJUSTADO — REQUIERE DECISIÓN O DOCUMENTACIÓN**

| Aspecto | Estado | Comentario |
|---------|--------|------------|
| **Ubicación física de los datos de companies** | En Product | Los JSON con empresas siguen en **Product**: `src/Product/Back/Infrastructure/Data/Seeds/demo-data.json`, `test-data.json`, `master-data.json` (sección `"companies": [...]`). |
| **Responsable de la carga (runtime)** | Product | Solo **Product** ejecuta el seed de companies: `JsonDataSeeder.SeedCompaniesAsync()` en Product.Infrastructure. Usa `_context.Companies` del **ApplicationDbContext** (Product) e instancia `GesFer.Product.Back.Domain.Entities.Company`. Admin **no** tiene ningún seed de companies: `AdminJsonDataSeeder` solo carga `admin-users.json`; no existe `companies.json` ni método equivalente en Admin. |
| **Modelo y tabla** | Compartido | Admin tiene `AdminDbContext.Companies` (entidad `Shared.Back.Domain.Entities.Company`) mapeada a tabla `"Companies"`. Product tiene `ApplicationDbContext.Companies` (entidad que hereda de Shared.Company). Si ambos contextos apuntan a la **misma base de datos**, la tabla `Companies` es única; los datos insertados por Product al ejecutar su seed son visibles para Admin. |
| **Alineación con SSOT** | Ambiguo | La SPEC establece Admin como SSOT para Company. Que el **seed inicial** lo ejecute Product es aceptable si: (1) se documenta explícitamente (p. ej. "En entornos con BD compartida, el seed de Product corre primero y pobla Companies; Admin consume esa tabla"), o (2) se traslada o duplica el seed de companies a Admin (p. ej. `Admin/Infrastructure/Data/Seeds/companies.json` y un método en `AdminJsonDataSeeder`) para que Admin sea también responsable de la carga inicial. |

**Recomendaciones (Seeds):**

1. **Documentar** en `docs/Feature/separate-company-management/` o en README de seeds (Admin y/o Product) que, en la arquitectura actual, el seed de **companies** lo ejecuta **Product** (BD compartida) y Admin solo consume la tabla. Así queda explícito que no es un error sino una decisión de despliegue.
2. **O bien** (opción más alineada con SSOT): Añadir en Admin un seed de companies (archivo `companies.json` en `Admin/Back/Infrastructure/Data/Seeds/` y lógica en `AdminJsonDataSeeder`) con el mismo formato o un subconjunto necesario para Admin, y documentar el orden de ejecución (Admin seed de companies antes o después de Product según estrategia de BD compartida o separada).
3. **Validación de formato:** Los JSON de Product usan `id`, `name`, `taxId`, `address`, `phone`, `email`, `languageId`. Admin usa `Shared.Back.Domain.Entities.Company` con Value Objects `TaxId` y `Email`. El seed actual en Product ya valida TaxId y Email (Value Objects) antes de insertar; ese mismo criterio debería aplicarse si se añade seed de companies en Admin (usar conversiones/validaciones equivalentes a `CompanyConfiguration`).

---

## 6. Resumen ejecutivo (Auditoría 3)

| Dimensión | Resultado | Riesgo |
|-----------|-----------|--------|
| **Arquitecto** | APROBADO | Ninguno (deuda Admin→Product resuelta) |
| **Seguridad** | APROBADO | Bajo |
| **Documentación** | APROBADO | Ninguno |
| **Cobertura de tests (nuevo código)** | INSUFICIENTE | Alto: sin tests para Admin Company, MyCompany, AdminApiClient; CompanyControllerTests de Product obsoletos |
| **Seeds de empresas** | PARCIAL — requiere decisión/documentación | Medio: seeds solo en Product; documentar o mover a Admin |

**Conclusión:** La rama cumple **Arquitecto, Seguridad y Documentación** sin condiciones (fronteras de dominio respetadas; deuda técnica Admin→Product eliminada). Se recomienda **no considerar cerrada la feature** hasta:

1. Añadir cobertura de tests sobre el nuevo código (al menos integración Admin Company y/o Product MyCompany + AdminApiClient) y resolver la obsolescencia de `CompanyControllerTests` en Product.
2. Dejar documentada la estrategia de seeds de companies (Product como ejecutor en BD compartida) o implementar seed de companies en Admin y documentar el orden de carga.

---

*Auditoría 3 según openspecs/agents/architect.json, security-engineer.json, knowledge-architect.json y criterios de cobertura de tests y seeds.*
