# Plan de Acción Kaizen

Este documento rastrea las acciones de mejora continua identificadas y su estado.

## Backlog Priorizado

| Prioridad | Tarea | Estado | Origen |
|-----------|-------|--------|--------|
| **Alta** | **Implementar verificación explícita de `docker-compose` en Console** | **Pendiente** | Día 6 |
| Media | Crear scripts de inicio cross-platform (`.sh`/`.ps1`) para reemplazar `.bat` | Pendiente | Día 1 |
| Baja | Eliminar otros warnings menores en `GesFer.Console` | Pendiente | Día 1 |

## Acciones en Progreso (Día 6)

1.  **Robustez de Console:**
    *   Crear `CheckDockerComposeCommand`.
    *   Integrar validación en `MenuService`.

## Histórico de Completados

*   **Día 1:** Crear `GesFer.sln` global en la raíz.
*   **Día 2:** Solucionado error de compilación `Duplicate Attribute` en `GesFer.Admin.Api`.
*   **Día 3:** Mejoras de estabilidad en Console (Null checks).
*   **Día 4:** Reparación de rutas relativas en `.csproj` y limpieza de código muerto (`Program2.cs`).
*   **Día 5:** Verificación de ejecución de Consola y análisis de warnings de infraestructura.
*   **Día 5 (Verificado Día 6):** Refactorizar `IAsyncLogPublisher` a `void`.
*   **Día 5 (Verificado Día 6):** Corregir warnings CS8629 en `ApplicationDbContext`.
