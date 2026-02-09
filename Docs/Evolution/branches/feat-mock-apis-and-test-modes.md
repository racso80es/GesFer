# feat/mock-apis-and-test-modes — APIs mock y modos de test (sin API / sin frontend)

## Objetivo

- Permitir **validar los clientes** (Product Front, Admin Front) cuando no hay acceso a las APIs reales, usando APIs mock.
- Permitir **testear la API como cliente** (Playwright tests de API) sin levantar el frontend, contra API real o mock.

## Alcance

- **infrastructure/mock-apis:** Servidor mock (Express) que simula Product API (5002) y Admin API (5012): login y health.
- **Product Front:** global-setup con soporte `USE_MOCK_API`; playwright.api-only.config.ts para tests solo de API; scripts test:e2e:api.
- **Documentación:** docs/infrastructure/MOCK_APIS_AND_TEST_MODES.md, scripts/Propuesta (dos propuestas de uso rápido), README de tests y de mock-apis.

## Ley aplicada

- **GIT:** No commits en master; trabajo en rama feat/mock-apis-and-test-modes con documentación en docs/evolution/branches.

## Referencias

- RUNBOOK_LOGIN_EMERGENCY.md, INFRASTRUCTURE_MAP.md, infrastructure/mock-apis/README.md.
