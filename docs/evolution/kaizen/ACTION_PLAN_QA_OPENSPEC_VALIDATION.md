# [KAIZEN] Validación Automática de OpenSpec

> **Estado:** Pendiente
> **Prioridad:** Alta
> **Asignado:** [QA-JUDGE] / [TEKTON]

## Contexto
Hemos migrado la definición de agentes de Markdown a JSON estricto bajo el estándar OpenSpec en `openspecs/`. Actualmente, la integridad de estos archivos depende de la disciplina manual.

## Objetivo
Implementar un mecanismo de validación automática ("The Analyzer") que asegure que la constitución y los agentes cumplen con el esquema definido.

## Especificación Técnica
Crear script `scripts/analyze-specs.ps1` que realice las siguientes validaciones:

1.  **Sintaxis JSON:** Todos los archivos `.json` en `openspecs/` deben ser válidos.
2.  **Schema Compliance (Agentes):**
    *   Campos obligatorios: `spec_version`, `agent_id`, `name`, `system_prompt`.
    *   `skills` debe ser un array de strings.
3.  **Integridad Referencial:**
    *   Cada skill listado en `agent.json` (ej: `"git-operations"`) debe existir como archivo en `openspecs/skills/git-operations.json`.
4.  **Constitución:**
    *   Validar que `openspecs/constitution.json` existe y tiene `universal_laws`.

## Criterios de Aceptación
- El script devuelve `exit 0` si todo está correcto.
- El script devuelve `exit 1` y lista los errores si falla.
- Integrar este script en el pipeline de `validate-pr.ps1`.
