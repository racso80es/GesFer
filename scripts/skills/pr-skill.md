# PR Skill – Documentación

> **Herramienta:** `pr-skill.sh`  
> **Ubicación:** `scripts/skills/pr-skill.sh`  
> **Entorno:** Bash (Linux/macOS/Git Bash en Windows). No existe versión PowerShell (`pr-skill.ps1`); el flujo oficial es el script Bash.

---

## 1. Propósito

El **PR Skill** es la barrera de calidad y seguridad antes de cada **push** (local) y en cada **Pull Request** (CI). Garantiza:

1. **Token de proceso** válido (solo local; en CI se omite).
2. **Compilación** correcta del proyecto (.NET), con reintentos.
3. **Documentación de rama** presente para ramas que no sean `master`/`main`.
4. **Suite completa de tests** (opción 11 de la Consola) ejecutada con éxito.

Si cualquier paso falla, el push/PR se **bloquea** y se registra en el log de auditoría.

---

## 2. Dónde se ejecuta

| Contexto | Disparador | Archivo |
|----------|------------|---------|
| **Local (pre-push)** | Hook Husky `pre-push` | `.husky/pre-push` → invoca `scripts/skills/pr-skill.sh` |
| **CI (GitHub Actions)** | Push/PR a `master` o `main` | `.github/workflows/pr-skill.yml` → ejecuta `pr-skill.sh` |

La ejecución debe hacerse **siempre desde la raíz del repositorio** (pre-push y workflow hacen checkout en la raíz).

---

## 3. Flujo (orden de pasos)

```
1. Detección de entorno (CI vs local)
2. [Solo local] Bypass opcional (BY PASS_AUDIT=1 + security-validation-skill.sh)
3. [Solo local] Validación de Token de Proceso (process-token-manager.sh Validate)
4. Escudo de compilación (dotnet build, máx. 7 intentos)
5. Certificación documentación de rama (obligatoria si rama ≠ master/main)
6. Suite completa de tests (dotnet run ... -- 11)
7. Registro en docs/audits/ACCESS_LOG.md y salida (éxito/fallo)
```

---

## 4. Dependencias

### 4.1 Ejecutables / entorno

- **Bash** (script con shebang `#!/bin/bash`).
- **`dotnet`** (SDK .NET 8.0) en PATH.
- **`git`** para nombre de rama y usuario (local).

### 4.2 Scripts del repositorio

| Script | Uso |
|--------|-----|
| `scripts/auditor/process-token-manager.sh` | Comandos `Validate` y `Generate` para el token de proceso (solo local). |
| `scripts/skills/security-validation-skill.sh` | Validación del bypass con `BYPASS_AUDIT=1` (solo local). |

### 4.3 Proyecto .NET

- **Consola:** `src/Console/GesFer.Console.csproj`
- El script ejecuta:  
  `dotnet run --project src/Console/GesFer.Console.csproj --no-build -- 11`  
  El argumento **`11`** corresponde a la opción del menú “Ejecutar tests” (suite completa).

### 4.4 Estructura de documentación

- **Log de acceso:** `docs/audits/ACCESS_LOG.md` (se crea si no existe).
- **Documentación de rama:** para rama distinta de `master`/`main` debe existir al menos uno de:
  - `docs/branches/<slug>.md`
  - `docs/branches/<slug>/OBJETIVO.md`  
  donde `<slug>` es el nombre de la rama con `/` y `\` sustituidos por `-`.  
  También se admite un “slug limpio” (sin sufijo numérico final) para ramas de CI.

### 4.5 Variables de entorno

| Variable | Efecto |
|----------|--------|
| `GITHUB_ACTIONS=true` | Modo CI: se omite token y se usa rama del evento (HEAD_REF o REF). |
| `BYPASS_AUDIT=1` | Solo local: intenta bypass vía `security-validation-skill.sh`; si falla, se bloquea. |

---

## 5. Salidas y códigos de retorno

- **0:** Todas las comprobaciones pasaron; push/PR permitido.
- **1:** Fallo en token, compilación, documentación de rama o tests; push/PR bloqueado.

Cada ejecución (local o CI) se registra en `docs/audits/ACCESS_LOG.md` con timestamp, usuario, rama, acción (PUSH/PR) y estado (SUCCESS, FAILED, BLOCKED, WARNING).

---

## 6. Escudo de compilación

- Se ejecuta **`dotnet build -nologo -v q`** hasta **7 veces** con 2 segundos entre intentos.
- Si tras 7 intentos sigue fallando:
  - Se escribe un log en `docs/diagnostics/<rama>/build_error_final.log`.
  - Se registra FAILED en `ACCESS_LOG.md` y el script sale con código 1.

---

## 7. Documentación de rama (certificación)

- **Ramas exentas:** `master`, `main`, y rama vacía/detached.
- **Resto de ramas:** debe existir documentación por **slug exacto** o por **slug limpio** (sin sufijo numérico final):
  - Ejemplo slug exacto: `feature-mi-rama` → `docs/branches/feature-mi-rama.md` o `docs/branches/feature-mi-rama/OBJETIVO.md`.
  - Ejemplo slug limpio: rama `jules-12345-abc` puede usar `docs/branches/jules-12345.md` o equivalente sin el sufijo numérico final, según la lógica del script.

Si no se encuentra ningún documento válido, el script imprime las rutas esperadas, registra BLOCKED en el log y sale con 1.

---

## 8. Uso en Windows

El script es **Bash**; no hay `pr-skill.ps1`. En Windows:

- **Pre-push (Husky):** el hook llama a `bash scripts/skills/pr-skill.sh`, por lo que hace falta **Bash** (p. ej. **Git for Windows** / Git Bash).
- **CI:** se ejecuta en `ubuntu-latest`, sin cambios.

Para desarrollo local en Windows, mantener Git Bash instalado y asegurarse de que el hook tenga permisos de ejecución si el entorno lo requiere.

---

## 9. Resumen de validación (análisis del script)

| Aspecto | Estado |
|---------|--------|
| Ruta del script | Correcta: `scripts/skills/pr-skill.sh` (no existe `pr-skill.ps1`). |
| Dependencias (process-token-manager, security-validation-skill) | Presentes y utilizadas según modo local/CI. |
| Compilación | Reintentos y log de diagnóstico coherentes. |
| Documentación de rama | Comprueba slug exacto y slug limpio; coherente con `docs/branches/`. |
| Suite de tests | Invoca correctamente la Consola con argumento `11`. |
| Log de auditoría | Crea/actualiza `docs/audits/ACCESS_LOG.md` con formato esperado. |
| CI (pr-skill.yml) | Ejecuta el script desde la raíz con .NET 8.0; no valida token. |

El script está alineado con el flujo definido (Token → Compilación → Doc rama → Tests) y con las referencias en `openspecs`, documentación de ramas y auditorías.
