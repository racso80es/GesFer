<#
TAE — Start-Task.ps1
Contrato: docs/branches/feat-tekton-automation-engine/INIT.md

Principios:
- Por defecto: PlanOnly ($true). No ejecuta cambios sin ApproveHash.
- Salida JSON determinista con suggestedNextStep.
- Códigos de salida estables (0,10,11,20-22,30,40,50).

Compatibilidad: Windows PowerShell 5.1+ / PowerShell 7+
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

    [ValidateNotNullOrEmpty()]
    [string]$BranchPrefix,

    [ValidateNotNullOrEmpty()]
    [string]$Branch,

    [ValidateNotNullOrEmpty()]
    [string]$BaseBranch = 'master',

    [ValidateNotNullOrEmpty()]
    [string]$Remote = 'origin',

    [switch]$NoFetch,
    [switch]$NoPrune,
    [switch]$ReuseIfExists,

    [bool]$FailIfDirty = $true,

    [bool]$EnsureBranchDocs = $true,
    [bool]$EnsureIATelemetry = $true,
    [bool]$EnsureGlobalTracker = $true,

    [ValidateNotNullOrEmpty()]
    [string]$Template = 'Tekton/Templates/IA_PERF_REPORT.md',

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
        tool              = 'Start-Task'
        version           = '1.0.0'
        ok                = $Ok
        exitCode          = $ExitCode
        name              = $Name
        scope             = $Scope
        type              = $Type
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

    # Text
    Write-Host ("OK={0} ExitCode={1}" -f $Result.ok, $Result.exitCode)
    if ($Result.planHash) { Write-Host ("PlanHash: {0}" -f $Result.planHash) }
    if ($Result.suggestedNextStep) { Write-Host ("Next: {0}" -f $Result.suggestedNextStep) }
}

function Get-BranchSlug {
    param([string]$BranchName)
    return ($BranchName -replace "[/\\]", "-")
}

function Get-NameSlug {
    param([string]$Value)
    $v = $Value.Trim().ToLowerInvariant()

    # Eliminar acentos/diacríticos cuando sea posible (para compatibilidad de branch)
    try {
        $norm = $v.Normalize([Text.NormalizationForm]::FormD)
        $sb = New-Object System.Text.StringBuilder
        foreach ($ch in $norm.ToCharArray()) {
            $cat = [Globalization.CharUnicodeInfo]::GetUnicodeCategory($ch)
            if ($cat -ne [Globalization.UnicodeCategory]::NonSpacingMark) {
                [void]$sb.Append($ch)
            }
        }
        $v = $sb.ToString().Normalize([Text.NormalizationForm]::FormC)
    } catch {
        # fallback: mantener $v
    }

    $v = ($v -replace "[^a-z0-9]+", "-").Trim("-")
    if ([string]::IsNullOrWhiteSpace($v)) { return "tarea" }
    return $v
}

function Compute-PlanHash {
    param([string[]]$Ops)
    $sorted = $Ops | Sort-Object
    $canonical = ($sorted -join [char]10)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($canonical) # UTF-8 sin BOM
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $hashBytes = $sha.ComputeHash($bytes)
    return ([System.BitConverter]::ToString($hashBytes)).Replace('-', '').ToUpperInvariant()
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
    param([string[]]$Args)
    $output = & git @Args 2>&1 | Out-String
    $code = $LASTEXITCODE
    return [ordered]@{
        exitCode = $code
        output   = $output.TrimEnd()
        args     = ($Args -join " ")
    }
}

function Classify-GitFailure {
    param([string]$GitOutput)
    $o = $GitOutput
    if ($o -match "CONFLICT|Merge conflict|Automatic merge failed") { return 21 }
    if ($o -match "Permission denied|Authentication failed|fatal: Authentication|denied to|not authorized") { return 22 }
    return 20
}

