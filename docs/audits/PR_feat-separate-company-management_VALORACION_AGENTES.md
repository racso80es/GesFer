# Valoración de calidad — PR feat/separate-company-management

**Fecha:** 2026-02-10  
**Rama:** feat/separate-company-management-2466795083738940271  
**Alcance:** Cambios respecto a `master`: separación de gestión de empresas al dominio Admin; Product como consumidor vía API; Admin Front CRUD empresas; Product Front "Mi empresa"; Shared entidad Company.  
**Archivos afectados:** 70 (código Back/Front .cs, .ts, .tsx, docs, openspecs).

---

## 1. Valoración — Agente Arquitecto (System Architect)

**Criterios aplicados:** Product cannot import Admin; Admin cannot import Product; Shared sin importar Product/Admin; Strict Directory Map; Value Objects.

### Resultado: **CON CONDICIONES (violaciones críticas de dependencia)**

| Criterio | Valoración | Comentario |
|----------|------------|------------|
| **Product cannot import Admin** | ❌ Violación | `GesFer.Infrastructure` (Product.Back) tiene `ProjectReference` a `GesFer.Admin.Application`. `AdminApiClient` e `IAdminApiClient` usan `CompanyDto`, `UpdateCompanyDto` de Admin. La regla exige que Product no importe Admin. |
| **Admin cannot import Product** | ❌ Violación | `GesFer.Admin.Api` tiene `ProjectReference` a `GesFer.Infrastructure` (Product.Back). Se genera dependencia bidireccional Admin ↔ Product a nivel infraestructura. |
| **Shared sin importar Product/Admin** | ✅ Conforme | La entidad `Company` en `Shared/Back/Domain/Entities/Company.cs` usa solo `BaseEntity` y Value Objects de Shared (`TaxId`, `Email`); no referencia Product ni Admin. |
| **Strict Directory Map** | ✅ Conforme | Código en `src/Admin/`, `src/Product/`, `src/Shared/`; documentación en `docs/Feature/separate-company-management/`, `docs/audits/`. |
| **Value Objects** | ✅ Conforme | Entidad `Company` en Shared usa `TaxId` y `Email` (VOs). DTOs en Admin usan tipos primitivos (aceptable en capa de aplicación para contrato API). |

**Recomendaciones (Arquitecto):**

1. **Eliminar la referencia Product → Admin:** En Product.Back.Infrastructure, no referenciar `GesFer.Admin.Application`. Definir en Product un DTO local (p. ej. `MyCompanyResponseDto`) o contrato que refleje solo los campos necesarios para "mi empresa", y mapear en `AdminApiClient` desde JSON al DTO de Product (o usar tipos anónimos/JsonElement). La comunicación debe ser solo HTTP + contrato, sin referencia a ensamblado Admin.
2. **Eliminar la referencia Admin → Product:** Revisar por qué `Admin.Api` referencia `Product.Infrastructure`. Si es por BD compartida o migraciones, extraer a un proyecto compartido (p. ej. bajo Shared o un proyecto de persistencia común) para que Admin no dependa de Product.
3. **Invarianza de dominio:** Hasta que se corrijan las referencias cruzadas, el diseño funcional (Admin SSOT, Product consumidor vía HTTP) es correcto; la implementación actual vulnera la regla estricta de fronteras de dominio.

---

## 2. Valoración — Agente Seguridad (Security Engineer)

**Criterios aplicados:** Vision Zero (acciones destructivas con confirmación); validación de inputs; Value Objects; Auth separation (admin_ vs auth_); Zod en frontend.

### Resultado: **APROBADO CON OBSERVACIONES**

| Criterio | Valoración | Comentario |
|----------|------------|------------|
| **Vision Zero / acciones destructivas** | ⚠️ Observación | Existe `DELETE /api/companies/{id}` en Admin Back y en Admin Front (`app/api/companies/[id]/route.ts`). En la UI actual no se encontró botón de borrado en listado ni en edición; si se expone en el futuro, debe usarse patrón de confirmación explícita (p. ej. `<DestructiveActionConfirm>` o modal de confirmación) antes de llamar a DELETE. |
| **Autorización Admin** | ✅ Conforme | `CompanyController` (Admin) protegido con `[AuthorizeSystemOrAdmin]`: acceso por JWT rol Admin o por header `X-Internal-Secret` (SharedSecret). Separación admin vs sistema (token) correcta. |
| **Autorización Product** | ✅ Conforme | `MyCompanyController` usa `[Authorize]` y extrae `CompanyId` del token; solo permite operaciones sobre la empresa del usuario. |
| **SharedSecret** | ✅ Conforme | Secret en configuración (`SharedSecret`); no hardcodeado. Header `X-Internal-Secret` solo para comunicación Product.Back → Admin.Back. |
| **Validación frontend (Zod)** | ✅ Conforme | Admin Front `lib/validations/company.ts` define `createCompanySchema` y `updateCompanySchema` con Zod (nombre, taxId, address, email, etc.). |
| **Value Objects backend** | ✅ Conforme | Entidad `Company` en Shared usa `TaxId` y `Email` (VOs). DTOs de Admin usan strings (aceptable en contrato API). |

