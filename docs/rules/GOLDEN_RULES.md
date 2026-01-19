# LEYES OPERATIVAS (GOLDEN RULES) — GesFer (S+)

Este archivo es la **constitución operativa única** del repositorio. Absorbe y unifica:

- `AI_GUIDELINES.md` (operativa IA/validación/UI/tests/commits)
- `DIAGNOSTICS.md` (reglas de resiliencia de seeds/VOs/tests/logging)
- `docs/AUTOMATION_RULES.md` (automatización: circuit breaker) — **ABSORBIDO**
- `TEKTON_MANIFEST.json` (rutas canónicas y política git) — **ABSORBIDO**

Si existe contradicción con cualquier otro documento, **este archivo prevalece**.

---

## 0) Blindaje técnico (obligatorio)

- **Entorno obligatorio**: Windows 11 / **PowerShell 7+**
- **Prohibido** (en operativa Windows y scripts): comandos Bash/Unix (`bash`, `ls`, `rm`, `grep`).
  - Alternativas permitidas: PowerShell (cmdlets nativos) y comandos `git`.
- **Rutas canónicas (S+)**:
  - Backend API: `Api/src/Api`
  - Frontend: `Cliente`
  - Tests: `Api/src/IntegrationTests`
  - Seeds demo: `Api/src/Infrastructure/Data/Seeds/demo-data.json`

---

## 1) Soberanía y Puerta de Entrada

- La **verdad absoluta** de comportamiento reside en:
  - `docs/MANIFESTO.md` (valores)
  - `docs/rules/GOLDEN_RULES.md` (leyes operativas)
- Cualquier “manual” o “guía” adicional debe considerarse **derivado**, nunca soberano.

---

## 2) Política Git (bloqueante)

- **Prohibido** hacer commits directos a `master`/`main`.
- El repositorio opera bajo política: `no_master_commit` (absorbe el contrato del manifiesto Tekton).

---

## 3) Documentación de Rama — Regla de Oro (bloqueante, S‑Grade)

- **Toda rama** debe tener su documento obligatorio en `docs/branches/`.
- Regla bloqueante:
  - **Antes de cualquier commit o validación de Juez**, debe existir y estar **no vacío**:
    - `docs/branches/<rama-actual>.md`

### Convención de nombre del archivo de rama

Debido a que los nombres de rama pueden contener `/`, el archivo se deriva así:

- `archivo-rama` = nombre de rama de Git con `/` reemplazado por `-`
- Ejemplo:
  - Rama: `feat/architectural-alignment-S-Plus`
  - Archivo: `docs/branches/feat-architectural-alignment-S-Plus.md`

---

## 4) Telemetría IA (bloqueante por infraestructura)

### Artefactos obligatorios

- Debe existir y estar no vacío:
  - `docs/performance/GLOBAL_IA_TRACKER.md`

### Template obligatorio

- Template oficial de reportes:
  - `docs/performance/templates/IA_PERF_REPORT.md`

### Reporte por rama (bloqueante en Juez)

- Debe existir y estar no vacío (nombre derivado de la rama):
  - `docs/performance/IA_PERF_<rama-actual>.md`

### Regla de cierre de rama

Al cerrar una rama se debe generar un informe en `docs/performance/` evaluando:

- **Acierto al primer disparo**: cuántas acciones salieron correctas en el primer intento.
- **Densidad de refactorización**: ratio de cambios estructurales vs cambios superficiales, y número de archivos tocados por unidad de objetivo.
- **Fugas de contexto**: cuántas contradicciones/reglas dispersas permanecen, y si aparecen “constituciones paralelas”.
- **Alineación con Manifiesto**: coherencia explícita con `docs/MANIFESTO.md` (Soberanía/Proactividad/Rigor).

---

## 5) Juez Modular (enforcement)

- El **Juez Modular** bloquea el flujo si falla cualquier regla S‑Grade.
- Bloqueos mínimos obligatorios:
  1. Documentación de rama ausente o vacía.
  2. Telemetría IA global ausente o vacía.

