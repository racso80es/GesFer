# [SPEC-ID]: E2E Product Back con dependencias mockeadas

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Especificación** | SPEC-GF-2026-E2E-PRODUCT-BACK |
| **Rama Relacionada** | feat/e2e-product-back-mocked |
| **Estado** | Implementado |
| **Responsable** | Tekton / Spec Architect |
| **Token de Auditoría** | AUDITOR-PROCESS-OK |

## 2. Propósito y Contexto

### 2.1. Objetivo (Goal)
Añadir tests E2E que validen **únicamente el backend (API) de Product**. El punto bajo test es la API Product (`src/Product/Back/Api`). Todas las dependencias de esa API (base de datos, caché, servicios externos) deben estar **mockeadas** para que los tests sean ejecutables sin infraestructura real y de forma aislada.

### 2.2. Alcance (Scope)
*   **Incluido:** Suite E2E contra la API Product (endpoints críticos: auth, user, company según contratos existentes); uso de mocks para dependencias (p. ej. `infrastructure/mock-apis` para simular respuestas o tests contra mock; o TestServer/WebApplicationFactory con in-memory/mocks en backend); documentación y scripts necesarios para ejecutar los E2E con mocks.
*   **Fuera de Alcance:** Tests E2E de frontend completo (navegador); tests que requieran API real o BD real sin mock; cambios funcionales en la API.

## 3. Arquitectura y Diseño Técnico

### 3.1. Componentes Afectados
*   **Tests E2E:** Ubicación a definir entre:
  * `src/Product/Front/tests/api/` (Playwright API-only, ya existente; ampliar specs y asegurar ejecución con mock), y/o
  * `src/Product/Back/` (tests de integración E2E con TestServer y mocks en C#).
*   **Mocks:** `infrastructure/mock-apis/` (ya existe; alinear contratos si hace falta); posible extensión para cubrir más endpoints o escenarios.
*   **Configuración:** `playwright.api-only.config.ts`, `global-setup.ts`, variables `USE_MOCK_API`, `API_URL`; posible configuración de TestServer en Back si se elige enfoque C#.

### 3.2. Modelo de Datos / Lógica
Sin cambios en modelo de datos. Los tests validan contratos HTTP existentes (login, user, company). Se debe respetar el uso de `company` en lugar de `empresa` en código nuevo; en contratos de API ya expuestos se mantiene la convención existente.

## 4. Requisitos de Seguridad

*   **Validación de Input:** Los tests no introducen datos sensibles reales; usar datos de prueba o ficticios (alineado con mock existente).
*   **Privacidad:** No persistir PII en mocks; datos de test ya definidos en seeds/mock.
*   **Autorización:** Los tests que requieran autenticación usarán credenciales de test/mock (p. ej. Empresa Demo / admin / admin123) sin hardcodear secretos en repositorio.

## 5. Criterios de Aceptación

- [ ] Existe una suite E2E ejecutable que testea al menos un flujo representativo del Product Back (p. ej. login + al menos un recurso: user o company).
- [ ] Las dependencias del Product Back están mockeadas; los E2E pasan sin API real ni BD real (p. ej. con `USE_MOCK_API=1` y mock en 5002, o con TestServer + mocks).
- [ ] El código compila sin errores (`dotnet build` en Back si se añade código C#; `npm run build` en Front si solo se toca Front).
- [ ] Los tests E2E añadidos/modificados pasan localmente (Playwright y/o dotnet test según implementación).
- [ ] Documentación actualizada (README de tests o `docs/infrastructure/`) indicando cómo ejecutar E2E para Product Back con mocks.
- [ ] El log de auditoría en `docs/audits/ACCESS_LOG.md` ha sido actualizado si se usan comandos de consola con token.

## 6. Structured Action Tags (Previstos)

*   `[E2E-MOCK]`: Tests E2E Product Back con dependencias mockeadas.
*   `[DOC-RUNBOOK]`: Actualización de documentación de ejecución de tests.

## 7. Trazabilidad de Auditoría

*   **Fecha de Creación:** 2026-02-10
*   **Evento:** Especificación inicial para feat/e2e-product-back-mocked.
*   **Referencia de Log:** `docs/audits/ACCESS_LOG.md`
*   **Rama:** feat/e2e-product-back-mocked
