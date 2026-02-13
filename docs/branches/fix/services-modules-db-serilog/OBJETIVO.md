# OBJETIVO: Fix Admin Back fallos reiterados y docs/bugs

## Resumen
Rama de corrección (fix) que resuelve los fallos reiterados del Admin Back, establece garantía de detección de "proyecto no funcional" (smoke tests) y unifica documentación de bugs bajo la ruta del agente documental (fixPath + bug-id).

## Cambios principales
*   **Admin Back:** Smoke tests (AdminApiSmokeTests), Swagger en Testing, LogController sin ambigüedad, HTTPS redirect solo fuera de Development/Testing, JwtSettings y config en appsettings.Development.
*   **Documentación:** SPEC, CLARIFICATIONS y PLAN del fix en `docs/bugs/admin-back-repeated-failures/`; checklist "proyecto no funcional"; Kaizen; acción Spec con --context desde agente documental.
*   **Kaizen:** StockBenchmark.cs actualizado a ArticleFamily (eliminado Family obsoleto).
*   **Configuración:** knowledge-architect paths.fixPath; agentes consultan fixPath para documentación de bugs.

## Criterios de cierre
*   Tests Admin IntegrationTests pasan (incluidos smoke).
*   Build de solución sin errores.
*   Documentación del fix bajo docs/bugs/admin-back-repeated-failures/.
