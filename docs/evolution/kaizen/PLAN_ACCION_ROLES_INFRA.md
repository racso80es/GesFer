# [KAIZEN] Definición de Interacción de Roles Infraestructura vs. DevOps

> **Estado:** PENDIENTE
> **Fecha Creación:** 2026-05-23
> **Prioridad:** MEDIA

## 1. SITUACIÓN ACTUAL (Diagnóstico)
Se ha creado el rol `rol_arquitecto_infra.md` (Arquitecto de Infraestructura) y existe previamente el rol `rol_tekton_dev.md` (Motor de Ejecución/DevOps). Existe un riesgo potencial de solapamiento en responsabilidades relacionadas con:
*   Creación de scripts de automatización.
*   Gestión de pipelines de CI/CD.
*   Ejecución de tareas de despliegue.

## 2. OBJETIVO KAIZEN
Clarificar y documentar la frontera operativa y los protocolos de colaboración entre el **Arquitecto de Infraestructura** (diseño, estándares, "el qué") y **Tekton** (implementación, ejecución, "el cómo").

## 3. ACCIONES PROPUESTAS
1.  **Analizar roles:** Revisar en detalle las capacidades de ambos agentes.
2.  **Definir flujos:** Establecer diagramas o reglas escritas sobre quién aprueba y quién ejecuta.
    *   *Ejemplo:* Arquitecto Infra define la estrategia de backup; Tekton escribe el script de bash/powershell y lo integra en el pipeline.
3.  **Actualizar documentación:** Modificar ambos archivos de rol (`rol_arquitecto_infra.md` y `rol_tekton_dev.md`) incluyendo una sección de "Interacción con otros agentes".

## 4. IMPACTO ESPERADO
*   Eliminación de ambigüedades.
*   Mayor eficiencia en la asignación de tareas de infraestructura.
*   Reducción de conflictos de autoridad en PRs de Ops.
