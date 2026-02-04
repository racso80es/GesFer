# Rama: `feat/saas-structure-and-consolidation` — Pasaporte (S+)

**Repositorio**: GesFer (Paciente 0)  
**Propósito del documento**: Documento obligatorio de referencia para esta rama.  
**Movimiento**: 12 — Consolidación estratégica y estructura SaaS (sellado definitivo).  

---

## Objetivo de la rama

Consolidar la consciencia del sistema y el empaquetado SaaS:

- Unificar la inteligencia histórica de telemetría IA en `docs/EVOLUTION_LOG.md` (con KPIs de salud explícitos).
- Profesionalizar y dejar inequívocos los **tiers SaaS** (Demo / Funcional / Premium) y la regla de soberanía contractual.
- Codificar la **Regla de Sincronización (DoD)**: una tarea no termina hasta que **local y nube son espejo**.
- Eliminar artefactos redundantes (reportes antiguos) una vez consolidada la inteligencia.

---

## Alcance

- Documentación soberana:
  - `docs/EVOLUTION_LOG.md`
  - `docs/BUSINESS_DOMAIN.md`
  - `Tekton/Rules/GOLDEN_RULES.md`
- Telemetría IA:
  - Mantener artefactos obligatorios (`docs/performance/GLOBAL_IA_TRACKER.md` + template).
  - Generar `docs/performance/IA_PERF_feat-saas-structure-and-consolidation.md` para cumplimiento del Juez en esta rama.
  - Consolidar y eliminar reportes antiguos redundantes.

---

## Criterios de éxito (DoD de esta rama)

- `docs/EVOLUTION_LOG.md` contiene:
  - KPIs de salud definidos: **Aislamiento de Dominio**, **Latencia de Operación**, **Integridad de Derechos**, **Agilidad de Contratación**.
  - Hitos y aprendizajes consolidados (sin necesidad de mantener reportes antiguos).
- `docs/BUSINESS_DOMAIN.md` define tiers SaaS con claridad profesional:
  - **Demo** (Báscula) con límites de tiempo/volumen.
  - **Funcional** (Caja/Stock/Usuarios).
  - **Premium** (Analítica avanzada, multi‑sede, contratos marco).
  - Regla: la soberanía de la empresa está supeditada al **contrato de producto activo**.
- `Tekton/Rules/GOLDEN_RULES.md` incorpora la **[DOD: REGLA DE SINCRONIZACIÓN]** (local y nube como espejo).
- `scripts/validate-pr.ps1` ejecuta en **VERDE** (E2E puede quedar como advertencia si el entorno no expone servicios).
- `master` queda actualizado y sincronizado con `origin/master` y el entorno local queda purgado de ramas temporales.

