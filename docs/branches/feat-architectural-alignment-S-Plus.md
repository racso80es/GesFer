# Rama: `feat/architectural-alignment-S-Plus` — Documentación de Rama Obligatoria

**Repositorio**: GesFer (Paciente 0)  
**Propósito del documento**: Punto de referencia obligatorio para cada acción realizada en esta rama.  
**Alcance**: Auditoría de reglas activas, soberanía de reglas, enforcement real (hooks/scripts), contradicciones y estado del repositorio.  

---

## Objetivo de Rama

**Sincronizar el patrón de separación de contextos de Calma en GesFer**, estableciendo la nueva **Puerta de Entrada** y el **Manifiesto**.

Este objetivo guía todas las acciones en esta rama. Cualquier cambio debe poder trazarse a este objetivo (directamente o como prerequisito).

---

## Informe de Auditoría de Reglas (persistencia íntegra)

### Análisis de reglas (fuentes activas y soberanía)

- **Soberanía declarada / “fuente de reglas”**
  - **`.cursorrules`**: declara soberanía de reglas en `docs/AUTOMATION_RULES.md` y pide consultar `TEKTON_MANIFEST.json`.
  - **`TEKTON_MANIFEST.json`**: refuerza esa soberanía con `"rules": "docs/AUTOMATION_RULES.md"` y además define **política git** `"git_policy": "no_master_commit"` y **rutas canónicas** (`Api/src/Api`, `Cliente`, `Api/src/IntegrationTests`, seeds demo).

- **Reglas de automatización (agente)**
  - **`docs/AUTOMATION_RULES.md`**: **Regla de Oro “Circuit Breaker (3 Strikes)”** → si una corrección automática falla 3 veces con el mismo error, detenerse y generar `AUDIT_FAIL.md`.

- **Reglas “activas” por enforcement real (hooks/scripts)**
  - **`.husky/pre-commit`** → ejecuta `scripts/validate-commit.ps1` (Windows) o `scripts/validate-commit.sh` (Unix).
  - **`scripts/validate-commit.ps1` / `scripts/validate-commit.sh`**:
    - **Bloqueo de commits a `master/main`** (exit 1).
    - **Backend**: `dotnet build` obligatorio (si falla, bloquea).
    - **Frontend**: `npm run lint` obligatorio (si falla, bloquea).
    - **Tests**: se ejecutan pero **no bloquean** si fallan/no existen (tratados como “no crítico”).
  - **`.husky/pre-push`**: el “Juez del Proyecto” está **comentado/deshabilitado** y termina con `exit 0` (no bloquea push).
  - **`scripts/validate-pr.ps1` / `scripts/validate-pr.sh`**: describen validación completa (build + tests + e2e + orquestación de servicios). En `.ps1` hay lógica para no contar como error ciertos fallos E2E por “servicios no disponibles”.

- **Reglas de “Golden Rules” documentadas (no necesariamente enforceadas por hooks)**
  - **`AI_GUIDELINES.md`**: manual operativo con reglas de UI, tests (`data-testid`), commits, estructura Next.js por route groups, sincronización Backend↔Frontend, y protocolo “pre-push”.
  - **`DIAGNOSTICS.md`**: “Reglas de Oro de la Casa” centradas en **seeding resiliente**, **Value Objects**, **logging Kaizen** y **tests de resiliencia**.
  - **`.gesfer/docs/metrics/HEALTH_RADAR.md`** y **`.gesfer/docs/diagnostics/feature-arch-kpi-telemetry/BASELINE.md`**: definen métricas “sagradas” y acciones (Kaizen/confirm/data-test-id), más un requerimiento explícito de **actualizar `CURRENT_REF.md`**.

- **Reglas operativas en código (validadores internos)**
  - **`GesFer.Console/Services/GoldenRulesComplianceService.cs`**: valida **sincronización Seeds/Tests vs Entidades**, persiste estado en `.golden-rules-state.json` y marca entidades “requieren atención”.
  - **`GesFer.Console/Services/IntegrityValidationService.cs`**: valida ecosistema (Docker, API, Next.js, Sequential GUIDs, AdminUsers) y persiste `.validation-state.json`.

---

### Informe de estado: Reglas de Oro activas (con origen)

- **Reglas activas “hard enforcement”**
  - **No commits directos a `master/main`**: `scripts/validate-commit.ps1` + `scripts/validate-commit.sh` (invocados por `.husky/pre-commit`).
  - **Build Backend obligatorio**: `scripts/validate-commit.*`.
  - **Lint Frontend obligatorio**: `scripts/validate-commit.*`.

- **Reglas activas “soft enforcement” / recomendación ejecutable**
  - **Validación completa PR/push (Juez)**: `scripts/validate-pr.ps1` / `scripts/validate-pr.sh` (pero **hook pre-push está deshabilitado**, así que no es enforcement automático hoy).
  - **Circuit Breaker del agente**: `docs/AUTOMATION_RULES.md` (aplica al agente, no al runtime del repo).

- **Reglas de Oro de arquitectura/proceso (documentales)**
  - **Seeding resiliente + VO + logging + tests resiliencia**: `DIAGNOSTICS.md`.
  - **UI/shared, data-testid, estructura Next.js, sincronización tipos, formato de commits**: `AI_GUIDELINES.md`.
  - **Métricas sagradas + Kaizen + actualización de referencia**: `.gesfer/docs/metrics/HEALTH_RADAR.md` y `.gesfer/docs/diagnostics/feature-arch-kpi-telemetry/BASELINE.md`.
  - **Auditorías/seguimiento** (no reglas nuevas, pero señalan incumplimientos y acciones): `CUMPLIMIENTO-REGLAS-ORO-ADMIN.md`, `CHANGELOG-CORRECCION-REGLAS-ORO.md`, `TRABAJO_REQUEST_8816fc85.md`, `PR_SUMMARY.md`.

