# Plan de Acción Kaizen

Este documento rastrea las acciones de mejora continua identificadas y su estado.

## Backlog Priorizado

| Prioridad | Tarea | Estado | Origen |
|-----------|-------|--------|--------|
| **Critica** | **Corregir errores de compilación (Build Failed)** | **Completado** | Día 4 |
| **Alta** | **Verificar ejecución de `GesFer.Console`** | **Completado** | Día 4 |
| Media | Refactorizar `IAsyncLogPublisher` a `void` (Fire-and-Forget) | Pendiente | Día 5 |
| Media | Corregir warnings CS8629 en `ApplicationDbContext` | Pendiente | Día 5 |
| Baja | Crear scripts de inicio cross-platform (`.sh`/`.ps1`) para reemplazar `.bat` | Pendiente | Día 1 |
| Baja | Eliminar otros warnings menores en `GesFer.Console` | Pendiente | Día 1 |

## Acciones en Progreso (Día 5)

1.  **Refactorización de Logging Infrastructure:**
    *   Cambiar firma de `IAsyncLogPublisher.PublishLog` de `Task` a `void`.
    *   Eliminar `async/await` innecesario en `AsyncLogPublisher` (manteniendo `Task.Run` interno).
    *   Esto solucionará automáticamente el warning en `AdminApiLogSink`.

2.  **Limpieza de Código:**
    *   Corregir acceso nullable en `ApplicationDbContext`.

## Histórico de Completados

*   **Día 1:** Crear `GesFer.sln` global en la raíz.
*   **Día 2:** Solucionado error de compilación `Duplicate Attribute` en `GesFer.Admin.Api`.
*   **Día 3:** Mejoras de estabilidad en Console (Null checks).
*   **Día 4:** Reparación de rutas relativas en `.csproj` y limpieza de código muerto (`Program2.cs`).
*   **Día 5:** Verificación de ejecución de Consola y análisis de warnings de infraestructura.
