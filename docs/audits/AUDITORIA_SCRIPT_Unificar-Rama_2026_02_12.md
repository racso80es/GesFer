# Auditoría: Script `scripts/skills/Unificar-Rama.ps1`

**Fecha:** 2026-02-12  
**Agentes:** Security Engineer + Process & Interaction Auditor  
**Referencias:** `openspecs/agents/security-engineer.json`, `openspecs/agents/auditor/process-interaction.json`, AGENTS.md

---

## 1. Resumen ejecutivo

El script `Unificar-Rama.ps1` implementa un flujo de “certificación Tekton” (compilación, validación de documentación, commit). **No cumple** las restricciones del agente de Seguridad ni del Auditor de Procesos: permite commits sin token, sin protección de rama `master` y sin registro en el log de auditoría. Se recomienda **no usarlo en su estado actual** para operaciones Git hasta aplicar las correcciones indicadas.

---

## 2. Hallazgos por agente

### 2.1 Security Engineer

| ID | Severidad | Regla / Ley | Hallazgo |
|----|-----------|-------------|----------|
| S1 | **CRITICAL** | Vision Zero (destructive actions) | El script ejecuta `git add .` y `git commit` sin **confirmación explícita** del usuario. Una acción que modifica el historial Git debe requerir confirmación textual antes de ejecutarse. |
| S2 | **CRITICAL** | Ley 3 GIT (AGENTS.md) | **No se comprueba** que la rama actual sea distinta de `master`. Es posible hacer commit en `master` por error, violando "NO commits a master". |
| S3 | **HIGH** | Input validation | `$BranchName` y `$CommitMessage` son parámetros no validados. `$CommitMessage` se usa directamente en `git commit -m "..."`, lo que permite **inyección de argumentos** (p. ej. comillas o backticks que alteren el comando). |
| S4 | **MEDIUM** | Trazabilidad | No se registra la ejecución (éxito/fallo) en `docs/audits/ACCESS_LOG.md`, por lo que no hay evidencia auditable del uso del script. |

### 2.2 Process & Interaction Auditor

| ID | Severidad | Regla / Constraint | Hallazgo |
|----|-----------|--------------------|----------|
| P1 | **CRITICAL** | Zero Trust: no Git sin Hash/Token | El script **no valida** el Token de Interacción antes de ejecutar `git add`/`git commit`. Según `process-interaction.json`, toda interacción Git debe estar protegida por token; este script la elude. |
| P2 | **HIGH** | Mandatory Validation (commits → Unit Tests) | No se ejecutan **tests unitarios** antes del commit. El estándar exige que un commit dispare la ejecución de Unit Tests (como en `commit-skill.sh`). |
| P3 | **HIGH** | Log all interactions | No se escribe ninguna entrada en `docs/audits/ACCESS_LOG.md` (Success, Failure, Bypass). El auditor exige registrar todas las interacciones. |
| P4 | **MEDIUM** | Intercept Git via scripts/skills | El script vive en `scripts/skills/` pero no reutiliza la cadena Token → Tests → Log que definen `commit-skill.sh` y `pr-skill.sh`, generando un canal alternativo sin las mismas garantías. |

---

## 3. Coherencia con otros skills

- **commit-skill.sh** y **pr-skill.sh**:
  - Validan token con `scripts/auditor/process-token-manager.sh Validate`.
  - Ejecutan tests (Unit en commit, todos en PR).
  - Escriben en `docs/audits/ACCESS_LOG.md`.

- **Unificar-Rama.ps1**:
  - No llama a `scripts/auditor/process-token-manager.ps1` (Validate).
  - No ejecuta tests.
  - No escribe en ACCESS_LOG.

Unificar-Rama.ps1 queda como **bypass** del protocolo de proceso e incumple el mismo estándar que los demás skills.

---

## 4. Recomendaciones (priorizadas)

1. **Bloquear commit en `master`**  
   Antes de cualquier `git add`/`git commit`, comprobar la rama actual y salir con error si es `master`.

2. **Validar Token de Interacción**  
   Invocar `scripts/auditor/process-token-manager.ps1 Validate` (o su lógica equivalente en PowerShell). Si el token es inválido o no existe, bloquear y no ejecutar Git.

3. **Confirmación explícita (Vision Zero)**  
   Pedir confirmación textual (p. ej. "Escriba SÍ para proceder con el commit") antes de ejecutar `git add` y `git commit`.

4. **Sanitizar/validar inputs**  
   - Validar que `$BranchName` coincida con la rama actual o sea una rama permitida.  
   - Sanitizar `$CommitMessage` (longitud máxima, caracteres prohibidos como comillas dobles sin escapar, backticks, newlines no controlados) antes de pasarlo a `git commit -m`.

5. **Ejecutar Unit Tests antes de commit**  
   Ejecutar los mismos proyectos *UnitTests que usa `commit-skill.sh`; en caso de fallo, no hacer commit.

6. **Registrar en ACCESS_LOG.md**  
   Añadir una fila por ejecución con: timestamp, usuario Git, rama, acción (ej. "UNIFICAR_RAMA"), estado (Success/Failure/Blocked), detalle (mensaje o motivo de bloqueo).

7. **Opcional:** Descomentar `git push` solo tras cumplir los puntos anteriores y, si se desea alinear con PR, exigir también validación tipo PR (token + suite completa) antes de push.

---

## 5. Conclusión

Desde **Seguridad** y **Auditor de Procesos**, el script **Unificar-Rama.ps1** no debe usarse para realizar commits hasta que se implementen: protección de `master`, validación de token, confirmación explícita, validación/sanitización de parámetros, ejecución de tests previos al commit y registro en `docs/audits/ACCESS_LOG.md`. Con estos cambios, el script quedará alineado con las Leyes Universales (AGENTS.md), con el Security Engineer y con el Process & Interaction Auditor.
