# PLAN: E2E Product Back con dependencias mockeadas

**Date:** 2026-02-10
**Source Spec:** openspecs/specs/e2e-product-back-mocked.md
**Source Clarify:** openspecs/specs/e2e-product-back-mocked_CLARIFICATIONS.md
**Rama:** feat/e2e-product-back-mocked

## Goal

Añadir y asegurar tests E2E que validen el Product Back (API) ejecutando contra el mock, de forma que no se requiera API real ni BD.

## Context

- Ya existen: `src/Product/Front/tests/api/auth-api.spec.ts`, `usuarios-api.spec.ts` y `playwright.api-only.config.ts` con `test:e2e:api`.
- Mock en `infrastructure/mock-apis` (Product en 5002) con auth, user (CRUD), health.
- Las clarificaciones fijan: usar Playwright API-only contra mock; cubrir login + al menos un recurso (usuarios ya cubierto); documentar ejecución.

## Implementation Plan (Task Roadmap)

- [x] **Step 1: [E2E-MOCK] Verificar mock y global-setup**
  - Confirmar que `tests/global-setup.ts` permite `USE_MOCK_API=1` y no falla cuando el mock está en 5002.
  - Levantar mock (`npm run start` en `infrastructure/mock-apis`) y ejecutar `$env:USE_MOCK_API="1"; $env:API_URL="http://127.0.0.1:5002"; npm run test:e2e:api` en Product Front.
  - **[VERIFY]** Todos los specs en `tests/api/*.spec.ts` pasan contra el mock.

- [x] **Step 2: [E2E-MOCK] Ajustar tests API para compatibilidad con mock**
  - Revisar `auth-api.spec.ts` y `usuarios-api.spec.ts` por diferencias de contrato (ej. campos, códigos HTTP) entre API real y mock; ajustar expectativas o mock si es necesario para que pasen con mock.
  - **[VERIFY]** `npm run test:e2e:api` con mock (5002) pasa sin errores.

- [x] **Step 3: [E2E-MOCK] Añadir spec E2E de companies (opcional si mock lo soporta)**
  - Si el mock expone endpoints de company (GET/POST/DELETE /api/company), añadir `tests/api/companies-api.spec.ts` que ejecute contra mock.
  - Si el mock no los expone, documentar que la cobertura E2E con mock cubre auth + usuarios; companies queda para API real o ampliación futura del mock.
  - **[VERIFY]** Suite `test:e2e:api` estable con mock.

- [x] **Step 4: [DOC-RUNBOOK] Documentación**
  - Actualizar `src/Product/Front/tests/README.md` con sección "E2E API (Product Back) con mock": comando para levantar mock, variables `USE_MOCK_API`, `API_URL`, y ejecución de `npm run test:e2e:api`.
  - Opcional: enlace o párrafo en `docs/infrastructure/MOCK_APIS_AND_TEST_MODES.md` referenciando estos E2E.
  - **[VERIFY]** Un desarrollador puede seguir el README y ejecutar E2E contra mock sin API real.

- [x] **Step 5: [VERIFY] Cierre**
  - Ejecutar `dotnet build` en raíz (si se tocó Back); ejecutar `npm run build` en Product Front (si se tocó Front).
  - Ejecutar `npm run test:e2e:api` con mock y confirmar que todos los tests pasan.
  - Actualizar `docs/evolution/branches/feat-e2e-product-back-mocked.md` con estado "Implementado" y resumen.

## Risks & Mitigation

- **Risk 1:** Diferencias de contrato entre mock y API real hacen fallar tests cuando se cambie a API real.
  - *Mitigation:* Documentar que la suite está pensada para ejecución con mock; si se ejecuta contra API real, revisar expectativas (ej. token vs userId) en specs.
- **Risk 2:** global-setup exige API disponible y falla en CI si el mock no está levantado.
  - *Mitigation:* El global-setup ya contempla `USE_MOCK_API`; asegurar que en CI el mock se levante antes de los tests o que el skip sea explícito y documentado.
