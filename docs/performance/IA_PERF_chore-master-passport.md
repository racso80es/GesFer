# IA Performance Report — master-passport (S+)

**Repositorio**: GesFer (Paciente 0)  
**Rama**: `chore/master-passport`  
**Fecha**: 2026-01-19  
**Objetivo**: crear pasaporte de `master` y ajustar el Juez para que en `master/main` el check de documentacion sea informativo (no bloqueante).

---

## 0) Resumen ejecutivo

Se agrego el pasaporte `docs/branches/master.md` y se adapto el Juez (`scripts/validate-pr.ps1`) para que en ramas troncales (`master/main`) el check de documentacion (y el reporte IA por rama) no bloquee el flujo. En ramas no troncales se mantuvo el enforcement bloqueante (S-Grade) para doc y telemetria.

---

## 1) First-shot Success

**Resultado**: Medio

- Se requirio una iteracion adicional por la regla de telemetria IA por rama: el Juez bloqueo la rama temporal hasta crear `IA_PERF_chore-master-passport.md`.

---

## 2) Refactor Density

**Resultado**: Media

- Cambio focalizado en el Juez (excepcion para troncal) y documentacion (pasaporte).

---

## 3) Context Leaks

**Resultado**: Bajo

- **Resueltas**:
  - Evitar bloqueo artificial en `master/main` por documentacion de rama.
- **Pendientes**:
  - Disponibilidad real de PowerShell 7 (`pwsh`) en la maquina local (no forma parte de este cambio).

---

## 4) Manifesto Alignment

- **Soberania de Racso**: OK — troncal documentada y reglas centralizadas.
- **Proactividad**: OK — excepcion explicita para evitar friccion operativa en pasaporte troncal.
- **Rigor Tecnico**: OK — validacion por Juez antes de consolidar.

