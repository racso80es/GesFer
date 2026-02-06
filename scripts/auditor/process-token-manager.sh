#!/bin/bash
# Auditor de Procesos - Gestor de Tokens (Bash Port)

COMMAND="${1:-Validate}"
TOKEN_PATH=".git/INTERACTION_TOKEN"
SALT="GESFER_AUDIT_SALT_v1"

get_branch_name() {
    git branch --show-current | xargs
}

calculate_hash() {
    local input="$1"
    # echo -n "$input$SALT" | sha256sum | awk '{print $1}'
    if command -v sha256sum >/dev/null; then
        echo -n "$input$SALT" | sha256sum | awk '{print $1}'
    else
        # Fallback for systems without sha256sum (e.g. some macs, use shasum -a 256)
        echo -n "$input$SALT" | shasum -a 256 | awk '{print $1}'
    fi
}

assert_passport() {
    local branch="$1"
    if [ "$branch" = "master" ] || [ "$branch" = "main" ]; then
        return 0
    fi

    local slug=$(echo "$branch" | sed 's/[\/\\]/-/g')
    local doc_path="docs/branches/${slug}.md"

    if [ ! -f "$doc_path" ]; then
        echo -e "\033[0;31mBLOQUEO DE AUDITOR: Pasaporte de rama no encontrado.\033[0m"
        echo -e "\033[0;33mRuta esperada: $doc_path\033[0m"
        return 1
    fi

    if [ ! -s "$doc_path" ]; then
        echo -e "\033[0;31mBLOQUEO DE AUDITOR: Pasaporte de rama vacío.\033[0m"
        return 1
    fi
    return 0
}

generate_token() {
    echo -e "\033[0;36m[AUDITOR] Solicitando Token de Interacción...\033[0m"
    local branch=$(get_branch_name)

    if [ -z "$branch" ]; then
        echo "Error: No hay rama activa."
        exit 1
    fi

    if ! assert_passport "$branch"; then
        echo "TOKEN DENEGADO."
        exit 1
    fi

    local today=$(date +%Y-%m-%d)
    local raw="${branch}|${today}"
    local hash=$(calculate_hash "$raw")

    mkdir -p .git
    echo "$hash" > "$TOKEN_PATH"

    echo -e "\033[0;32mToken generado: $hash\033[0m"
    echo "Contexto: $branch | $today"
}

validate_token() {
    local branch=$(get_branch_name)
    local today=$(date +%Y-%m-%d)
    local raw="${branch}|${today}"
    local expected_hash=$(calculate_hash "$raw")

    if [ ! -f "$TOKEN_PATH" ]; then
        echo -e "\033[0;33m[AUDITOR] Token no encontrado.\033[0m"
        return 1
    fi

    local stored_hash=$(cat "$TOKEN_PATH" | xargs)

    if [ "$stored_hash" = "$expected_hash" ]; then
        return 0
    else
        echo -e "\033[0;31m[AUDITOR] Token expirado o inválido.\033[0m"
        return 1
    fi
}

if [ "$COMMAND" = "Generate" ]; then
    generate_token
elif [ "$COMMAND" = "Validate" ]; then
    if validate_token; then
        exit 0
    fi
    echo "[AUDITOR] Intentando generar nuevo token..."
    generate_token
    if validate_token; then
        exit 0
    else
        echo -e "\033[0;31mFATAL: No se pudo establecer un token válido.\033[0m"
        exit 1
    fi
elif [ "$COMMAND" = "Revoke" ]; then
    rm -f "$TOKEN_PATH"
    echo "Token revocado."
else
    echo "Comando desconocido: $COMMAND"
    exit 1
fi
