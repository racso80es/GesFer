# SPEC: CRUD Familia de Artículo (ArticleFamily) — PROPUESTA

**Estado:** Borrador para revisión  
**Origen:** [objetive.md](.docs/feature/article-family/objetive.md)  
**Rama:** feat-spec-article-family-13284649229191957205

---

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Especificación** | SPEC-GF-2026-ARTICLE-FAMILY (sugerido) |
| **Rama Relacionada** | feat-spec-article-family-* |
| **Estado** | Draft (propuesta) |
| **Tipo** | Maestro CRUD (Master Data) |
| **Contexto** | Product (multitenancy por CompanyId) |

---

## 2. Propósito y Contexto

### 2.1. Objetivo (Goal)
Permitir el mantenimiento del maestro **Familia de Artículo** (ArticleFamily): crear, listar, editar y eliminar familias de artículos con aislamiento por compañía. Cada familia se asocia a un **Tipo de Tasa** (TaxType) existente, lo que determina el tratamiento fiscal por defecto de los artículos de esa familia.

### 2.2. Alcance (Scope)
*   **Incluido:**
    *   Entidad `ArticleFamily`, persistencia, API REST (CQRS), tests backend.
    *   Página y componentes frontend bajo "Maestros > Familias de Artículos".
    *   Seeds de datos demo y traducciones (es, en, ca).
*   **Fuera de Alcance:**
    *   Cambios en la entidad Article o en otros módulos fuera de Product.
    *   Histórico de cambios de familia en artículos existentes.
    *   Migración masiva de datos desde otros sistemas.

---

## 3. Modelo de Datos y Reglas

### 3.1. Entidad: ArticleFamily

| Campo | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| Id | Guid | PK | Identificador único. |
| CompanyId | Guid | FK (Company), obligatorio | Tenancy; datos aislados por compañía. |
| Code | string | Obligatorio, único por CompanyId, MaxLength 50 | Código interno (ej. "MET"). |
| Name | string | Obligatorio, MaxLength 100 | Nombre (ej. "Metales"). |
| Description | string? | Opcional, MaxLength 500 | Descripción libre. |
| TaxTypeId | Guid | FK (TaxType), obligatorio | Tipo de tasa por defecto para la familia. |

*   **Tabla BD:** `ArticleFamilies`.
*   **Herencia:** `BaseEntity` (Id, IsActive, CreatedAt, etc., según convención del proyecto).
*   **Navegación:** `Company`, `TaxType`.

### 3.2. Reglas de Negocio
1.  **Unicidad:** El par `(CompanyId, Code)` debe ser único.
2.  **Multitenancy:** Todas las lecturas/escrituras filtradas por `CompanyId` (contexto de usuario).
3.  **TaxType:** `TaxTypeId` debe existir y pertenecer a la misma `CompanyId`.
4.  **Borrado:** Soft delete (IsActive = false / DeletedAt según convención actual del módulo Product).

---

## 4. Arquitectura y Componentes Afectados

### 4.1. Backend (Product.Back)
*   **Domain:** `src/Product/Back/domain/Entities/ArticleFamily.cs`
*   **Persistence:** Configuration en `Infrastructure/Persistence/Configurations/`, `DbSet<ArticleFamily>` en `ApplicationDbContext`, migración EF Core (índices: CompanyId, Code).
*   **Application:** DTOs en `application/DTOs/ArticleFamilies/` (Create, Update, Read). Commands en `application/Commands/ArticleFamilies/`. Queries en `application/Queries/ArticleFamilies/`. Handlers en `application/Handlers/ArticleFamilies/`.
*   **API:** `ArticleFamiliesController` en `Api/Controllers/`.
*   **Tests:** Unit en `GesFer.Product.UnitTests/ArticleFamilies/`, integración en `GesFer.Product.IntegrationTests/Controllers/ArticleFamiliesControllerTests.cs`.
*   **Seeds:** Entrada en `Infrastructure/Data/Seeds/demo-data.json` y uso en `JsonDataSeeder` (orden: TaxTypes antes que ArticleFamilies para respetar FK).

