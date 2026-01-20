<#
TAE - Close-Task.ps1
Contrato: docs/branches/feat-tekton-automation-engine/INIT.md

Modos:
- Prepare: valida + prepara evidencia + push (sin merge a master).
- Cleanup: limpieza post-merge (borrar rama local/remota, prune).
- All: Prepare + Cleanup (Cleanup solo si procede).

Por defecto: PlanOnly ($true). No ejecuta cambios sin ApproveHash.
#>

[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Name,

    [Parameter(Mandatory = $true)]
    [ValidateSet('Api', 'Cliente', 'Infra', 'Cross', 'Tekton')]
    [string]$Scope,

    [Parameter(Mandatory = $true)]
    [ValidateSet('Sencilla', 'Normal', 'Compleja')]
    [string]$Type,

    [ValidateSet('Prepare', 'Cleanup', 'All')]
    [string]$Mode = 'Prepare',

    [bool]$RequireMerged = $false,

    [bool]$RunValidateCommit = $true,
    [bool]$RunValidatePr = $true,
    [bool]$Autocheck = $true,

    [bool]$Push = $true,
    [bool]$DeleteLocalBranch = $true,
    [bool]$DeleteRemoteBranch = $false,

    [ValidateNotNullOrEmpty()]
    [string]$BaseBranch = 'master',

    [ValidateNotNullOrEmpty()]
    [string]$Remote = 'origin',

    [bool]$PlanOnly = $true,

    [ValidateNotNullOrEmpty()]
    [string]$ApproveHash,

    [ValidateSet('Text', 'Json')]
    [string]$OutputFormat = 'Text',

    [ValidateNotNullOrEmpty()]
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function New-TaeResult {
    param(
        [bool]$Ok,
        [int]$ExitCode,
        [string]$SuggestedNextStep,
        [hashtable]$Data
    )

    $result = [ordered]@{
        engine            = 'TAE'
        tool              = 'Close-Task'
        version           = '1.0.0'
        ok                = $Ok
        exitCode          = $ExitCode
        name              = $Name
        scope             = $Scope
        type              = $Type
        mode              = $Mode
        branchName        = $null
        branchSlug        = $null
        planOnly          = [bool]$PlanOnly
        planHash          = $null
        approvedHash      = if ([string]::IsNullOrWhiteSpace($ApproveHash)) { $null } else { $ApproveHash }
        plannedOps        = @()
        artifacts         = [ordered]@{}
        errors            = @()
        warnings          = @()
        suggestedNextStep = $SuggestedNextStep
    }

    if ($Data) {
        foreach ($k in $Data.Keys) {
            $result[$k] = $Data[$k]
        }
    }

    return $result
}

function Write-TaeOutput {
    param(
        [hashtable]$Result
    )

    if ($OutputFormat -eq 'Json') {
        $json = ($Result | ConvertTo-Json -Depth 12 -Compress)
        if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
            $dir = Split-Path -Parent $OutputPath
            if (-not [string]::IsNullOrWhiteSpace($dir)) {
                New-Item -ItemType Directory -Force -Path $dir | Out-Null
            }
            [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $OutputPath).Path, $json, [System.Text.Encoding]::UTF8)
        }
        Write-Output $json
        return
    }

    Write-Host ("OK={0} ExitCode={1}" -f $Result.ok, $Result.exitCode)
    if ($Result.planHash) { Write-Host ("PlanHash: {0}" -f $Result.planHash) }
    if ($Result.suggestedNextStep) { Write-Host ("Next: {0}" -f $Result.suggestedNextStep) }
}

function Add-Error {
    param(
        [hashtable]$Result,
        [string]$Category,
        [string]$Code,
        [string]$Message,
        [string]$Remediation
    )
    $Result.errors += [ordered]@{
        category    = $Category
        code        = $Code
        message     = $Message
        remediation = $Remediation
    }
}

function Get-BranchSlug {
    param([string]$BranchName)
    return ($BranchName -replace "[/\\]", "-")
}