try {
    # 1) Dependencias y precondiciones base
    Assert-CommandAvailable -CommandName 'git' -FriendlyName 'Git'
    Assert-CommandAvailable -CommandName 'dotnet' -FriendlyName '.NET SDK (dotnet)'

    $inside = Invoke-Git -Args @('rev-parse', '--is-inside-work-tree')
    if ($inside.exitCode -ne 0 -or $inside.output -ne 'true') {
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

    if ($FailIfDirty) {
        $st = Invoke-Git -Args @('status', '--porcelain')
        if ($st.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $st.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Ejecuta: git status; revisa el estado y reintenta."
            Add-Error -Result $r -Category 'git' -Code 'GIT_STATUS_FAILED' -Message "Falló 'git status --porcelain'." -Remediation "Verifica Git/permiso y reintenta."
            Write-TaeOutput -Result $r
            exit $exit
        }
        if (-not [string]::IsNullOrWhiteSpace($st.output)) {
            $r = New-TaeResult -Ok $false -ExitCode 11 -SuggestedNextStep "Ejecuta: git status; limpia/stash/commit y reintenta Start-Task."
            Add-Error -Result $r -Category 'precondition' -Code 'WORKTREE_DIRTY' -Message "Working tree no está limpio." -Remediation "Haz commit/stash/limpia y reintenta."
            Write-TaeOutput -Result $r
            exit 11
        }
    }

    # 2) Derivar branch
    if ([string]::IsNullOrWhiteSpace($BranchPrefix)) {
        switch ($Type) {
            'Sencilla' { $BranchPrefix = 'feat' } # conservador: mantener convención existente en repo
            'Normal' { $BranchPrefix = 'feat' }
            'Compleja' { $BranchPrefix = 'feat' }
        }
    }

    if ([string]::IsNullOrWhiteSpace($Branch)) {
        $nameSlug = Get-NameSlug -Value $Name
        $Branch = ("{0}/{1}" -f $BranchPrefix, $nameSlug)
    }

    $branchSlug = Get-BranchSlug -BranchName $Branch

    # 3) Planificar operaciones
    $plannedOps = New-Object System.Collections.Generic.List[string]
    if (-not $NoFetch) { $plannedOps.Add(("OP|git|fetch|remote={0};prune={1}" -f $Remote, (-not $NoPrune))) }
    if (-not $NoPrune) { $plannedOps.Add(("OP|git|remote-prune|remote={0}" -f $Remote)) }
    $plannedOps.Add(("OP|git|checkout|branch={0};base={1};remote={2};reuseIfExists={3}" -f $Branch, $BaseBranch, $Remote, $ReuseIfExists.IsPresent))

    $artifacts = [ordered]@{}

    if ($EnsureBranchDocs) {
        $docPath = Join-Path -Path 'docs/branches' -ChildPath ("{0}.md" -f $branchSlug)
        $plannedOps.Add(("OP|file|ensure-nonempty|{0}" -f $docPath))
        $artifacts.branchDoc = $docPath
    }
    if ($EnsureGlobalTracker) {
        $gt = 'docs/performance/GLOBAL_IA_TRACKER.md'
        $plannedOps.Add(("OP|file|ensure-nonempty|{0}" -f $gt))
        $artifacts.aiGlobal = $gt
    }
    if ($EnsureIATelemetry) {
        $rp = Join-Path -Path 'docs/performance' -ChildPath ("IA_PERF_{0}.md" -f $branchSlug)
        $plannedOps.Add(("OP|file|ensure-nonempty|{0};template={1}" -f $rp, $Template))
        $artifacts.aiReport = $rp
    }

    $planHash = Compute-PlanHash -Ops $plannedOps.ToArray()

    $result = New-TaeResult -Ok $true -ExitCode 0 -SuggestedNextStep "Si quieres ejecutar, reintenta con: -PlanOnly:$false -ApproveHash $planHash" -Data @{
        branchName = $Branch
        branchSlug = $branchSlug
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

    # 4) Ejecutar plan (hash aprobado)
    if (-not (Test-Path -LiteralPath 'docs/branches')) { New-Item -ItemType Directory -Force -Path 'docs/branches' | Out-Null }
    if (-not (Test-Path -LiteralPath 'docs/performance')) { New-Item -ItemType Directory -Force -Path 'docs/performance' | Out-Null }

    if (-not $NoFetch) {
        $args = @('fetch', $Remote)
        if (-not $NoPrune) { $args += '--prune' }
        $fetch = Invoke-Git -Args $args
        if ($fetch.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $fetch.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Revisa conectividad/permisos con '$Remote' y reintenta."
            Add-Error -Result $r -Category 'git' -Code 'GIT_FETCH_FAILED' -Message $fetch.output -Remediation "Autentica/soluciona red y reintenta."
            Write-TaeOutput -Result $r
            exit $exit
        }
    }
    if (-not $NoPrune) {
        $pr = Invoke-Git -Args @('remote', 'prune', $Remote)
        if ($pr.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $pr.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Revisa conectividad/permisos con '$Remote' y reintenta."
            Add-Error -Result $r -Category 'git' -Code 'GIT_REMOTE_PRUNE_FAILED' -Message $pr.output -Remediation "Autentica/soluciona red y reintenta."
            Write-TaeOutput -Result $r
            exit $exit
        }
    }

    # Branch checkout/create
    $existsLocal = (Invoke-Git -Args @('show-ref', '--verify', '--quiet', ("refs/heads/{0}" -f $Branch))).exitCode -eq 0
    if ($existsLocal) {
        if (-not $ReuseIfExists) {
            $r = New-TaeResult -Ok $false -ExitCode 11 -SuggestedNextStep "Usa -ReuseIfExists para reusar la rama existente, o elige otro -Branch."
            Add-Error -Result $r -Category 'precondition' -Code 'BRANCH_EXISTS_LOCAL' -Message "La rama '$Branch' ya existe localmente." -Remediation "Reusa o cambia nombre."
            Write-TaeOutput -Result $r
            exit 11
        }
        $co = Invoke-Git -Args @('checkout', $Branch)
        if ($co.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $co.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Revisa errores de checkout y reintenta."
            Add-Error -Result $r -Category 'git' -Code 'GIT_CHECKOUT_FAILED' -Message $co.output -Remediation "Corrige el error y reintenta."
            Write-TaeOutput -Result $r
            exit $exit
        }
    } else {
        # prefer: crear desde remote/base si existe
        $baseRef = ("{0}/{1}" -f $Remote, $BaseBranch)
        $ck = Invoke-Git -Args @('show-ref', '--verify', '--quiet', ("refs/remotes/{0}" -f $baseRef))
        if ($ck.exitCode -eq 0) {
            $co = Invoke-Git -Args @('checkout', '-b', $Branch, $baseRef)
        } else {
            $co = Invoke-Git -Args @('checkout', '-b', $Branch, $BaseBranch)
        }
        if ($co.exitCode -ne 0) {
            $exit = Classify-GitFailure -GitOutput $co.output
            $r = New-TaeResult -Ok $false -ExitCode $exit -SuggestedNextStep "Verifica que '$BaseBranch' exista y reintenta."
            Add-Error -Result $r -Category 'git' -Code 'GIT_CREATE_BRANCH_FAILED' -Message $co.output -Remediation "Corrige base branch y reintenta."
            Write-TaeOutput -Result $r
            exit $exit
        }
    }

    # Ensure files non-empty
    if ($EnsureBranchDocs) {
        $docPath = Join-Path -Path 'docs/branches' -ChildPath ("{0}.md" -f $branchSlug)
        if (-not (Test-Path -LiteralPath $docPath)) {
            New-Item -ItemType File -Force -Path $docPath | Out-Null
        }
        $content = (Get-Content -LiteralPath $docPath -Raw -ErrorAction SilentlyContinue)
        if ([string]::IsNullOrWhiteSpace($content)) {
            $stub = @(
                "# {0}" -f $Branch
                ""
                "- Nombre: {0}" -f $Name
                "- Ámbito: {0}" -f $Scope
                "- Tipo: {0}" -f $Type
                ""
                "## INIT"
                "- `docs/branches/{0}/INIT.md` (si aplica)" -f $branchSlug
                ""
            ) -join "`r`n"
            [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $docPath).Path, $stub, [System.Text.Encoding]::UTF8)
        }
    }

    if ($EnsureGlobalTracker) {
        $gt = 'docs/performance/GLOBAL_IA_TRACKER.md'
        if (-not (Test-Path -LiteralPath $gt)) { New-Item -ItemType File -Force -Path $gt | Out-Null }
        $content = (Get-Content -LiteralPath $gt -Raw -ErrorAction SilentlyContinue)
        if ([string]::IsNullOrWhiteSpace($content)) {
            $stub = @(
                "# GLOBAL IA TRACKER"
                ""
                "Registro global de telemetría IA."
                ""
            ) -join "`r`n"
            [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $gt).Path, $stub, [System.Text.Encoding]::UTF8)
        }
    }

    if ($EnsureIATelemetry) {
        $rp = Join-Path -Path 'docs/performance' -ChildPath ("IA_PERF_{0}.md" -f $branchSlug)
        if (-not (Test-Path -LiteralPath $rp)) { New-Item -ItemType File -Force -Path $rp | Out-Null }
        $content = (Get-Content -LiteralPath $rp -Raw -ErrorAction SilentlyContinue)
        if ([string]::IsNullOrWhiteSpace($content)) {
            $tpl = $null
            if (Test-Path -LiteralPath $Template) {
                $tpl = (Get-Content -LiteralPath $Template -Raw -ErrorAction SilentlyContinue)
            }
            if ([string]::IsNullOrWhiteSpace($tpl)) {
                $tpl = @(
                    "# IA PERF — {0}" -f $Branch
                    ""
                    "Plantilla mínima (template no disponible)."
                    ""
                ) -join "`r`n"
            }
            [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $rp).Path, $tpl, [System.Text.Encoding]::UTF8)
        }
    }

    $result.ok = $true
    $result.exitCode = 0
    $result.suggestedNextStep = "Continúa trabajando; al cerrar, usa Close-Task.ps1 (modo Prepare)."
    Write-TaeOutput -Result $result
    exit 0
}
catch {
    $msg = $_.Exception.Message

    if ($msg -like 'DEPENDENCY_MISSING::*') {
        $parts = $msg.Split('::')
        $cmd = $parts[1]
        $friendly = $parts[2]
        $r = New-TaeResult -Ok $false -ExitCode 11 -SuggestedNextStep "Instala/expón en PATH: $friendly (comando '$cmd') y reintenta."
        Add-Error -Result $r -Category 'dependency' -Code 'DEPENDENCY_MISSING' -Message $msg -Remediation "Instala la dependencia y reintenta."
        Write-TaeOutput -Result $r
        exit 11
    }

    $r = New-TaeResult -Ok $false -ExitCode 50 -SuggestedNextStep "Revisa el error y reintenta; si persiste, captura logs y eleva a Racso."
    Add-Error -Result $r -Category 'unexpected' -Code 'UNEXPECTED_ERROR' -Message $msg -Remediation "Inspecciona excepción y corrige."
    Write-TaeOutput -Result $r
    exit 50
}

