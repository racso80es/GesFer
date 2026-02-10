# CLARIFICATION: E2E Product Back con dependencias mockeadas

**ID:** SPEC-GF-2026-E2E-PRODUCT-BACK-CLARIFY
**Date:** 2026-02-10
**Author:** Clarifier
**Status:** CLOSED
**Spec:** openspecs/specs/e2e-product-back-mocked.md

## Questions & Answers

### 1. Ubicación de los tests E2E
**Q:** ¿Los tests E2E para Product Back se implementan en el front (Playwright en `src/Product/Front/tests/api/`) o en el back (C# con TestServer/WebApplicationFactory)?
**A:** Se prioriza el enfoque **Playwright API-only** ya existente en `src/Product/Front/tests/api/`, ejecutado contra el **mock** en `infrastructure/mock-apis` (puerto 5002). Así no se requiere BD ni API real; el mock es la dependencia ya disponible y las “dependencias” del Product Back quedan sustituidas por el propio mock que simula la API. No se exige en esta iteración añadir E2E con TestServer en C#.

### 2. Qué se considera “dependencias mockeadas”
**Q:** ¿Qué dependencias del Product Back deben quedar mockeadas?
**A:** Para este objetivo, “mockear dependencias” se cumple **ejecutando los E2E contra el servidor mock** (`USE_MOCK_API=1`, `API_URL=http://127.0.0.1:5002`). El mock ya simula la API Product (auth, user, company); por tanto no se usa BD, caché ni API real. Si en el futuro se añaden E2E contra la API real, entonces se podrá usar TestServer con in-memory/mocks en backend.

### 3. Endpoints / flujos a cubrir
**Q:** ¿Qué endpoints o flujos E2E son obligatorios en esta feature?
**A:** Como mínimo: **login** (POST /api/auth/login) y **al menos un recurso** (usuarios o companies). Los specs actuales en `auth-api.spec.ts` y `usuarios-api.spec.ts` ya cubren parte; la tarea es **asegurar que existan y pasen con mock** y, si falta cobertura de companies u otros contratos críticos, añadir specs que los cubran y sigan ejecutándose con mock.

### 4. Documentación
**Q:** ¿Dónde se documenta cómo ejecutar los E2E de Product Back con mocks?
**A:** En `src/Product/Front/tests/README.md` y/o `docs/infrastructure/MOCK_APIS_AND_TEST_MODES.md`, indicando el comando (p. ej. `USE_MOCK_API=1`, `API_URL=http://127.0.0.1:5002`, `npm run test:e2e:api`) y que el mock debe estar levantado en 5002.

## Decision

Proceder con la implementación usando Playwright API-only contra el mock existente; asegurar que la suite `test:e2e:api` pase con mock; ampliar specs si falta cobertura de companies u otros contratos; actualizar documentación de ejecución.
