# Auditor de Procesos - Gestor de Tokens de Interacción
# Gestiona la emisión y validación de hashes de sesión para interacciones técnicas (Git)

param(
    [string]$Command = "Validate", # Validate, Generate, Revoke
    [string]$Context = "Git"
)

$ErrorActionPreference = "Stop"

$TokenPath = ".git/INTERACTION_TOKEN"
$Salt = "GESFER_AUDIT_SALT_v1"

function Get-BranchName {
    $branch = (git branch --show-current).Trim()
    return $branch
}

function Calculate-Hash {
    param($InputString)
    $algo = [System.Security.Cryptography.SHA256]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($InputString + $Salt)
    $hash = $algo.ComputeHash($bytes)
    return [BitConverter]::ToString($hash).Replace("-", "").ToLower()
}

function Assert-Passport {
    param($BranchName)

    # Excepciones para ramas troncales
    if ($BranchName -eq "master" -or $BranchName -eq "main") {
        return $true
    }

    $slug = ($BranchName -replace "[/\\]", "-")
    $docPath = Join-Path -Path "docs\branches" -ChildPath ("{0}.md" -f $slug)

    if (-not (Test-Path $docPath)) {
        Write-Host "BLOQUEO DE AUDITOR: Pasaporte de rama no encontrado." -ForegroundColor Red
        Write-Host "Ruta esperada: $docPath" -ForegroundColor Yellow
        Write-Host "Acción requerida: Crea el documento de pasaporte para esta rama antes de continuar." -ForegroundColor White
        return $false
    }

    $content = Get-Content -Path $docPath -Raw -ErrorAction SilentlyContinue
    if ([string]::IsNullOrWhiteSpace($content)) {
        Write-Host "BLOQUEO DE AUDITOR: Pasaporte de rama vacío." -ForegroundColor Red
        return $false
    }

    return $true
}

function Generate-Token {
    Write-Host "[AUDITOR] Solicitando Token de Interacción..." -ForegroundColor Cyan

    $branch = Get-BranchName
    if ([string]::IsNullOrWhiteSpace($branch)) {
        Write-Host "Error: No hay rama activa." -ForegroundColor Red
        exit 1
    }

    # Validar Pasaporte antes de emitir token
    $passportValid = Assert-Passport -BranchName $branch
    if (-not $passportValid) {
        Write-Host "TOKEN DENEGADO." -ForegroundColor Red
        exit 1
    }

    # Generar Hash (Vinculado a Rama + Fecha)
    # Esto hace que el token expire naturalmente al cambiar de día o de rama
    $today = Get-Date -Format "yyyy-MM-dd"
    $raw = "{0}|{1}" -f $branch, $today
    $hash = Calculate-Hash -InputString $raw

    # Guardar en .git (oculto)
    if (-not (Test-Path ".git")) {
        New-Item -ItemType Directory -Path ".git" -Force | Out-Null
    }
    Set-Content -Path $TokenPath -Value $hash -Force

    Write-Host "Token generado: $hash" -ForegroundColor Green
    Write-Host "Contexto: $branch | $today" -ForegroundColor Gray
}

function Validate-Token {
    $branch = Get-BranchName
    $today = Get-Date -Format "yyyy-MM-dd"
    $raw = "{0}|{1}" -f $branch, $today
    $expectedHash = Calculate-Hash -InputString $raw

    if (-not (Test-Path $TokenPath)) {
        Write-Host "[AUDITOR] Token no encontrado." -ForegroundColor Yellow
        return $false
    }

    $storedHash = (Get-Content -Path $TokenPath).Trim()

    if ($storedHash -eq $expectedHash) {
        # Token válido
        return $true
    } else {
        Write-Host "[AUDITOR] Token expirado o inválido para este contexto." -ForegroundColor Red
        Write-Host "  Esperado: $expectedHash (Rama: $branch)" -ForegroundColor DarkGray
        Write-Host "  Actual:   $storedHash" -ForegroundColor DarkGray
        return $false
    }
}

# --- Flujo Principal ---

if ($Command -eq "Generate") {
    Generate-Token
} elseif ($Command -eq "Validate") {
    # 1. Intentar validar token existente
    if (Validate-Token) {
        exit 0
    }

    # 2. Si no es válido, intentar generar uno nuevo al vuelo (Auto-Provisioning)
    Write-Host "[AUDITOR] Intentando generar nuevo token..." -ForegroundColor Cyan
    Generate-Token

    # 3. Re-validar
    if (Validate-Token) {
        exit 0
    } else {
        Write-Host "FATAL: No se pudo establecer un token de interacción válido." -ForegroundColor Red
        exit 1
    }
} elseif ($Command -eq "Revoke") {
    if (Test-Path $TokenPath) {
        Remove-Item $TokenPath
        Write-Host "Token revocado." -ForegroundColor Green
    }
} else {
    Write-Host "Comando desconocido: $Command" -ForegroundColor Red
    exit 1
}
