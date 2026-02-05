# DEUDA TÉCNICA DOCUMENTAL

**Fecha:** 2026-02-04
**Estado:** Pendiente

---

## 1. Actualización de Scripts Tekton
**Prioridad:** Alta
**Descripción:** Los scripts en `docs/operations/scripts/` (antiguo `Tekton/Tools/`) y configuraciones en `docs/operations/tekton/` contienen rutas hardcodeadas (ej. `./Tekton/Configuration`).
**Acción:** Refactorizar scripts para apuntar a la nueva estructura `docs/operations/`.

## 2. Validación de Propuesta Legacy
**Prioridad:** Media
**Descripción:** El archivo `docs/evolution/proposals/PROPUESTA_REORGANIZACION.md` es legacy.
**Acción:** Revisar si su contenido es vigente o descartable. Integrar ideas valiosas en `EVOLUTION_LOG.md` y eliminar.

## 3. Constitución de IA
**Prioridad:** Alta
**Descripción:** `docs/governance/AI_CONSTITUTION.md` ha sido reemplazado por `openspecs/constitution.json`.
**Acción:** Expandir con reglas detalladas de prompts, contextos y manejo de errores.

## 4. Referencias Cruzadas
**Prioridad:** Media
**Descripción:** Actualizar referencias a `Tekton/` en `AGENTS.md` y otros documentos de gobernanza.