function Compute-PlanHash {
    param([string[]]$Ops)
    $sorted = $Ops | Sort-Object
    $canonical = ($sorted -join [char]10)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($canonical)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $hashBytes = $sha.ComputeHash($bytes)
    return ([System.BitConverter]::ToString($hashBytes)).Replace('-', '').ToUpperInvariant()
}

function Assert-CommandAvailable {
    param(
        [string]$CommandName,
        [string]$FriendlyName
    )
    $cmd = Get-Command $CommandName -ErrorAction SilentlyContinue
    if ($null -eq $cmd) {
        throw ("DEPENDENCY_MISSING::{0}::{1}" -f $CommandName, $FriendlyName)
    }
}

function Invoke-Git {
    param([string[]]$GitArgs)
    $output = & git @GitArgs 2>&1 | Out-String
    $code = $LASTEXITCODE
    return [ordered]@{
        exitCode = $code
        output   = $output.TrimEnd()
        args     = ($GitArgs -join " ")
    }
}

function Classify-GitFailure {
    param([string]$GitOutput)
    $o = $GitOutput
    if ($o -match "CONFLICT|Merge conflict|Automatic merge failed") { return 21 }
    if ($o -match "Permission denied|Authentication failed|fatal: Authentication|denied to|not authorized") { return 22 }
    return 20
}

function Invoke-ExternalPs1 {
    param(
        [string]$Path
    )
    $p = Start-Process -FilePath 'powershell.exe' -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $Path) -Wait -PassThru -WindowStyle Hidden
    return $p.ExitCode
}

function Ensure-Directory {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Force -Path $Path | Out-Null
    }
}