### 4.2. Frontend (Product.Front)
*   **Types:** `src/Product/Front/lib/types/article-family.ts`
*   **API:** `src/Product/Front/lib/api/article-families.ts`
*   **I18n:** `locales/{es,en,ca}/translation.json` (claves para "Familia de Artículo", campos y mensajes de error).
*   **Page:** `app/[locale]/(app)/maestros/familias-articulos/page.tsx`
*   **Components:** `ArticleFamilyTable` (listado + acciones), `ArticleFamilyForm` (crear/editar con selector de TaxType).
*   **Navegación:** Entrada "Familias de Artículos" en Sidebar bajo "Maestros". Ruta: `/maestros/familias-articulos`.

---

## 5. Requisitos de Seguridad

*   **Autorización:** Endpoints protegidos con `[Authorize]`; uso del contexto de usuario para `CompanyId`.
*   **Validación de entrada:** Validadores FluentValidation para Commands (Create/Update); rechazo de códigos duplicados y TaxTypeId inválido.
*   **Privacidad:** No se consideran datos PII; solo datos maestros de negocio.
*   **Consistencia:** No exponer datos de otras compañías (validación en handlers por CompanyId).

---

## 6. Criterios de Aceptación

- [ ] El backend compila sin errores (`dotnet build src/Product/Back/GesFer.Product.sln`).
- [ ] Tests unitarios e de integración pasan (`dotnet test src/Product/Back/GesFer.Product.sln`).
- [ ] El frontend compila (`npm run build --prefix src/Product/Front`).
- [ ] Navegación a `/maestros/familias-articulos` y CRUD operativo:
    - [ ] Crear: validación de Code único y TaxType obligatorio.
    - [ ] Listar: solo registros de la compañía del usuario.
    - [ ] Editar: actualización de nombre, descripción y TaxType.
    - [ ] Borrar: soft delete (o físico según estándar del módulo).
- [ ] Seeds de demo cargados correctamente y visibles en UI (TaxTypes + ArticleFamilies).
- [ ] Traducciones (es, en, ca) para etiquetas y errores.

---

## 7. Seeds de demo (impuestos habituales en España)

### 7.1. TaxTypes en `demo-data.json`
Incluir en la clave `taxTypes` los tipos impositivos estándar en España, por compañía demo (`companyId` coherente con `users`/`companies` existentes). Validar antes de instanciar (véase §8.2 Seguridad).

| Code   | Name                 | Value | Descripción breve        |
|--------|----------------------|-------|--------------------------|
| IVA21  | IVA General 21%      | 21.00 | Tipo general (Ley del IVA) |
| IVA10  | IVA Reducido 10%     | 10.00 | Tipo reducido            |
| IVA4   | IVA Superreducido 4% | 4.00  | Tipo superreducido       |
| EXENTO | Exento               | 0.00  | Operaciones exentas      |

*   **Estructura por ítem:** `id` (Guid), `companyId` (Guid), `code`, `name`, `description` (opcional), `value` (decimal).
*   **Orden de carga:** TaxTypes se cargan antes que ArticleFamilies (FK `TaxTypeId`).
*   **Ejemplo** (reemplazar `COMPANY_ID_DEMO` por el Guid de la compañía demo, p. ej. `550e8400-e29b-41d4-a716-446655440000`):

```json
"taxTypes": [
  { "id": "11111111-1111-1111-1111-111111111101", "companyId": "COMPANY_ID_DEMO", "code": "IVA21", "name": "IVA General 21%", "description": "Tipo general (Ley 37/1992)", "value": 21.00 },
  { "id": "11111111-1111-1111-1111-111111111102", "companyId": "COMPANY_ID_DEMO", "code": "IVA10", "name": "IVA Reducido 10%", "description": "Tipo reducido", "value": 10.00 },
  { "id": "11111111-1111-1111-1111-111111111103", "companyId": "COMPANY_ID_DEMO", "code": "IVA4", "name": "IVA Superreducido 4%", "description": "Tipo superreducido", "value": 4.00 },
  { "id": "11111111-1111-1111-1111-111111111104", "companyId": "COMPANY_ID_DEMO", "code": "EXENTO", "name": "Exento", "description": "Operaciones exentas", "value": 0.00 }
]
```

