# Rama: `feat/tekton-scope-integration` — Pasaporte (S+)

**Repositorio**: GesFer (Paciente 0)  
**Propósito del documento**: Documento obligatorio de referencia para esta rama.  
**Fecha de inicio**: 2026-01-20  

---

## Objetivo de la rama

Implementar el sistema de **Ámbitos** en la ontología y operativa de Tekton, incorporando la **Fase 0: Declaración de Ámbito** como requisito previo a cualquier análisis o escritura.

---

## Alcance

- Ontología Tekton (diccionario):
  - Añadir **Ámbito** y **Validación** a `/Tekton/Configuration/DICTIONARY.json`.
- Operativa Tekton:
  - Inyectar **Fase 0: Declaración de Ámbito** en `/Tekton/Configuration/TEKTON_ACTIONS.json` como secuencia de inicio de tareas.
- Leyes operativas:
  - Regla bloqueante en `/Tekton/Rules/GOLDEN_RULES.md` sobre declarar Ámbito antes de analizar/escribir.
- Registro de evolución:
  - Entrada en `docs/EVOLUTION_LOG.md`: “Integración de Fase 0 (Ámbito) para optimización de contexto IA.”
- Validación técnica:
  - Compilación local (build check).

---

## Criterios de éxito (DoD de esta rama)

- `/Tekton/Configuration/DICTIONARY.json` contiene definiciones para:
  - **Ámbito**
  - **Validación**
- `/Tekton/Configuration/TEKTON_ACTIONS.json` incluye la **Fase 0: Declaración de Ámbito** como inicio explícito de tareas.
- `/Tekton/Rules/GOLDEN_RULES.md` incluye la regla:
  - “Es obligatorio que Tekton declare el Ámbito antes de cualquier análisis o escritura. En Tareas Complejas, el Ámbito debe encabezar el IMPLEMENTATION_PLAN.json.”
- `docs/EVOLUTION_LOG.md` registra el hito de integración de Fase 0.
- Existe y no está vacío:
  - `docs/performance/IA_PERF_feat-tekton-scope-integration.md`
- El proyecto compila tras los cambios (build check reproducible).

