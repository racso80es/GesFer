# Plan de Acción Kaizen

Este documento rastrea las acciones de mejora continua identificadas y su estado.

## Backlog Priorizado

| Prioridad | Tarea | Estado | Origen |
|-----------|-------|--------|--------|
| **Critica** | **Corregir errores de compilación (Build Failed)** | **En Progreso** | Día 4 |
| **Alta** | Verificar ejecución de `GesFer.Console` tras fix | Pendiente | Día 4 |
| Media | Eliminar warnings en `GesFer.Console` | Pendiente | Día 1 |
| Baja | Crear scripts de inicio cross-platform (`.sh`/`.ps1`) para reemplazar `.bat` | Pendiente | Día 1 |
| Baja | Investigar warnings en `GesFer.Infrastructure` | Pendiente | Día 1 |

## Acciones en Progreso (Día 4)

1.  **Reparación de Build (`GesFer.sln`):**
    *   Eliminar duplicado `Program2.cs` en `GenerateHash`.
    *   Corregir interfaz `IAsyncLogPublisher` (Task vs void).
    *   Corregir rutas relativas rotas en `.csproj` de Tests e InitDatabase.

## Histórico de Completados

*   **Día 1:** Crear `GesFer.sln` global en la raíz (Verificado: Existe).
*   **Día 2:** Solucionado error de compilación `Duplicate Attribute` en `GesFer.Admin.Api`.
*   **Día 3:** Mejoras de estabilidad en Console (Null checks).
