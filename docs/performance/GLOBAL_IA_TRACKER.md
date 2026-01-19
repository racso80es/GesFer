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