**Recomendaciones (Seguridad):**

1. Si se añade botón "Eliminar empresa" en Admin Front, exigir confirmación explícita (modal o componente dedicado) antes de ejecutar DELETE.
2. Mantener el secreto compartido solo en configuración y no exponerlo en frontend ni en logs.

---

## 3. Valoración — Agente Documentación (Knowledge Architect)

**Criterios aplicados:** Jerarquía estricta; SSOT; contenido mínimo de docs/Feature según acción feature; trazabilidad.

### Resultado: **CON CONDICIONES (documentación de feature incompleta)**

| Criterio | Valoración | Comentario |
|----------|------------|------------|
| **Strict Hierarchy** | ✅ Conforme | Documentación en `docs/Feature/separate-company-management/`, `docs/audits/`, `docs/evolution/`. No hay docs técnicos en raíz. |
| **SSOT** | ✅ Conforme | SPEC y PLAN de la feature concentrados en `docs/Feature/separate-company-management/`. |
| **Contenido mínimo docs/Feature** | ⚠️ Incompleto | Según `openspecs/actions/feature.md`, la carpeta de la feature debe contener al menos: **OBJETIVO.md**, SPEC, CLARIFICATIONS, PLAN. En esta feature existen **SPEC-SEPARATE-COMPANY-MANAGEMENT.md** y **PLAN-SEPARATE-COMPANY-MANAGEMENT.md**, pero **falta OBJETIVO.md** y no hay archivo de **CLARIFICATIONS**. |
| **Trazabilidad** | ⚠️ Revisar | Se han eliminado entradas en `docs/EVOLUTION_LOG.md` y en `docs/audits/ACCESS_LOG.md`; se han eliminado documentos de otra feature (`SPEC-ACCESIBILIDAD-ELECTRON`, `PLAN-ACCESIBILIDAD-ELECTRON`, `CLARIFICATIONS`). Confirmar que es intencional y que la trazabilidad de la feature "separate-company-management" queda reflejada en Evolution Log. |

**Recomendaciones (Documentación):**

1. Añadir **OBJETIVO.md** en `docs/Feature/separate-company-management/` con: objetivo, alcance, ley aplicada, resumen de fases, cierre/PR y referencias (alineado con la acción feature).
2. Opcional: añadir **SPEC-SEPARATE-COMPANY-MANAGEMENT_CLARIFICATIONS.md** si hubo decisiones o gaps resueltos durante el diseño.
3. Verificar que los cambios en ACCESS_LOG y en docs eliminados (accesibilidad electron) están documentados o aprobados para no perder trazabilidad.

---

## 4. Resumen ejecutivo

| Agente | Resultado | Riesgo |
|--------|-----------|--------|
| **Arquitecto** | CON CONDICIONES | Crítico: dependencias Product↔Admin |
| **Seguridad** | APROBADO CON OBSERVACIONES | Bajo: confirmación si se expone DELETE en UI |
| **Documentación** | CON CONDICIONES | Medio: falta OBJETIVO.md y opcional CLARIFICATIONS |

**Conclusión:** Los cambios de la rama son funcionalmente coherentes con la especificación (gestión de empresas en Admin, Product como consumidor, Shared Company con VOs), pero **no se recomienda merge a master** hasta:

1. Corregir dependencias entre dominios: Product.Infrastructure no debe referenciar Admin.Application; Admin.Api no debe referenciar Product.Infrastructure (o justificar y documentar excepción con aprobación de arquitectura).
2. Completar documentación de la feature: al menos OBJETIVO.md en `docs/Feature/separate-company-management/`.
3. (Opcional) Si se expone borrado de empresa en la UI Admin: añadir confirmación explícita antes de DELETE.

---

*Generado según definiciones en openspecs/agents/architect.json, security-engineer.json, knowledge-architect.json.*