---

## 6) AC‑001 [LOGS] — Autocheck obligatorio (bloqueante de cierre)

- Antes de finalizar una tarea debe ejecutarse un **autocheck reproducible**.
- AC‑001 aplica especialmente a cambios que impacten logs/telemetría.

---

## 7) Compilación — No se entrega si no compila

- Ninguna tarea se considera entregada si el proyecto no compila.
- La compilación debe ser ejecutable localmente en el entorno oficial (Windows/PowerShell).

---

## 8) Automatización segura (Circuit Breaker — 3 Strikes) — ABSORBIDO

- Si una corrección automática falla **3 veces consecutivas con el mismo error**, el agente debe **detenerse**.
- Acción obligatoria: generar `AUDIT_FAIL.md` en la raíz con:
  - error, intentos, logs relevantes, y solicitud de intervención humana.

---

## 9) UI y Frontend (contrato)

### Componentes `shared/` (inmutables)

- Los componentes en `Cliente/components/shared/` son **puros e inmutables**.
- Variaciones solo vía **props** (composición, no modificaciones ad‑hoc).
- Prohibido usar HTML nativo si existe equivalente shared:
  - Prohibido: `<button>`, `<input>`, `<table>` cuando exista wrapper `shared/`.
  - Obligatorio: `Button`, `Input`, `DataTable`, `ModalBase`.

### `data-testid` (blindaje de tests)

- Nomenclatura obligatoria:
  - `shared-[componente]-[accion]`
- Tests y POMs deben usar `getByTestId()` / selectores por `data-testid`.
- Prohibido depender de estructura HTML/CSS (selectores tipo `button`, `.class`, `#id`, XPath).

### Kaizen (acción obligatoria)

- Cada intervención debe elevar la densidad de mejora.
- Acción prioritaria del baseline:
  - Implementar `DestructiveActionConfirm` y reemplazar usos de `confirm()` nativo.

---

## 10) Integridad Backend ↔ Frontend (contrato)

### Validación con Zod (frontend)

- Validaciones de formularios deben usar esquemas Zod en `Cliente/lib/validations/` reflejando restricciones del backend.

### Sincronización de tipos

- Antes de desarrollar/refactorizar: revisar entidades/DTOs en backend.
- Tipos TS deben reflejar exactamente backend (Guid → string, DateTime → ISO string, etc.).
- Si hay divergencia: **no proceder** hasta sincronizar.

---

## 11) Seeding resiliente + Value Objects (contrato)

### Seeds: validación pre‑contexto (obligatorio)

- Datos inválidos **nunca** llegan a BD.
- Validar **antes** de instanciar (ej. `Email.Create()`, `TaxId.Create()`).
- No usar try/catch alrededor de `SaveChanges` para “validar después”.
- Si un registro es inválido: se descarta, se loguea, y el proceso continúa.

### Seeds duales

- Seeds deben contener datos buenos y datos malos para validar cuarentena/resiliencia.

### Resiliencia en referencias

- Si una entidad referenciada fue rechazada por violación de dominio, las entidades dependientes se filtran/ignoran y se loguea; **no** se lanzan excepciones.

### Logging de seeding (formato obligatorio)

- Formato mínimo:
  - `[SEED] Violación de Dominio - ... Registro ignorado.`
  - `[SEED] <Entidad>: X ignorados por Violación de Dominio de Y totales`

---

## 12) Tests (contrato)

- Deben existir tests de integración que certifiquen:
  - datos inválidos no persisten,
  - el sistema sobrevive a datos corruptos parciales,
  - el logging reporta ignorados y resumen.

---

## 13) Commits y auditoría de PR (contrato)

- Antes de commit: ejecutar `scripts/validate-commit.ps1` (Windows).
- Antes de push/PR: ejecutar Juez Modular (`scripts/validate-pr.ps1`).
- Auditoría pre‑PR obligatoria en `docs/governance/audits/` con formato:
  - `YYYYMMDD_HHMM_[NOMBRE-RAMA]_CIERRE.md`


