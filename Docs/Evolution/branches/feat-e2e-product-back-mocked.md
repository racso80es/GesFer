# feat/e2e-product-back-mocked — Tests E2E para Product Back con dependencias mockeadas

**Estado:** Implementado (2026-02-10). Suite E2E API ejecutable contra mock; documentación actualizada.

## Objetivo

- Añadir **tests E2E que validen el backend (API) de Product** como único punto bajo test.
- Las **dependencias** que tenga la API Product (BD, caché, servicios externos, etc.) han de estar **mockeadas**, de modo que los E2E no dependan de infraestructura real y sean ejecutables de forma aislada.

## Alcance

- **Incluido:** Suite E2E contra la API Product (endpoints bajo test); uso de mocks para dependencias (p. ej. mock de APIs ya existente en `infrastructure/mock-apis`, o mocks específicos para BD/caché si aplica).
- **Fuera de alcance:** Tests E2E de frontend completo; tests que requieran API real o BD real sin mock.

## Ley aplicada

- **GIT:** No commits en master; trabajo en rama `feat/e2e-product-back-mocked` con documentación en `docs/evolution/branches`.
- **ENTORNO:** Windows 11 + PowerShell 7+.

## Proceso de feature (openspecs/actions)

0. Preparar entorno (rama) — hecho.
1. Documentación con objetivos — este documento.
2. Fase especificación (`GesFer.Console --spec`).
3. Fase clarificación (`GesFer.Console --clarify`).
4. Fase planificación (`GesFer.Console --plan`).
5. Fase implementación.
6. Cierre y PR.

## Cierre y PR

- **Commit:** `feat(e2e): E2E Product Back con mock - spec, plan, docs` (rama `feat/e2e-product-back-mocked`).
- **Para abrir PR:** `git push -u origin feat/e2e-product-back-mocked` y crear Pull Request hacia `master` en el remoto. Incluir en la descripción el enlace a esta rama y a `openspecs/specs/e2e-product-back-mocked.md`.
- **Verificación pre-PR:** Con mock levantado en 5002, ejecutar `$env:USE_MOCK_API="1"; $env:API_URL="http://127.0.0.1:5002"; npm run test:e2e:api` en `src/Product/Front` (9 tests deben pasar).

## Referencias

- `openspecs/actions/spec.md`, `clarify.md`, `planning.md`
- `src/Product/Front/tests/api/` (Playwright API-only), `playwright.api-only.config.ts`
- `infrastructure/mock-apis/`, `docs/infrastructure/MOCK_APIS_AND_TEST_MODES.md`
