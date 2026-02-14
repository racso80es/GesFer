# SPEC-REFACTOR-PRODUCT-NO-COMPANY

> **Canon:** `docs/Feature/refactor-product-no-company/SPEC-REFACTOR-PRODUCT-NO-COMPANY.md`

## 1. Información general

| Campo | Detalle |
|-------|---------|
| **ID** | SPEC-REFACTOR-PRODUCT-NO-COMPANY |
| **Rama sugerida** | refactor/product-no-company |
| **Estado** | Fase de Especificación (Objetivos + Clarificación) |
| **Base** | SPEC-COMPANY-MANAGED-BY-ADMIN |

## 2. Contexto

Se ha eliminado la dependencia de Product sobre Company. Product debe consumir exclusivamente el DTO de empresa desde Admin API (`AdminCompanyDto`). En Product solo existe **una empresa**: la del usuario actual (CompanyId del token).

## 3. Alcance

- **Incluido:** Dominio Product (entidades, DbContext, handlers, API, Front), seeds, autenticación
- **Fuera de alcance:** Admin (dueño de Company); Shared (evaluar uso de Company en Admin)

## 4. Fases

| Fase | Documento | Estado |
|------|-----------|--------|
| Objetivos | [REFACTOR-PRODUCT-NO-COMPANY-PHASE-OBJECTIVES.md](./REFACTOR-PRODUCT-NO-COMPANY-PHASE-OBJECTIVES.md) | Completada |
| Clarificación | [REFACTOR-PRODUCT-NO-COMPANY_CLARIFICATIONS.md](./REFACTOR-PRODUCT-NO-COMPANY_CLARIFICATIONS.md) | Completada |
| Análisis de impacto | Pendiente | - |
| Plan de migración | Pendiente | - |
| Implementación | Pendiente | - |

## 5. Referencias

- SPEC-COMPANY-MANAGED-BY-ADMIN
- docs/Feature/company-managed-by-admin/
- docs/Feature/separate-company-management/
