#!/bin/bash
# PR Skill
# Trigger: pre-push (local) o GitHub Actions (CI)
# Actions: [CI skip Token] Compilation Shield -> Branch Doc Check -> All Tests (Integration, E2E, Unit)
# Mejoras integradas desde Unificar-Rama.ps1: escudo de compilación, certificación de documentación de rama.

set -e
LOG_FILE="docs/audits/ACCESS_LOG.md"
TIMESTAMP=$(date "+%Y-%m-%d %H:%M:%S")

# Detección de entorno: CI (GitHub Actions) vs local
if [ -n "$GITHUB_ACTIONS" ] && [ "$GITHUB_ACTIONS" = "true" ]; then
    CI_MODE=1
    USER_NAME="github-actions"
    # En PR: rama de origen; en push: rama actual
    BRANCH="${GITHUB_HEAD_REF:-${GITHUB_REF#refs/heads/}}"
else
    CI_MODE=0
    USER_NAME=$(git config user.name 2>/dev/null || echo "local")
    BRANCH=$(git branch --show-current 2>/dev/null || echo "detached")
fi

log_entry() {
    local status="$1"
    local message="$2"
    echo "| $TIMESTAMP | $USER_NAME | $BRANCH | PUSH/PR | $status | $message |" >> "$LOG_FILE"
}

# Asegurar que el log existe
if [ ! -f "$LOG_FILE" ]; then
    mkdir -p docs/audits
    echo "| Timestamp | User | Branch | Action | Status | Details |" > "$LOG_FILE"
    echo "|---|---|---|---|---|---|" >> "$LOG_FILE"
fi

# --- Bypass (solo local) ---
if [ "$CI_MODE" -eq 0 ] && [ "$BYPASS_AUDIT" = "1" ]; then
    echo "⚠ BYPASS DETECTADO: Ejecutando validación de seguridad..."
    bypass_ok=0
    ./scripts/skills/security-validation-skill.sh "BYPASS_TOKEN" "PUSH_BYPASS" || bypass_ok=$?
    if [ "${bypass_ok}" -eq 0 ]; then
        log_entry "WARNING" "Bypass ejecutado exitosamente via variable de entorno"
        exit 0
    else
        log_entry "BLOCKED" "Fallo en validación de seguridad del Bypass"
        exit 1
    fi
fi

# --- Token de proceso (solo local; en CI no hay token) ---
if [ "$CI_MODE" -eq 0 ]; then
    echo "🔒 [AUDITOR] Validando Token de Proceso..."
    if ! ./scripts/auditor/process-token-manager.sh Validate; then
        log_entry "BLOCKED" "Token inválido o expirado"
        echo "❌ Token inválido. Ejecute 'scripts/auditor/process-token-manager.sh Generate'"
        exit 1
    fi
else
    echo "🔓 [CI] Ejecución en GitHub Actions; validación de token omitida."
fi

# --- [COMPILATION SHIELD] (desde Unificar-Rama.ps1) ---
echo "--- Escudo de Compilación (máx. 7 intentos) ---"
RETRY_MAX=7
RETRY_DELAY=2
attempt=1
build_ok=0

while [ $attempt -le "$RETRY_MAX" ]; do
    echo "Intento de compilación #$attempt..."
    if dotnet build -nologo -v q; then
        build_ok=1
        echo "Compilación exitosa."
        break
    fi
    if [ $attempt -eq "$RETRY_MAX" ]; then
        echo "CRITICAL: Fallo de compilación persistente tras $RETRY_MAX intentos."
        DIAG_DIR="docs/diagnostics/${BRANCH//\//-}"
        mkdir -p "$DIAG_DIR"
        dotnet build -nologo > "$DIAG_DIR/build_error_final.log" 2>&1 || true
        log_entry "FAILED" "Fallo compilación persistente; ver $DIAG_DIR/build_error_final.log"
        exit 1
    fi
    echo "Reintentando en ${RETRY_DELAY}s..."
    sleep "$RETRY_DELAY"
    attempt=$((attempt + 1))
done

# --- [CERTIFICACIÓN DOCUMENTACIÓN DE RAMA] (desde Unificar-Rama.ps1 / process-token-manager) ---
# Regla de oro: ramas (salvo master/main) deben tener documentación
if [ "$BRANCH" != "master" ] && [ "$BRANCH" != "main" ] && [ -n "$BRANCH" ]; then
    # Strategy: Check both exact slug and slug without trailing numeric suffix (for CI branches)

    # 1. Exact Slug
    slug=$(echo "$BRANCH" | sed 's/[\/\\]/-/g')
    passport="docs/branches/${slug}.md"
    objective_doc="docs/branches/${slug}/OBJETIVO.md"

    # 2. Cleaned Slug (remove trailing -numbers)
    # Matches a dash followed by one or more digits at the end of the string
    clean_slug=$(echo "$slug" | sed -E 's/-[0-9]+$//')
    clean_passport="docs/branches/${clean_slug}.md"
    clean_objective_doc="docs/branches/${clean_slug}/OBJETIVO.md"

    if [ -f "$passport" ] || [ -f "$objective_doc" ]; then
        echo "Documentación de rama encontrada (Exacta): $passport o $objective_doc"
    elif [ -f "$clean_passport" ] || [ -f "$clean_objective_doc" ]; then
        echo "Documentación de rama encontrada (Limpia): $clean_passport o $clean_objective_doc"
    else
        echo "ERROR: No se encuentra documentación de rama."
        echo "Esperado (Exacto): $passport o $objective_doc"
        echo "Esperado (Limpio): $clean_passport o $clean_objective_doc"
        log_entry "BLOCKED" "Documentación de rama ausente ($slug)"
        exit 1
    fi
fi

# --- Suite completa de tests ---
echo "🧪 [SKILL] Ejecutando SUITE COMPLETA de Tests..."
if dotnet run --project src/Console/GesFer.Console.csproj --no-build -- 11; then
    log_entry "SUCCESS" "Suite Completa validada"
    echo "✅ PR Skill Verificado."
    exit 0
else
    log_entry "FAILED" "Fallo en Suite Completa de Tests"
    echo "❌ Tests fallidos. Push/PR rechazado."
    exit 1
fi
