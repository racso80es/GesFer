# Rama: `feat/reorganize-tekton-core` — Pasaporte (S+)

**Repositorio**: GesFer (Paciente 0)  
**Propósito del documento**: Documento obligatorio de referencia para esta rama.  
**Fecha de inicio**: 2026-01-20  

---

## Objetivo de la rama

Centralizar la infraestructura de Tekton en un directorio raíz `/Tekton` para estandarizar herramientas de IA y evitar fragmentación de rutas/referencias.

---

## Alcance

- Reorganización de artefactos Tekton:
  - Reglas (Golden Rules)
  - Configuración/Manifiestos (JSON)
  - Plantillas Markdown
- Actualización de referencias:
  - `.cursorrules` y cualquier puntero documental que apunte a rutas anteriores.
- Validación:
  - Compilación previa y posterior para asegurar integridad del repositorio.

---

## Criterios de éxito (DoD de esta rama)

- Existe el directorio raíz `/Tekton` con subcarpetas:
  - `/Tekton/Rules`
  - `/Tekton/Configuration`
  - `/Tekton/Templates`
- `.cursorrules` referencia rutas canónicas bajo `/Tekton`.
- Los artefactos obligatorios del Juez siguen en verde:
  - `docs/branches/feat-reorganize-tekton-core.md` (este pasaporte) no vacío.
  - `docs/performance/GLOBAL_IA_TRACKER.md` existe y no vacío.
  - `docs/performance/IA_PERF_feat-reorganize-tekton-core.md` existe y no vacío.
- El proyecto compila tras los cambios (`dotnet build "Api/GesFer.sln"`).

