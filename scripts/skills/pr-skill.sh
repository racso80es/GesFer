#!/bin/bash
# PR Skill
# Trigger: pre-push (Simulando PR Check local)
# Actions: Token Validation -> All Tests (Integration, E2E, Unit)

LOG_FILE="docs/audits/ACCESS_LOG.md"
TIMESTAMP=$(date "+%Y-%m-%d %H:%M:%S")
USER_NAME=$(git config user.name)
BRANCH=$(git branch --show-current)

log_entry() {
    local status="$1"
    local message="$2"
    echo "| $TIMESTAMP | $USER_NAME | $BRANCH | PUSH/PR | $status | $message |" >> "$LOG_FILE"
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
    ./scripts/skills/security-validation-skill.sh "BYPASS_TOKEN" "PUSH_BYPASS"

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

echo "🧪 [SKILL] Ejecutando SUITE COMPLETA de Tests..."

# Use Console App argument 11 (Tests)
# Assuming non-interactive mode via arguments is supported (as verified in Program.cs)
dotnet run --project src/Console/GesFer.Console.csproj -- 11

if [ $? -eq 0 ]; then
    log_entry "SUCCESS" "Suite Completa validada"
    echo "✅ PR Skill Verificado."
    exit 0
else
    log_entry "FAILED" "Fallo en Suite Completa de Tests"
    echo "❌ Tests fallidos. Push rechazado."
    exit 1
fi
