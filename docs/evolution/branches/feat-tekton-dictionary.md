# Rama: `feat/tekton-dictionary` — Pasaporte (S+)

**Repositorio**: GesFer (Paciente 0)  
**Propósito del documento**: Documento obligatorio de referencia para esta rama.  
**Fecha de inicio**: 2026-01-20  

---

## Objetivo de la rama

Crear el diccionario ontológico de Tekton (`/Tekton/Configuration/DICTIONARY.json`) para unificar terminología IA/Humano en reportes, logs y mensajes de commit.

---

## Alcance

- Creación de diccionario de términos Tekton:
  - **Tarea**
  - **Acción**
- Refuerzo de gobierno:
  - Nota bajo **Telemetría IA** en `/Tekton/Rules/GOLDEN_RULES.md` para forzar uso de la terminología definida en el diccionario.
- Registro de evolución:
  - Entrada en `docs/EVOLUTION_LOG.md`.
- Validación técnica:
  - Compilación local (`dotnet build "Api/GesFer.sln"`).

---

## Criterios de éxito (DoD de esta rama)

- Existe `/Tekton/Configuration/DICTIONARY.json` y contiene definiciones refinadas con **scope** para **Tarea** y **Acción**.
- `Tekton/Rules/GOLDEN_RULES.md` incluye la nota de terminología bajo la sección **Telemetría IA**.
- `docs/EVOLUTION_LOG.md` registra el hito: “Creación del diccionario de términos Tekton para unificación de lenguaje IA/Humano.”
- Existe y no está vacío:
  - `docs/performance/IA_PERF_feat-tekton-dictionary.md`
- El proyecto compila tras los cambios (`dotnet build "Api/GesFer.sln"`).

