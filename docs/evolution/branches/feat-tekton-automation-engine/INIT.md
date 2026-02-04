# INIT — Tekton Automation Engine (TAE)

Ámbito: **[INFRA / TEKTON]**  
Foco: **`/Tekton/Tools/`** (herramientas) + **`/Tekton/Configuration/`** (orquestación)

Este documento formaliza el **contrato CLI** y la estrategia de operación de TAE para automatizar tareas repetitivas de Git/infra de forma **segura** y compatible con una futura migración a **CLI en C#**.

---

## 1) Objetivo

Estandarizar dos herramientas motorizadas:

- `Tekton/Tools/Start-Task.ps1`: iniciar una tarea de forma segura (rama + artefactos S‑Grade + plan por hash).
- `Tekton/Tools/Close-Task.ps1`: cerrar una tarea de forma segura (validación + preparación PR + limpieza post‑merge).

**Regla de Oro de Seguridad (TAE)**: por defecto, **no ejecutar cambios** sin aprobación explícita por **hash**.

---

## 2) Dependencias y precondiciones (check obligatorio)

Antes de cualquier acción que modifique estado, los scripts deben validar:

- **Git**:
  - `git` disponible en `PATH`.
  - repositorio válido (p. ej. `git rev-parse --is-inside-work-tree`).
- **.NET**:
  - `dotnet` disponible en `PATH` (para integrarse con validaciones existentes y futuras).
- **Acceso a carpetas**:
  - lectura/escritura en `docs/branches/` y `docs/performance/`.
  - lectura en `Tekton/Configuration/` y `Tekton/Rules/`.
- **Entorno**:
  - compatible con Windows.
  - si se exige PowerShell 7+, reportar claramente si solo existe `powershell.exe`.

Si falta una dependencia o una precondición (por ejemplo, *working tree dirty*), el script debe:

- devolver **exit code** estandarizado (ver sección 6),
- emitir resultado estructurado (ver sección 5),
- incluir `suggestedNextStep`.

---

## 3) Contrato CLI (final)

### 3.1 Convenciones generales

- Interfaz **CLI explícita**: sin prompts interactivos salvo flags de control (`-ApproveHash`, `-Force`).
- **Idempotencia**: repetir el comando no debe corromper estado; si el estado ya existe, debe reusar o fallar de forma declarativa.
- **Salida determinista**:
  - `-OutputFormat Text|Json` (default `Text`).
  - En `Json`, escribir un único objeto JSON por stdout (sin banners).
- **Control por Hash**:
  - `-PlanOnly` default **`$true`**: solo planifica, no ejecuta.
  - para ejecutar: `-PlanOnly:$false -ApproveHash <sha256>` (el hash debe coincidir exactamente con el plan emitido).

### 3.2 `Start-Task.ps1` — contrato de parámetros

**Requeridos**

- `-Name <string>`: nombre humano de la tarea (para docs/reportes).
- `-Scope <Api|Cliente|Infra|Cross|Tekton>`: ámbito declarado.
- `-Type <Sencilla|Normal|Compleja>`: tipo operativo.

**Opcionales (branching y git)**

- `-BranchPrefix <string>`: default derivado del tipo (`feat` para Normal/Compleja; `fix` si se usa explícitamente).
- `-Branch <string>`: si se indica, se valida y se utiliza tal cual.
- `-BaseBranch <string>`: default `master`.
- `-Remote <string>`: default `origin`.
- `-FailIfDirty` (default `true`): aborta si hay cambios sin commit.
- `-ReuseIfExists` (default `false`): si la rama existe, reusar en vez de fallar.
- `-NoFetch` / `-NoPrune`: desactiva `fetch`/`remote prune` (solo para entornos restringidos).

**Opcionales (artefactos S‑Grade)**

- `-EnsureBranchDocs` (default `true`): asegurar `docs/branches/<slug>.md` no vacío.
- `-EnsureIATelemetry` (default `true`): asegurar `docs/performance/IA_PERF_<slug>.md` no vacío.
- `-EnsureGlobalTracker` (default `true`): asegurar `docs/performance/GLOBAL_IA_TRACKER.md` no vacío (si falta: planificar creación; no ejecutar sin hash).
- `-Template <path>`: plantilla para inicializar reportes (default `Tekton/Templates/IA_PERF_REPORT.md`).

**Control por hash / salida**

- `-PlanOnly` (default `true`)
- `-ApproveHash <sha256>` (requerido para ejecución real)
- `-OutputFormat Text|Json` (default `Text`)
- `-OutputPath <path>` (opcional; volcar JSON a disco además de stdout)

### 3.3 `Close-Task.ps1` — contrato de parámetros

