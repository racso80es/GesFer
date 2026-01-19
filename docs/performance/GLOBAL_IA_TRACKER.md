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

- `IA_PERF_feat-architectural-alignment-S-Plus.md` — Rama `feat/architectural-alignment-S-Plus` (canónico para el Juez)
- `IA_PERF_feat-architectural-alignment.md` — Rama `feat/architectural-alignment-S-Plus` (alias humano)
- `IA_PERF_architectural-alignment.md` — Rama `feat/architectural-alignment-S-Plus` (alias humano adicional)
- `IA_PERF_chore-master-passport.md` — Rama `chore/master-passport` (pasaporte troncal + excepcion informativa en Juez)
- `IA_PERF_fix-admin-auth-isolation.md` — Rama `fix/admin-auth-isolation` (bifurcación de dominio Admin ↔ Cliente)
- `IA_PERF_fix-cleanup-and-roles-definition.md` — Rama `fix/cleanup-and-roles-definition` (purificación de ruido + soberanía de roles/permisos)

---

## Operaciones (eventos de cierre)

- **2026-01-19** — `fix/admin-auth-isolation`
  - **Resultado**: VERDE (Juez Modular + build backend + tests FE; E2E con advertencia de entorno).
  - **Acción**: aislamiento total del dominio Admin (sin empresa/tenant) + nueva Ley de Invariancia y Soberanía de Dominio.
  - **Commit**: `04a0c78558325b5b09160e6e55fe77bfc2138111`

- **2026-01-19** — `fix/cleanup-and-roles-definition`
  - **Resultado**: VERDE (Juez Modular: `validate-commit` + `validate-pr`; E2E con advertencia de entorno).
  - **Acción**: eliminación de ruido/instrumentación temporal + consolidación de roles (Administrador Global/Empresa/Operativos) y regla de validación granular de acción.
  - **Commit**: `29ff67af5d922cc53c0f8c1157e256bf695f30b7`