### 7.2. ArticleFamilies en `demo-data.json`
Incluir en la clave `articleFamilies` (o el nombre acordado para el DTO de seed) ejemplos que referencien los `taxTypes` anteriores, por ejemplo familias "Metales", "Plásticos", "Consumibles" con `taxTypeId` apuntando a IVA21 o IVA10 según corresponda.

*   **Estructura por ítem:** `id`, `companyId`, `code`, `name`, `description` (opcional), `taxTypeId` (Guid de un TaxType del mismo `companyId`).
*   **Actualización del seeder:** Añadir método `SeedArticleFamiliesAsync` (o equivalente) y llamada tras `SeedTaxTypesAsync`; extender el DTO de seed de demo (ej. `ArticleFamilySeed`) en `JsonDataSeeder`.

---

## 8. Requisitos por agente (Arquitecto, Seguridad, Auditoría)

### 8.1. Arquitecto (openspecs/agents/architect.json)
*   **Fronteras:** Toda la lógica y datos de ArticleFamily permanecen en **Product**. Shared no importa Product/Admin; Product no importa Admin.
*   **Ubicación estricta:** Entidad en `src/Product/Back/domain/Entities/`; persistencia en `src/Product/Back/Infrastructure/`; aplicación (Commands, Queries, Handlers, DTOs) en `src/Product/Back/application/`.
*   **Value Objects:** Donde aplique (p. ej. códigos o identificadores con reglas), preferir ValueObjects del dominio en lugar de strings crudos; en seeds, validar formato antes de instanciar entidades.

### 8.2. Seguridad (openspecs/agents/security-engineer.json)
*   **Validación de seeds:** Validar datos de `demo-data.json` (y cualquier MassLoad) **antes** de instanciar entidades: `Guid.TryParse` para Ids, `CompanyId` coherente con empresas existentes, `Value >= 0` para TaxType, longitudes máximas (Code, Name, Description). Si la validación falla, no crear entidades y registrar en log.
*   **Autorización:** Endpoints con `[Authorize]`; uso de contexto de usuario para `CompanyId`; no exponer datos de otras compañías.
*   **Acciones destructivas (UI):** El borrado de una familia (soft delete o físico) debe usar el patrón de confirmación explícita (p. ej. `<DestructiveActionConfirm>` o equivalente) en frontend.
*   **Valoraciones sensibles:** No hay PII en TaxType/ArticleFamily; emails u otros datos sensibles en otros seeds deben usar ValueObjects (p. ej. Email) donde el agente de seguridad lo exija.

### 8.3. Auditoría (Backend, Proceso, QA)
*   **Backend (auditor.back):** Product DbContext **no** debe incluir DbSets de Audit/Log (aislamiento). Uso consistente del patrón Command y async/await (evitar CS1998).
*   **Proceso (auditor.process):** Interacciones que modifiquen documentación o ejecuten specs deben quedar registradas en `docs/audits/ACCESS_LOG.md` según protocolo.
*   **QA Judge:** Para cerrar la rama/PR: documentación de rama presente, tests para la nueva lógica (unit + integración ArticleFamilies), compilación correcta. Ejecutar `scripts/validate-pr.ps1` y `dotnet test`. Si corresponde, generar `docs/audits/YYYYMMDD_HHMM_<BRANCH>_CLOSE.md`.

---

## 9. Verificación Post-Implementación

*   Build y tests según criterios anteriores.
*   Revisión de migración (índices en CompanyId y Code).
*   Comprobación de que el selector de TaxType en el formulario solo muestra tipos de la compañía actual.

---

## 10. Trazabilidad

*   **Documento de objetivos:** `.docs/feature/article-family/objetive.md`
*   **Plantilla usada:** Maestro CRUD (TaxType como referencia).
*   **Ubicación final sugerida (tras validación):** `openspecs/specs/SPEC-GF-2026-ARTICLE-FAMILY.md` o `docs/Feature/article-family/SPEC-ARTICLE-FAMILY-CRUD.md` según convención del proyecto.

---

*Propuesta generada a partir del análisis del documento de objetivos. Pendiente de tus comentarios para ajustar alcance, reglas o criterios antes de dar por cerrada la especificación.*