try {
    Assert-CommandAvailable -CommandName 'git' -FriendlyName 'Git'
    Assert-CommandAvailable -CommandName 'dotnet' -FriendlyName '.NET SDK (dotnet)'

    $inside = Invoke-Git -GitArgs @('rev-parse', '--is-inside-work-tree')
    $insideOk = ($inside.exitCode -eq 0 -and $inside.output -match '(?m)^\s*true\s*$')
    if (-not $insideOk) {
        $r = New-TaeResult -Ok $false -ExitCode 11 -SuggestedNextStep "Ejecuta el comando dentro del repositorio Git."
        Add-Error -Result $r -Category 'precondition' -Code 'NOT_A_REPO' -Message "No estás dentro de un repositorio Git válido." -Remediation "Navega a la raíz del repo y reintenta."
        Write-TaeOutput -Result $r
        exit 11
    }

    # `.cursorrules` puntero estático: verificar, NO corregir automáticamente
    if (Test-Path -LiteralPath '.cursorrules') {
        $cr = (Get-Content -LiteralPath '.cursorrules' -Raw -ErrorAction SilentlyContinue).Trim()
        if ($cr -ne 'Tekton/Rules/GOLDEN_RULES.md') {
            $r = New-TaeResult -Ok $false -ExitCode 11 -SuggestedNextStep "Restaura '.cursorrules' al puntero: Tekton/Rules/GOLDEN_RULES.md"
            Add-Error -Result $r -Category 'precondition' -Code 'CURSORRULES_POINTER_INVALID' -Message "'.cursorrules' no apunta a Tekton/Rules/GOLDEN_RULES.md" -Remediation "Corrige el puntero y reintenta."
            Write-TaeOutput -Result $r
            exit 11
        }
    }

    $branch = (Invoke-Git -GitArgs @('branch', '--show-current')).output.Trim()
    if ([string]::IsNullOrWhiteSpace($branch)) {
        $r = New-TaeResult -Ok $false -ExitCode 11 -SuggestedNextStep "Ejecuta: git checkout <rama> y reintenta."
        Add-Error -Result $r -Category 'precondition' -Code 'NO_CURRENT_BRANCH' -Message "No se pudo determinar la rama actual." -Remediation "Asegura una rama válida."
        Write-TaeOutput -Result $r
        exit 11
    }

    $slug = Get-BranchSlug -BranchName $branch

    $plannedOps = New-Object System.Collections.Generic.List[string]
    $artifacts = [ordered]@{}

    $doPrepare = ($Mode -eq 'Prepare' -or $Mode -eq 'All')
    $doCleanup = ($Mode -eq 'Cleanup' -or $Mode -eq 'All')

    if ($doPrepare) {
        if ($RunValidateCommit) { $plannedOps.Add("OP|ps1|run|scripts/validate-commit.ps1") }
        if ($RunValidatePr) { $plannedOps.Add("OP|ps1|run|scripts/validate-pr.ps1") }
        if ($Autocheck) { $plannedOps.Add("OP|validation|autocheck|AC-001") }

        Ensure-Directory -Path 'docs/governance/audits'
        $ts = Get-Date -Format 'yyyyMMdd_HHmm'
        $auditPath = Join-Path -Path 'docs/governance/audits' -ChildPath ("{0}_{1}_CIERRE.md" -f $ts, $slug)
        $plannedOps.Add(("OP|file|write|{0}" -f $auditPath))
        $artifacts.audit = $auditPath

        if ($Push) {
            $plannedOps.Add(("OP|git|push|remote={0};branch={1}" -f $Remote, $branch))
        }
    }

    if ($doCleanup) {
        if ($RequireMerged) {
            $plannedOps.Add(("OP|git|assert-merged|branch={0};base={1}" -f $branch, $BaseBranch))
        }
        $plannedOps.Add(("OP|git|checkout|branch={0}" -f $BaseBranch))
        $plannedOps.Add(("OP|git|pull-ff-only|remote={0};branch={1}" -f $Remote, $BaseBranch))

        if ($DeleteLocalBranch) {
            $plannedOps.Add(("OP|git|delete-local-branch|branch={0}" -f $branch))
        }
        if ($DeleteRemoteBranch) {
            $plannedOps.Add(("OP|git|delete-remote-branch|remote={0};branch={1}" -f $Remote, $branch))
        }
        $plannedOps.Add(("OP|git|remote-prune|remote={0}" -f $Remote))
        $plannedOps.Add(("OP|git|assert-clean|base={0};remote={1}" -f $BaseBranch, $Remote))
    }

    $planHash = Compute-PlanHash -Ops $plannedOps.ToArray()

    $result = New-TaeResult -Ok $true -ExitCode 0 -SuggestedNextStep "Si quieres ejecutar, reintenta con: -PlanOnly:$false -ApproveHash $planHash" -Data @{
        branchName = $branch
        branchSlug = $slug
        planHash   = $planHash
        plannedOps = $plannedOps.ToArray()
        artifacts  = $artifacts
    }

    if ($PlanOnly) {
        Write-TaeOutput -Result $result
        exit 0
    }

    if ([string]::IsNullOrWhiteSpace($ApproveHash) -or $ApproveHash.ToUpperInvariant() -ne $planHash) {
        $result.ok = $false
        $result.exitCode = 10
        $result.suggestedNextStep = "Vuelve a ejecutar con -ApproveHash $planHash (exacto)."
        Add-Error -Result $result -Category 'contract' -Code 'HASH_NOT_APPROVED' -Message "ApproveHash no coincide con el planHash calculado." -Remediation "Copia el planHash exacto y reintenta."
        Write-TaeOutput -Result $result
        exit 10
    }

    # Ejecutar
    if ($doPrepare) {
        if ($RunValidateCommit) {
            if (-not (Test-Path -LiteralPath 'scripts/validate-commit.ps1')) {
                $r = New-TaeResult -Ok $false -ExitCode 40 -SuggestedNextStep "Verifica scripts/validate-commit.ps1."
                Add-Error -Result $r -Category 'io' -Code 'MISSING_VALIDATE_COMMIT' -Message "No existe scripts/validate-commit.ps1" -Remediation "Restaura el archivo."
                Write-TaeOutput -Result $r
                exit 40
            }
            $code = Invoke-ExternalPs1 -Path 'scripts/validate-commit.ps1'
            if ($code -ne 0) {
                $r = New-TaeResult -Ok $false -ExitCode 30 -SuggestedNextStep "Corrige el fallo en validate-commit y reintenta Close-Task."
                Add-Error -Result $r -Category 'validation' -Code 'VALIDATE_COMMIT_FAILED' -Message "validate-commit falló con exit code $code" -Remediation "Ejecuta scripts/validate-commit.ps1 y corrige."
                Write-TaeOutput -Result $r
                exit 30
            }
        }
        if ($RunValidatePr) {
            if (-not (Test-Path -LiteralPath 'scripts/validate-pr.ps1')) {
                $r = New-TaeResult -Ok $false -ExitCode 40 -SuggestedNextStep "Verifica scripts/validate-pr.ps1."
                Add-Error -Result $r -Category 'io' -Code 'MISSING_VALIDATE_PR' -Message "No existe scripts/validate-pr.ps1" -Remediation "Restaura el archivo."
                Write-TaeOutput -Result $r
                exit 40
            }
            $code = Invoke-ExternalPs1 -Path 'scripts/validate-pr.ps1'
            if ($code -ne 0) {
                $r = New-TaeResult -Ok $false -ExitCode 30 -SuggestedNextStep "Corrige el fallo del Juez (validate-pr) y reintenta Close-Task."
                Add-Error -Result $r -Category 'validation' -Code 'VALIDATE_PR_FAILED' -Message "validate-pr falló con exit code $code" -Remediation "Ejecuta scripts/validate-pr.ps1 y corrige."
                Write-TaeOutput -Result $r
                exit 30
            }
        }

        # Evidencia mínima de cierre (auditoría)
        Ensure-Directory -Path 'docs/governance/audits'
        $ts = Get-Date -Format 'yyyyMMdd_HHmm'
        $auditPath = Join-Path -Path 'docs/governance/audits' -ChildPath ("{0}_{1}_CIERRE.md" -f $ts, $slug)
        if (-not (Test-Path -LiteralPath $auditPath)) {
            $content = @(
                "# CIERRE - {0}" -f $branch
                ""
                "- Nombre: {0}" -f $Name
                "- Ambito: {0}" -f $Scope
                "- Tipo: {0}" -f $Type
                "- Fecha: {0}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm')
                ""
                "## Validaciones"
                "- validate-commit: {0}" -f $RunValidateCommit
                "- validate-pr: {0}" -f $RunValidatePr
                ""
                "## Notas"
                "- (Completar por Racso/Tormentosa si aplica)"
                ""
            ) -join "`r`n"
            [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath (Split-Path -Parent $auditPath)).Path + "\" + (Split-Path -Leaf $auditPath), $content, [System.Text.Encoding]::UTF8)
        }

        if ($Push) {
            $push = Invoke-Git -GitArgs @('push', '-u', $Remote, 'HEAD')
            if ($push.exitCode -ne 0) {
                $exit = Classify-GitFailure -GitOutput $push.output
                $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Reautentica o revisa permisos en remoto y reintenta."
                Add-Error -Result $r -Category 'git' -Code 'GIT_PUSH_FAILED' -Message $push.output -Remediation "Soluciona auth/red y reintenta."
                Write-TaeOutput -Result $r
                exit $exit
            }
        }
    }

    if ($doCleanup) {
        if ($RequireMerged) {
            $merged = Invoke-Git -GitArgs @('branch', '--merged', $BaseBranch)
            if ($merged.exitCode -ne 0) {
                $exit = Classify-GitFailure -GitOutput $merged.output
                $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Revisa estado de merge y reintenta."
                Add-Error -Result $r -Category 'git' -Code 'GIT_MERGED_CHECK_FAILED' -Message $merged.output -Remediation "Corrige Git y reintenta."
                Write-TaeOutput -Result $r
                exit $exit
            }
            if ($merged.output -notmatch [Regex]::Escape($branch)) {
                $r = New-TaeResult -Ok $false -ExitCode 11 -SuggestedNextStep "Confirma merge en PR y reintenta Close-Task -Mode Cleanup."
                Add-Error -Result $r -Category 'precondition' -Code 'BRANCH_NOT_MERGED' -Message "La rama '$branch' no aparece como mergeada en '$BaseBranch'." -Remediation "Mergea por PR y reintenta."
                Write-TaeOutput -Result $r
                exit 11
            }
        }

        $co = Invoke-Git -GitArgs @('checkout', $BaseBranch)
        if ($co.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $co.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Verifica base branch y reintenta."
            Add-Error -Result $r -Category 'git' -Code 'GIT_CHECKOUT_BASE_FAILED' -Message $co.output -Remediation "Corrige y reintenta."
            Write-TaeOutput -Result $r
            exit $exit
        }

        $pull = Invoke-Git -GitArgs @('pull', '--ff-only', $Remote, $BaseBranch)
        if ($pull.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $pull.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Revisa divergencias; reintenta o resuelve manualmente."
            Add-Error -Result $r -Category 'git' -Code 'GIT_PULL_FF_FAILED' -Message $pull.output -Remediation "Corrige divergencia y reintenta."
            Write-TaeOutput -Result $r
            exit $exit
        }

        if ($DeleteLocalBranch) {
            $del = Invoke-Git -GitArgs @('branch', '-d', $branch)
            if ($del.exitCode -ne 0) {
                $exit = Classify-GitFailure -GitOutput $del.output
                $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Verifica si la rama está mergeada; si no, no se puede borrar con -d."
                Add-Error -Result $r -Category 'git' -Code 'GIT_DELETE_LOCAL_FAILED' -Message $del.output -Remediation "Mergea o borra manualmente."
                Write-TaeOutput -Result $r
                exit $exit
            }
        }

        if ($DeleteRemoteBranch) {
            $delr = Invoke-Git -GitArgs @('push', $Remote, '--delete', $branch)
            if ($delr.exitCode -ne 0) {
                $exit = Classify-GitFailure -GitOutput $delr.output
                $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Revisa permisos para borrar ramas remotas."
                Add-Error -Result $r -Category 'git' -Code 'GIT_DELETE_REMOTE_FAILED' -Message $delr.output -Remediation "Solicita permisos o borra manualmente."
                Write-TaeOutput -Result $r
                exit $exit
            }
        }

        $pr = Invoke-Git -GitArgs @('remote', 'prune', $Remote)
        if ($pr.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $pr.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Revisa conectividad/permisos con '$Remote' y reintenta."
            Add-Error -Result $r -Category 'git' -Code 'GIT_REMOTE_PRUNE_FAILED' -Message $pr.output -Remediation "Autentica/soluciona red y reintenta."
            Write-TaeOutput -Result $r
            exit $exit
        }

        $status = Invoke-Git -GitArgs @('status')
        if ($status.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $status.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Ejecuta: git status (manual) y revisa."
            Add-Error -Result $r -Category 'git' -Code 'GIT_STATUS_FAILED' -Message $status.output -Remediation "Revisa repo."
            Write-TaeOutput -Result $r
            exit $exit
        }
    }

    $result.ok = $true
    $result.exitCode = 0
    $result.suggestedNextStep = "Si procede, crea PR (Prepare) y luego ejecuta Cleanup tras merge."
    Write-TaeOutput -Result $result
    exit 0
}
catch {
    $msg = $_.Exception.Message
    if ($msg -like 'DEPENDENCY_MISSING::*') {
        $r = New-TaeResult -Ok $false -ExitCode 11 -SuggestedNextStep "Instala/expón dependencias (git/dotnet) y reintenta."
        Add-Error -Result $r -Category 'dependency' -Code 'DEPENDENCY_MISSING' -Message $msg -Remediation "Instala la dependencia y reintenta."
        Write-TaeOutput -Result $r
        exit 11
    }

    $r = New-TaeResult -Ok $false -ExitCode 50 -SuggestedNextStep "Revisa el error y reintenta; si persiste, captura logs y eleva a Racso."
    Add-Error -Result $r -Category 'unexpected' -Code 'UNEXPECTED_ERROR' -Message $msg -Remediation "Inspecciona excepción y corrige."
    Write-TaeOutput -Result $r
    exit 50
}

