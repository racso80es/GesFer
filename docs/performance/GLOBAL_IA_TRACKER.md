# GLOBAL IA TRACKER — GesFer (Paciente 0)

Este archivo consolida la telemetría de desempeño de IA por rama/tarea. Es **obligatorio** y el Juez Modular lo valida.

---

## Métricas

### 1) Acierto al primer disparo

- **Definición**: porcentaje de acciones que salen correctas en el primer intento (sin re-trabajo).
- **Evidencia**: referencias a comandos, validaciones o cambios que requirieron iteraciones.

### 2) Densidad de refactorización

- **Definición**: cambios estructurales (reducción de fragmentación, consolidación de reglas, enforcement) / cambios totales.
- **Evidencia**: número de archivos tocados, eliminación de duplicidades, reducción de “constituciones” paralelas.

### 3) Fugas de contexto

- **Definición**: contradicciones, reglas duplicadas, o fuentes de verdad múltiples que permanecen tras la intervención.
- **Evidencia**: lista de contradicciones resueltas / pendientes.

---

## Registro de reportes

Los reportes `IA_PERF_<rama>.md` existen para satisfacer el enforcement del Juez Modular durante el trabajo de una rama.

- **Regla**: al cierre, la inteligencia valiosa se consolida en `docs/EVOLUTION_LOG.md` y los reportes históricos redundantes pueden eliminarse para evitar fragmentación.
- **Artefactos obligatorios por infraestructura** (no eliminables):
  - `docs/performance/GLOBAL_IA_TRACKER.md`
  - `docs/performance/templates/IA_PERF_REPORT.md`

---

## Operaciones (eventos de cierre)

- **2026-01-19** — `fix/admin-auth-isolation`
  - **Resultado**: VERDE (Juez Modular + build backend + tests FE; E2E con advertencia de entorno).
  - **Acción**: aislamiento total del dominio Admin (sin empresa/tenant) + nueva Ley de Invariancia y Soberanía de Dominio.
  - **Commit**: `04a0c78558325b5b09160e6e55fe77bfc2138111`

- **2026-01-19** — `fix/cleanup-and-roles-definition`
  - **Resultado**: VERDE (Juez Modular: `validate-commit` + `validate-pr`; E2E con advertencia de entorno).
  - **Acción**: eliminación de ruido/instrumentación temporal + consolidación de roles (Administrador Global/Empresa/Operativos) y regla de validación granular de acción.
  - **Commit**: `8b9af0d3e7812c6bbe935e7a020b7bf928e834c8`

