# Action: Feature

## Propósito

La acción **feature** define el procedimiento formal de ciclo completo para desarrollar una funcionalidad o tarea: desde la creación de la rama hasta el cierre y la apertura del Pull Request. Orquesta las acciones **spec**, **clarify** y **plan** en secuencia, fija la ubicación de la documentación de la tarea y garantiza trazabilidad en los logs de evolución.

Proporciona un flujo repetible y auditado, alineado con las Leyes Universales (soberanía documental en `docs/`, no commits en `master`, entorno Windows + PowerShell).

## Alcance del procedimiento

| Fase | Nombre | Descripción |
| :--- | :--- | :--- |
| **0** | Preparar entorno | Crear rama `feat/<nombre-feature>` (o `fix/` si aplica) desde `master`. No trabajar en `master`. |
| **1** | Documentación con objetivos | Documentar objetivo, alcance y ley aplicada. La documentación de la tarea se ubica en **`docs/Feature/<nombre_feature>/`**. |
| **2** | Especificación | Ejecutar o generar SPEC (acción **spec**). Entrada: requerimiento o borrador; salida: especificación técnica en `openspecs/specs/` y copia/canon en `docs/Feature/<nombre_feature>/`. |
| **3** | Clarificación | Ejecutar o generar clarificaciones (acción **clarify**). Cerrar gaps y decisiones; persistir en `openspecs/specs/` y en `docs/Feature/<nombre_feature>/`. |
| **4** | Planificación | Ejecutar o generar plan (acción **plan**). Entrada: SPEC + Clarificaciones; salida: roadmap de tareas en `openspecs/plans/` y en `docs/Feature/<nombre_feature>/`. |
| **5** | Implementación | Ejecutar el plan (Tekton Developer). Verificación: `dotnet build` / `npm run build` y tests según corresponda. |
| **6** | Cierre y PR | Commits atómicos en la rama; actualización de **Evolution Logs**; push de la rama y creación del Pull Request hacia `master`. En la descripción del PR, enlazar a `docs/Feature/<nombre_feature>/`. |

## Implementación

Esta acción **no** dispone de un único comando de consola; se implementa como **procedimiento** que combina:

*   **Comandos opcionales de consola** (cuando se requiera trazabilidad con token de auditor):
    *   `GesFer.Console --spec --token <AUDITOR_TOKEN> [--prompt <TEXT>] [--title <TITLE>]`
    *   `GesFer.Console --clarify --token <AUDITOR_TOKEN> [--spec-path <PATH>] [--context <TEXT>]`
    *   `GesFer.Console --plan --token <AUDITOR_TOKEN> --spec <PATH> [--clarify <PATH>]`
*   **Ubicación obligatoria de la documentación de la tarea:** `docs/Feature/<nombre_feature>/`.

### Contenido mínimo de `docs/Feature/<nombre_feature>/`

| Documento | Contenido |
| :--- | :--- |
| **OBJETIVO.md** | Objetivo, alcance, ley aplicada, resumen del proceso (fases 0–6), cierre y PR, referencias. |
| **SPEC-*** (o equivalente) | Especificación técnica (puede generarse con `--spec` y copiarse/adaptarse aquí). |
| **SPEC-*_CLARIFICATIONS.md** | Clarificaciones y decisiones (puede generarse con `--clarify` y copiarse aquí). |
| **PLAN-*** | Plan de implementación / task roadmap (puede generarse con `--plan` y copiarse aquí). |

### Actualización de Evolution Logs

Al cierre de la feature (fase 6):

*   **`docs/EVOLUTION_LOG.md`:** Añadir una línea con formato `[YYYY-MM-DD] [feat/<nombre>] [Descripción breve del resultado.] [Estado].`
*   **`docs/evolution/EVOLUTION_LOG.md`:** Añadir una sección con fecha y título de la feature, resumen de acción/alcance/resultado y referencia a `docs/Feature/<nombre_feature>/OBJETIVO.md`.

## Integración con Agentes

*   **Arquitecto / Spec Architect:** Puede iniciar el procedimiento y asegurar que la fase 1 (documentación con objetivos) y la ubicación `docs/Feature/<nombre_feature>/` se respeten.
*   **Clarifier:** Responsable de la fase 3 (clarificación) y de persistir decisiones en el SPEC y en la carpeta de la feature.
*   **Tekton Developer:** Ejecuta las fases 4 (plan) y 5 (implementación); aplica la SPEC como marco legal; realiza la fase 6 (commits, actualización de logs, preparación del PR).
*   **Knowledge-Architect:** Valida que la documentación de la tarea esté en `docs/Feature/<nombre_feature>/` como SSOT para esa feature.

## Dependencias con otras acciones

*   **feature** invoca o utiliza los resultados de **spec**, **clarify** y **plan** en las fases 2, 3 y 4.
*   Los artefactos generados por spec/clarify/plan pueden vivir en `openspecs/` para consumo por la consola y agentes; la **documentación de la tarea** (objetivo, spec, clarifications, plan) debe residir en **`docs/Feature/<nombre_feature>/`** para aprobación y revisión humana.

## Estándares de Calidad

*   **Grado S+:** Trazabilidad desde el objetivo hasta el PR: rama → docs/Feature → spec/clarify/plan → implementación → Evolution Logs → PR.
*   **Ley GIT:** Ningún commit en `master`; todo el trabajo en rama `feat/` o `fix/` con documentación en `docs/Feature/<nombre_feature>/`.
*   **Single Source of Truth:** Para cada feature, la documentación canónica de la tarea es la carpeta `docs/Feature/<nombre_feature>/`; los archivos en `openspecs/` pueden ser copia o origen previo, pero la referencia en PR y en Evolution Log es la ruta en `docs/Feature/`.

## Referencia de ejecución

Procedimiento aplicado en la rama **feat/e2e-product-back-mocked** (2026-02-10). Documentación de la tarea: `docs/Feature/e2e-product-back-mocked/`. Acciones relacionadas: `openspecs/actions/spec.md`, `clarify.md`, `planning.md`.
