# Propuesta: Nueva acción «feature» en openspecs/actions

**Fecha:** 2026-02-10  
**Rama:** feat/e2e-product-back-mocked  
**Estado:** Aprobado por el usuario (2026-02-10)  

---

## 1. Objetivo de la propuesta

Definir una nueva acción en `openspecs/actions` que documente el **procedimiento de ciclo completo de una feature** ejecutado en esta rama, de modo que sea reutilizable y alineado con la filosofía de las acciones existentes (spec, clarify, plan).

**Nombre de la acción:** `feature`.

---

## 2. Contenido de la acción propuesta

La acción **feature** queda descrita en **`openspecs/actions/feature.md`** e incluye:

| Sección | Contenido |
|--------|------------|
| **Propósito** | Procedimiento formal de ciclo completo: rama → documentación → spec → clarify → plan → implementación → cierre y PR. Orquesta spec, clarify y plan. |
| **Alcance del procedimiento** | Tabla con las 7 fases (0–6): Preparar entorno, Documentación con objetivos, Especificación, Clarificación, Planificación, Implementación, Cierre y PR. |
| **Implementación** | No hay un único comando de consola; se implementa como procedimiento. Uso opcional de `--spec`, `--clarify`, `--plan`. **Ubicación obligatoria** de la documentación de la tarea: `docs/Feature/<nombre_feature>/`. |
| **Contenido mínimo de docs/Feature** | OBJETIVO.md, SPEC, CLARIFICATIONS, PLAN. |
| **Evolution Logs** | Actualización de `docs/EVOLUTION_LOG.md` y `docs/evolution/EVOLUTION_LOG.md` en la fase 6. |
| **Integración con agentes** | Arquitecto, Clarifier, Tekton, Knowledge-Architect. |
| **Dependencias** | feature utiliza spec, clarify y plan; documentación canónica en `docs/Feature/`. |
| **Estándares de calidad** | Grado S+, Ley GIT, SSOT en `docs/Feature/<nombre_feature>/`. |
| **Referencia de ejecución** | Rama feat/e2e-product-back-mocked como ejemplo. |

---

## 3. Diferencias respecto a las acciones existentes

- **spec, clarify, plan:** Tienen comando de consola (`GesFer.Console --spec`, etc.) y generan artefactos en `openspecs/`.
- **feature:** Es una **acción de proceso** (meta-acción): no tiene comando propio; define el flujo y las ubicaciones. La documentación de la tarea se exige en `docs/Feature/<nombre_feature>/`, manteniendo coherencia con la decisión de esta rama.

---

## 4. Archivo creado

- **Ruta:** `openspecs/actions/feature.md`
- **Incluido en este commit** (o en el siguiente según decida el usuario).

---

## 5. Decisión

- [x] **Aprobado** la acción `feature` en `openspecs/actions/feature.md`. Se mantiene y se puede referenciar desde AGENTS.md u otros procesos.

---

**Firma propuesta:** Tekton / Knowledge-Arch (procedimiento documentado a partir de la ejecución en feat/e2e-product-back-mocked).