**Requeridos**

- `-Name <string>`
- `-Scope <Api|Cliente|Infra|Cross|Tekton>`
- `-Type <Sencilla|Normal|Compleja>`

**Opcionales (modo de cierre)**

- `-Mode <Prepare|Cleanup|All>`:
  - `Prepare` (default): validar + preparar PR + evidencias (sin merge directo a `master`).
  - `Cleanup`: limpieza post‑merge (borrar rama local, prune; opcional borrar remota).
  - `All`: `Prepare` + `Cleanup` (solo si detecta rama mergeada).
- `-RequireMerged` (default `false`): en `Cleanup`, exigir que la rama esté mergeada en `master`.

**Opcionales (validación)**

- `-RunValidateCommit` (default `true`): ejecutar `scripts/validate-commit.ps1`.
- `-RunValidatePr` (default `true`): ejecutar `scripts/validate-pr.ps1`.
- `-Autocheck` (default `true`): AC‑001 [LOGS] (según plan).

**Opcionales (git/push/cleanup)**

- `-Push` (default `true`)
- `-DeleteLocalBranch` (default `true`)
- `-DeleteRemoteBranch` (default `false`) (solo en `Cleanup` y si procede)
- `-BaseBranch <string>` (default `master`)
- `-Remote <string>` (default `origin`)

**Control por hash / salida**

- `-PlanOnly` (default `true`)
- `-ApproveHash <sha256>`
- `-OutputFormat Text|Json` (default `Text`)
- `-OutputPath <path>`

---

## 4) Flujo de trabajo estandarizado

### 4.1 Inicio (Start‑Task)

- Validar dependencias.
- Validar estado Git:
  - abortar si `-FailIfDirty` y hay cambios sin commit.
  - asegurar base (`fetch`, `prune`) salvo que se deshabilite.
- Calcular `branchSlug` (reemplazar `/` y `\` por `-`).
- Planificar y asegurar artefactos S‑Grade (doc de rama, telemetría IA).
- Emitir plan + `planHash`.
- Ejecutar únicamente si `-ApproveHash` coincide.

### 4.2 Cierre (Close‑Task)

`Prepare`:

- Validar dependencias.
- Ejecutar validaciones existentes (Juez / pre‑commit / autocheck).
- Preparar evidencias de cierre (auditoría y telemetría de cierre).
- Push a remoto si aplica.
- **No** merge directo a `master` por política `no_master_commit`.

`Cleanup`:

- Verificar si la rama está mergeada (si `-RequireMerged`).
- Borrar rama local (y remota si está permitido).
- Ejecutar `git remote prune origin`.
- Confirmar estado final esperado (solo `master`, limpio, up‑to‑date).

---

## 5) Salida JSON (heartbeat) + `suggestedNextStep`

Cuando `-OutputFormat Json`, el script debe emitir un único objeto JSON con:

- `engine`, `tool`, `version`
- `ok`, `exitCode`
- `name`, `scope`, `type`
- `branchName`, `branchSlug`
- `planOnly`, `planHash`, `approvedHash`
- `plannedOps[]` (operaciones planeadas)
- `artifacts` (paths relevantes)
- `errors[]` / `warnings[]` (con `category`, `code`, `message`, `remediation`)
- **`suggestedNextStep`** (string): guía accionable para continuar tras error o éxito.

Ejemplos de `suggestedNextStep` (no exhaustivo):

- `"Ejecuta: git status; resuelve cambios sin commit y reintenta Start-Task."`
- `"Ejecuta: scripts/validate-pr.ps1; corrige fallos y reintenta Close-Task -Mode Prepare."`
- `"Confirma merge en PR y ejecuta Close-Task -Mode Cleanup."`

---

## 6) Códigos de salida (mapeo completo)

- **0**: éxito
- **10**: argumentos inválidos / contrato violado
- **11**: precondición no cumplida (dirty tree, repo inválido, dependencia ausente, acceso denegado a carpetas)
- **20**: fallo Git genérico (no clasificado)
- **21**: conflicto Git (merge/rebase/conflicts detectados)
- **22**: permisos/auth Git (push/delete/remote denied)
- **30**: validación/juez falló (`validate-commit`/`validate-pr`/AC‑001)
- **40**: error de I/O (lectura/escritura de archivos, paths inválidos)
- **50**: error inesperado (excepción no clasificada)

---

## 7) Compatibilidad con futura migración a C# CLI

El contrato está diseñado para mapear a un CLI tipo:

- `tae task start --name --scope --type --plan-only --approve-hash --output json`
- `tae task close --name --scope --type --mode --plan-only --approve-hash --output json`

Con JSON de salida estable y códigos de salida equivalentes.

