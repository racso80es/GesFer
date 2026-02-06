#!/bin/bash
# Commit Skill
# Trigger: pre-commit
# Actions: Token Validation -> Unit Tests

LOG_FILE="docs/audits/ACCESS_LOG.md"
TIMESTAMP=$(date "+%Y-%m-%d %H:%M:%S")
USER_NAME=$(git config user.name)
BRANCH=$(git branch --show-current)

log_entry() {
    local status="$1"
    local message="$2"
    echo "| $TIMESTAMP | $USER_NAME | $BRANCH | COMMIT | $status | $message |" >> "$LOG_FILE"
}

# Ensure log file exists with header
if [ ! -f "$LOG_FILE" ]; then
    mkdir -p docs/audits
    echo "| Timestamp | User | Branch | Action | Status | Details |" > "$LOG_FILE"
    echo "|---|---|---|---|---|---|" >> "$LOG_FILE"
fi

# Bypass Logic
if [ "$BYPASS_AUDIT" == "1" ]; then
    echo "⚠ BYPASS DETECTADO: Ejecutando validación de seguridad..."
    ./scripts/skills/security-validation-skill.sh "BYPASS_TOKEN" "COMMIT_BYPASS"

    if [ $? -eq 0 ]; then
        log_entry "WARNING" "Bypass ejecutado exitosamente via variable de entorno"
        exit 0
    else
        log_entry "BLOCKED" "Fallo en validación de seguridad del Bypass"
        exit 1
    fi
fi

# Normal Flow
echo "🔒 [AUDITOR] Validando Token de Proceso..."
./scripts/auditor/process-token-manager.sh Validate

if [ $? -ne 0 ]; then
    log_entry "BLOCKED" "Token inválido o expirado"
    echo "❌ Token inválido. Ejecute 'scripts/auditor/process-token-manager.sh Generate'"
    exit 1
fi

echo "🧪 [SKILL] Ejecutando Tests Unitarios..."

# Find all UnitTests projects
TEST_PROJECTS=$(find src -name "*UnitTests.csproj")
FAILED=0

for proj in $TEST_PROJECTS; do
    echo "   > Testeando: $(basename $proj)"
    dotnet test "$proj" --nologo --verbosity quiet
    if [ $? -ne 0 ]; then
        FAILED=1
    fi
done

if [ $FAILED -eq 0 ]; then
    log_entry "SUCCESS" "Tests Unitarios completados"
    echo "✅ Commit Skill Verificado."
    exit 0
else
    log_entry "FAILED" "Fallo en Tests Unitarios"
    echo "❌ Tests Unitarios fallidos. Commit rechazado."
    exit 1
fi
