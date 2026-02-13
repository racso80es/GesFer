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
    # Clean the branch name to remove dynamic suffixes (e.g., -123456789)
    # The pattern matches a dash followed by a sequence of digits at the end of the string
    clean_branch=$(echo "$BRANCH" | sed -E 's/-[0-9]+$//')
    slug=$(echo "$clean_branch" | sed 's/[\/\\]/-/g')

    passport="docs/branches/${slug}.md"
    objective_doc="docs/branches/${slug}/OBJETIVO.md"

    # Check for documentation with cleaned slug
    if [ -f "$passport" ] || [ -f "$objective_doc" ]; then
        echo "Documentación de rama encontrada ($passport o $objective_doc)."
    else
        # Fallback: check original slug just in case the numbers were intentional
        original_slug=$(echo "$BRANCH" | sed 's/[\/\\]/-/g')
        original_passport="docs/branches/${original_slug}.md"
        original_objective="docs/branches/${original_slug}/OBJETIVO.md"

        if [ -f "$original_passport" ] || [ -f "$original_objective" ]; then
             echo "Documentación de rama encontrada ($original_passport o $original_objective)."
    # Limpiar sufijos numéricos largos (e.g. timestamps de CI)
    slug_base=$(echo "$BRANCH" | sed 's/[\/\\]/-/g')
    slug_cleaned=$(echo "$slug_base" | sed -E 's/-[0-9]{10,}$//')

    passport_base="docs/branches/${slug_base}.md"
    objective_base="docs/branches/${slug_base}/OBJETIVO.md"
    passport_cleaned="docs/branches/${slug_cleaned}.md"
    objective_cleaned="docs/branches/${slug_cleaned}/OBJETIVO.md"

    if [ -f "$passport_base" ] || [ -f "$objective_base" ]; then
        echo "Documentación de rama encontrada ($passport_base o $objective_base)."
    elif [ -f "$passport_cleaned" ] || [ -f "$objective_cleaned" ]; then
        echo "Documentación de rama encontrada (base limpia: $passport_cleaned o $objective_cleaned)."
    else
        # Intento de fallback: si el slug termina en dígitos (sufijo CI), probar sin ellos
        base_slug=$(echo "$slug" | sed -E 's/-[0-9]+$//')
        base_passport="docs/branches/${base_slug}.md"
        base_objective="docs/branches/${base_slug}/OBJETIVO.md"

        if [ "$base_slug" != "$slug" ] && { [ -f "$base_passport" ] || [ -f "$base_objective" ]; }; then
            echo "Documentación de rama encontrada (fallback CI suffix): $base_passport o $base_objective."
        else
            echo "ERROR: No se encuentra documentación de rama. Esperado: $passport o $objective_doc"
            log_entry "BLOCKED" "Documentación de rama ausente ($slug)"
            exit 1
        fi
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
