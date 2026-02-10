# Objetivos de cobertura de código — GesFer

**Objetivo global:** La cobertura de código debe tender a **100%** en los ámbitos definidos, priorizando ramas críticas (auth, permisos, validaciones) y manteniendo un mínimo en el resto.

---

## 1. Alcance por capa

| Capa / Ámbito | Objetivo | Prioridad | Notas |
|---------------|----------|-----------|--------|
| **Tests API (Playwright)** | 100% de specs pasando contra API real o mock | Alta | `tests/api/*.spec.ts`; mock cubre auth + user. |
| **Tests E2E (Playwright)** | 100% de specs pasando con API disponible | Alta | Requieren API o mock + frontend. |
| **Tests unitarios (Jest)** | ≥ 80% líneas, 100% en auth y validaciones | Media | Frontend: componentes críticos, hooks, utils. |
| **Backend (C#)** | ≥ 80% líneas, 100% en controllers de auth y comandos sensibles | Alta | Unit + integración. |
| **Mock APIs** | 100% de endpoints usados por tests API cubiertos por mock | Alta | Ver `infrastructure/mock-apis` y PROPUESTA_CORRECCION_MOCK_USUARIOS. |

---

## 2. Criterios de “tender a 100%”

- **Tests API:** Todos los `*.spec.ts` bajo `tests/api/` en verde (auth + usuarios) contra API real o mock.
- **Ramas críticas:** Auth (login, logout, permisos), validación de entrada (POST/PUT), eliminación (DELETE) con permisos.
- **Exclusiones aceptables:** Código generado, prototipos marcados como deprecated, adaptadores de terceros sin lógica de negocio.

---

## 3. Validación de cobertura

- **Playwright (API):** `npm run test:e2e:api` (Product Front) con API real o `USE_MOCK_API=1` + mock levantado.
- **Jest:** `npm run test:coverage`; umbrales configurados en `jest.config.js` si aplica.
- **Backend:** `dotnet test --collect:"XPlat Code Coverage"` (o equivalente en el repo).

---

## 4. Estado actual (tests API contra mock)

Con el mock extendido (auth + user), los **9** tests de `tests/api/` pasan al 100%:

- `auth-api.spec.ts`: 4 tests (login exitoso, rechazo inválidos, campos requeridos, info usuario).
- `usuarios-api.spec.ts`: 5 tests (lista, obtener por ID, crear y limpiar, acceso sin token, validar formato ID).

Comando de validación: `USE_MOCK_API=1 API_URL=http://127.0.0.1:5002 npm run test:e2e:api` (mock levantado en 5002).

## 5. Referencias

- Mock y endpoints cubiertos: `infrastructure/mock-apis/README.md`, `scripts/Propuesta/PROPUESTA_CORRECCION_MOCK_USUARIOS.md`
- Configuración tests API: `src/Product/Front/playwright.api-only.config.ts`
- Métricas detalladas: `docs/technical/coverage-targets.json`
