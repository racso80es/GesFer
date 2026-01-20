# INIT — `feat/reorganize-tekton-core`

**Fecha**: 2026-01-20  
**Objetivo**: Refactorizar y centralizar la infraestructura de Tekton en un directorio raíz `/Tekton`.

---

## init_standard_sequence — Evidencia mínima

- **Base**: `master` actualizado y sincronizado con `origin/master`.
- **Rama**: `feat/reorganize-tekton-core` creada.
- **Pasaporte de rama**: `docs/branches/feat-reorganize-tekton-core.md`.
- **Telemetría IA**: `docs/performance/IA_PERF_feat-reorganize-tekton-core.md` (generado en esta rama).

---

## Build Check (antes de mover archivos)

- Comando: `dotnet build "Api/GesFer.sln"`
- Resultado: **OK** (0 errores; advertencias toleradas).