---

### Contradicciones detectadas (entre reglas y/o su aplicación)

- **Soberanía de reglas (conflicto de “fuente de verdad”)**
  - `.cursorrules` + `TEKTON_MANIFEST.json` apuntan a `docs/AUTOMATION_RULES.md` como soberano.
  - `AI_GUIDELINES.md` se autodefine “Fuente de Verdad única”.
  - `DIAGNOSTICS.md` se declara “Reglas de Oro… ACTIVO”.
  - Resultado: hay **múltiples “constituciones”** sin jerarquía explícita entre ellas (excepto la afirmación unilateral de `AI_GUIDELINES.md`).

- **Pre-push “Juez del Proyecto” (documentado vs real)**
  - `AI_GUIDELINES.md` afirma bloqueo técnico automático en pre-push.
  - `.husky/pre-push` lo tiene **comentado** y hace `exit 0` → **no bloquea**.

- **“Validación estricta” vs implementación de `validate-commit`**
  - `AI_GUIDELINES.md` describe validación pre-commit como estricta.
  - `scripts/validate-commit.*` **no falla** por tests unitarios (Backend/Frontend) si fallan/no existen (lo marca como “no crítico”). En la práctica, lo estrictamente bloqueante es **build + lint + rama**.

- **Referencia de rama activa (doble “CURRENT_REF”)**
  - `CURRENT_REF.md` (raíz) dice rama `feature/arch-kpi-telemetry` IN_PROGRESS y trae “próximos pasos” (fecha 2025-01-27).
  - `.gesfer/docs/diagnostics/CURRENT_REF.md` declara rama **`master` STABLE** y sugiere próxima rama `feature/arch-infra-docker-vo`.
  - Resultado: **contradicción directa** sobre el “contexto actual” del proyecto.

- **Indicador de mantenimiento incompleto**
  - `.gesfer/docs/metrics/HEALTH_RADAR.md` exige “`CURRENT_REF.md` actualizado”, pero el `CURRENT_REF.md` de raíz está fechado 2025-01-27 (y `.gesfer/.../CURRENT_REF.md` tiene “2026-01-XX” placeholder).

---

### Indexación de tareas y estado del repositorio (confirmación de foco)

- **Estado git**
  - **Rama activa**: `master` (local) siguiendo `origin/master`.
  - **Working tree**: limpio (sin cambios).
  - **HEAD**: `chore: implement tekton manifest and fix admin login` (según log reciente).

- **Tareas pendientes detectadas (por texto en repo)**
  - En `.gesfer/docs/diagnostics/feature-arch-optimization-types/DOC_DIAG_ARCH_TYPE_UNIFICATION.md`:
    - **“Migrar imports gradualmente”** — **PENDIENTE**
    - **“Ejecutar tests y validación de integridad”** — **PENDIENTE**
    - **“Eliminar tipos manuales obsoletos”** — **PENDIENTE**
  - En `CURRENT_REF.md` (raíz, contexto KPI/telemetry):
    - Implementar **`DestructiveActionConfirm`**
    - Reemplazar **5 usos** de `confirm()` nativo
    - Subir “Densidad Kaizen” (baseline)
  - En `.gesfer/docs/metrics/HEALTH_RADAR.md`:
    - Checklist de certificación baseline incluye **actualizar `CURRENT_REF.md`**.

- **Confirmación de enfoque**
  - Estoy **centrado al 100% en GesFer (“Paciente 0”)**: en esta auditoría no he hecho cambios; solo he identificado **qué reglas gobiernan** (por documento y por enforcement real), **qué contradicciones existen**, y **qué tareas quedan indexadas** para preparar la migración a “Puerta de Entrada” (separación de contextos).

---

## Nota operativa (para esta rama)

Este documento es **obligatorio** como referencia:
- Antes de cualquier cambio, validar que la acción:
  - respeta el “hard enforcement” (pre-commit y políticas git), y
  - reduce contradicciones o consolida soberanía (preparación para Puerta de Entrada / Manifiesto).

---

## Cierre de reestructuración (S+)

**Estado**: ✅ Reestructuración aplicada y Juez Modular activado.

### Puerta de Entrada (soberanía)

- `.cursorrules` fue sustituido para declarar explícitamente la Puerta de Entrada:
  - Gobierno conductual: `docs/MANIFESTO.md`
  - Leyes operativas: `docs/rules/GOLDEN_RULES.md`
  - Enforcement: Juez Modular bloquea acciones sin `docs/branches/`

### Blindaje del Juez Modular — Check de Documentación Inicial

Se activó el bloqueo **S-Grade** cuando falta documentación obligatoria de rama:

- Detecta rama actual (`git branch --show-current`).
- Deriva el archivo esperado reemplazando `/` por `-`:
  - `docs/branches/<rama-con-/>.md` → `docs/branches/<rama-con--> .md`
- Si el archivo **no existe** o está **vacío**, el Juez devuelve:
  - **ESTADO: ERROR (S- Grade)** y detiene el proceso.

**Puntos de enforcement**:
- Pre-push: `.husky/pre-push` (Juez activado).
- Validación: `scripts/validate-pr.ps1` (check inicial).
- Pre-commit: `scripts/validate-commit.ps1` (bloqueo antes de validar/commitear).

### Limpieza

- Eliminado: `docs/AUTOMATION_RULES.md` (sustituido por Manifiesto + Leyes Operativas).


