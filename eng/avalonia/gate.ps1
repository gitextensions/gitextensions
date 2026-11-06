#Requires -Version 7.0
# parity-scaffolding: single verification entry point for Avalonia port work until the parity gate closes.

<#
.SYNOPSIS
    Runs the Avalonia port verification gate and records one JSON receipt per run.

.DESCRIPTION
    Edit   Windows incremental build of the test project plus the focused tests.
    Batch  What a commit needs: guards, the WinForms solution only when a changed path can affect
           it, the Avalonia solution, focused and construction tests, inventory tests and the
           reviewed sweep, then the native-ext4 Linux build and focused tests.
    Exit   Everything forced with nothing reused: the whole test assembly on both systems, the
           Linux inventory compared with Windows, and the runtime smokes.
    CommentOnly (Batch) proves the complete HEAD delta before building Windows and running the
           reviewed inventory. All tests and Linux are skipped with reasons in the receipt.

    Legs run one at a time because the solutions share intermediate paths. A Batch leg is reused
    when no path it depends on changed since it last passed with the same filter, so a retry costs
    only what the fix can affect. A receipt names the git tree it verified; -Verify checks that a
    passing receipt covers the current tree.

.EXAMPLE
    pwsh eng/avalonia/gate.ps1 -Level Edit -Filter 'FullyQualifiedName~FormBrowseTests'

.EXAMPLE
    pwsh eng/avalonia/gate.ps1 -Level Batch -Filter 'FullyQualifiedName~FormBrowseTests'

.EXAMPLE
    pwsh eng/avalonia/gate.ps1 -Verify

.EXAMPLE
    pwsh eng/avalonia/gate.ps1 -Level Batch -CommentOnly
#>
[CmdletBinding()]
param(
    [ValidateSet('Edit', 'Batch', 'Exit')]
    [string]$Level = 'Batch',

    # dotnet test filter for the focused tests. Required unless only other legs run.
    [string]$Filter,

    # Prove the complete HEAD delta first, then build Windows and measure inventory without tests.
    [switch]$CommentOnly,

    # When positive, a focused leg must execute exactly this many cases.
    [int]$ExpectedCases = 0,

    # Standalone tooling is in neither solution. Build and test these projects on both systems.
    [string[]]$ToolTestProject = @(),

    # Changes since this ref decide whether the WinForms solution can be affected.
    [string]$Base = 'HEAD',

    # Run only these legs. Such a run never satisfies -Verify.
    [ValidateSet('winforms', 'edit-build', 'avalonia-build', 'focused', 'construction', 'tool-tests', 'linux-tool-tests', 'inventory', 'linux', 'linux-inventory', 'windows-smoke', 'linux-runtime')]
    [string[]]$Only,

    # Print the legs and why, and run nothing.
    [switch]$PlanOnly,

    # Check that a passing Batch or Exit receipt covers the current tree.
    [switch]$Verify,

    # Run every leg instead of reusing earlier passes.
    [switch]$NoReuse,

    # Permit changes to the read-only original sources. They must also enter the permanent delta.
    [switch]$AllowProtected,

    # Remove the Windows test and application build outputs, then exit.
    [switch]$CleanOutputs
)

$ErrorActionPreference = 'Stop'

if ($CommentOnly -and ($Level -ne 'Batch' -or $Only -or $ToolTestProject.Count -or $Filter)) {
    throw '-CommentOnly requires Batch with no -Only, -ToolTestProject or -Filter.'
}

$repo = (git -C $PSScriptRoot rev-parse --show-toplevel).Trim().Replace('/', '\')
Set-Location -LiteralPath $repo

$gateRoot = Join-Path $repo 'eng\avalonia\parity-evidence\gates'
$testProject = 'tests/app/UnitTests/GitUI.Avalonia.Tests/GitUI.Avalonia.Tests.csproj'
$inventoryProject = 'eng/tools/ParityInventory.Tests'
$inventoryDll = 'artifacts/Debug/bin/ParityInventory/net10.0/ParityInventory.dll'
$constructionFilter = 'FullyQualifiedName~PreviewabilityTests.Every_AXAML_view_should_construct|FullyQualifiedName~PreviewabilityTests.Non_window_AXAML_views|FullyQualifiedName~PreviewabilityTests.Every_AXAML_control_type'
$wholeAssemblyFilter = 'Category!=VisualParityCapture'
$buildArguments = @('-m:1', '-nr:false', '-p:CreateHardLinksForCopyLocalIfPossible=true', '-v:minimal')
$adapterArguments = @('--', 'NUnit.AssemblySelectLimit=100000')

# Paths that can never reach the WinForms solution. Anything else, unknown paths included, builds it.
$cannotAffectWinForms = '^(src/app/GitUI\.Avalonia/|src/app/GitExtensions\.Avalonia/|src/app/GitExtensions\.Shims\.WinForms/|tests/app/UnitTests/GitUI\.Avalonia\.Tests/|eng/avalonia/|eng/tools/|GitExtensions\.Avalonia\.slnx$)'
# Paths that cannot change what the Avalonia build and its tests do. These standalone tools have no product consumers.
$cannotAffectProduct = '^eng/avalonia/(portmap\.json|parity-ledger\.json|framework-adaptations\.json|[^/]+\.(md|ps1))$|^eng/tools/(?:ParityFix|ParityInventory)(?:\.Tests)?/'
# Paths that cannot change the source inventory. The portmap, ledger and adaptations can.
$cannotAffectInventory = '^eng/avalonia/[^/]+\.(md|ps1)$|^eng/tools/ParityFix(?:\.Tests)?/'
$readOnlyOriginals = '^(src/app/GitUI/|src/app/GitExtensions/|src/app/BugReporter/|externals/(?!Git\.hub\.Portable/|Directory\.Build\.))'
$anyPath = '^$'
$warnBelowGb = 15
# A Linux build alone grew the Windows pagefile by 3.9 GB on a 15 GB machine, on top of its output.
$refuseBelowGb = 6
$keepPassingLogs = 5

foreach ($project in $ToolTestProject) {
    if ($project -notmatch '^eng/tools/[A-Za-z0-9_./-]+\.csproj$' -or $project.Split('/') -contains '..' -or -not (Test-Path -LiteralPath $project)) {
        throw "-ToolTestProject needs an existing repository-relative eng/tools/*.csproj path: $project"
    }
}
if (@($Only | Where-Object { $_ -in 'tool-tests', 'linux-tool-tests' }).Count -and -not $ToolTestProject.Count) {
    throw 'Tool test legs require -ToolTestProject.'
}

function Get-FreeGb {
    return [math]::Round((Get-PSDrive -Name $repo.Substring(0, 1)).Free / 1GB, 1)
}

function Remove-ExpiredGateLogs([string]$GateRoot, [int]$KeepPassingLogs) {
    $passingRuns = @(Get-ChildItem -LiteralPath $gateRoot -Directory |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'receipt.json') } |
        Sort-Object Name -Descending |
        Where-Object { (Get-Content -LiteralPath (Join-Path $_.FullName 'receipt.json') -Raw | ConvertFrom-Json).status -eq 'passed' })
    foreach ($old in @($passingRuns | Select-Object -Skip $keepPassingLogs)) {
        $files = @(Get-ChildItem -LiteralPath $old.FullName -File)
        foreach ($result in @($files | Where-Object Extension -eq '.trx')) {
            try { $document = [xml][IO.File]::ReadAllText($result.FullName) }
            catch { continue } # An unreadable result cannot identify a deployment directory safely.
            foreach ($deployment in $document.SelectNodes('//*[local-name()="Deployment"]/@runDeploymentRoot')) {
                if ([string]::IsNullOrWhiteSpace($deployment.Value)) { continue }
                try { $target = [IO.Path]::GetFullPath((Join-Path $old.FullName $deployment.Value)) }
                catch { continue }
                # Only a named test deployment directly inside this run can be removed recursively.
                if ([IO.Path]::GetDirectoryName($target) -ne $old.FullName -or
                    [IO.Path]::GetFileName($target) -in 'windows-smoke', 'linux-runtime') { continue }
                if (Test-Path -LiteralPath $target -PathType Container) {
                    Remove-Item -LiteralPath $target -Recurse -Force
                }
            }
        }
        $files | Where-Object { $_.Extension -in '.log', '.trx' } | Remove-Item
    }
}

# Hashes the whole working tree, untracked files included, without touching the real index.
function Get-WorkTree {
    $index = New-TemporaryFile
    try {
        Copy-Item -LiteralPath ((git rev-parse --git-path index).Trim()) -Destination $index.FullName -Force
        $env:GIT_INDEX_FILE = $index.FullName
        git add -A -- . 2>$null
        return (git write-tree).Trim()
    }
    finally {
        Remove-Item Env:GIT_INDEX_FILE -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $index.FullName -ErrorAction SilentlyContinue
    }
}

function Get-ChangedPaths([string]$From, [string]$To) {
    $paths = @(git diff --name-only --no-renames $From $To 2>$null)
    if ($LASTEXITCODE -ne 0) { return $null }
    return , $paths
}

function Get-PriorRuns {
    if (-not (Test-Path -LiteralPath $gateRoot)) { return @() }
    return @(Get-ChildItem -LiteralPath $gateRoot -Directory |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'receipt.json') } |
        Sort-Object Name -Descending |
        Select-Object -First 40 |
        ForEach-Object { Get-Content -LiteralPath (Join-Path $_.FullName 'receipt.json') -Raw | ConvertFrom-Json })
}

function ConvertTo-WslPath([string]$Path) {
    return '/mnt/' + $Path.Substring(0, 1).ToLowerInvariant() + $Path.Substring(2).Replace('\', '/')
}

function Assert-CleanBuild([string]$Log, [int]$ExitCode) {
    $warnings = @(Select-String -LiteralPath $Log -Pattern ':\s+warning\s+[A-Z]+\d+\s*:' | Select-Object -First 10)
    if ($ExitCode -ne 0) { throw "build failed (exit $ExitCode); see $Log" }
    if ($warnings.Count) { throw "build has warnings, for example: $($warnings[0].Line.Trim())" }
}

function Invoke-Dotnet([string]$Log, [string[]]$Arguments) {
    & dotnet @Arguments *> $Log
    return $LASTEXITCODE
}

function Assert-CommentOnlyProof([string]$Log, [string]$Report) {
    $exitCode = Invoke-Dotnet $Log @($proofDll, 'prove', '--report', $Report)
    if ($exitCode -ne 0) { throw "comment-only proof failed (exit $exitCode); see $Log" }
    $proof = Get-Content -LiteralPath $Report -Raw | ConvertFrom-Json
    if (-not $proof.Passed) { throw "comment-only proof refused the working tree; see $Report" }
}

function Get-CommentOnlyLegs {
    $reason = 'comment-only: repository-wide proof replaces test and Linux legs'
    @{ Name = 'comment-proof'; Ignored = $anyPath }
    @{ Name = 'winforms'; Ignored = $cannotAffectWinForms; Skip = $skipWinForms }
    @{ Name = 'avalonia-build'; Ignored = $cannotAffectProduct }
    foreach ($name in 'focused', 'construction', 'tool-tests', 'inventory-tests') {
        @{ Name = $name; Ignored = $anyPath; Skip = $reason }
    }
    @{ Name = 'inventory'; Ignored = $cannotAffectInventory }
    foreach ($name in 'linux', 'linux-tool-tests') {
        @{ Name = $name; Ignored = $anyPath; Skip = $reason }
    }
}

function Read-Trx([string]$Path, [int]$ExitCode) {
    if (-not (Test-Path -LiteralPath $Path)) { throw "no test results (exit $ExitCode); expected $Path" }
    $results = @(([xml](Get-Content -LiteralPath $Path -Raw)).TestRun.Results.UnitTestResult | Where-Object { $_ })
    $passed = @($results | Where-Object outcome -EQ 'Passed').Count
    $skipped = @($results | Where-Object outcome -EQ 'NotExecuted').Count
    $failed = @($results | Where-Object { $_.outcome -notin 'Passed', 'NotExecuted' })
    return [pscustomobject]@{
        total = $results.Count
        passed = $passed
        skipped = $skipped
        failed = $failed.Count
        failedNames = @($failed | Select-Object -First 15 | ForEach-Object testName)
    }
}

function Assert-Tests($Result, [int]$ExitCode, [int]$Expected) {
    if ($Result.failed) { throw "$($Result.failed) failed: $($Result.failedNames -join ', ')" }
    if ($ExitCode -ne 0) { throw "test host exited $ExitCode with no failed case; the run may have aborted" }
    if ($Result.total -eq 0) { throw 'the filter selected no tests' }
    if ($Expected -gt 0 -and $Result.total -ne $Expected) { throw "expected $Expected cases, ran $($Result.total)" }
}

function Invoke-WindowsTests([string]$Name, [string]$Project, [string]$TestFilter, [int]$Expected = 0, [switch]$Avalonia) {
    $log = Join-Path $runDirectory "$Name.log"
    $trx = Join-Path $runDirectory "$Name.trx"
    $arguments = @('test', $Project, '--no-build', '--no-restore', '--filter', $TestFilter, '--logger', "trx;LogFileName=$Name.trx", '--results-directory', $runDirectory)
    if ($Avalonia) { $arguments += '-p:BuildAvalonia=true' }
    $exitCode = Invoke-Dotnet $log ($arguments + $adapterArguments)
    $result = Read-Trx $trx $exitCode
    Assert-Tests $result $exitCode $Expected
    return $result
}

function Test-AssemblyCompleted([string]$Trx) {
    if (-not (Test-Path -LiteralPath $Trx)) { return $false }
    try {
        $run = [xml][IO.File]::ReadAllText($Trx)
        $summary = $run.SelectSingleNode('//*[local-name()="ResultSummary"]')
        $runErrors = @($run.SelectNodes('//*[local-name()="RunInfo"]') | Where-Object outcome -in 'Error', 'Aborted')
        return $null -ne $summary -and $summary.outcome -in 'Passed', 'Completed', 'Failed' -and
            $null -eq $summary.SelectSingleNode('./*[local-name()="ErrorInfo"]') -and $runErrors.Count -eq 0
    }
    catch { return $false }
}

# Multiline commands reach WSL as an owned LF script: Windows argument reparsing corrupts them otherwise.
function Invoke-WslScript([string]$Name, [string]$Body) {
    # A CR after a Bash export becomes part of its value, so normalize generated wrappers.
    $Body = $Body.Replace("`r`n", "`n")
    $script = Join-Path $runDirectory "$Name.sh"
    [IO.File]::WriteAllText($script, "#!/usr/bin/env bash`nset -euo pipefail`n$Body`n", [Text.UTF8Encoding]::new($false))
    # A login shell, because dotnet is only on PATH through the user's profile.
    & wsl.exe -d Ubuntu -- bash -l (ConvertTo-WslPath $script) *> (Join-Path $runDirectory "$Name.log")
    return $LASTEXITCODE
}

function Read-InventorySummary([string]$Inventory) {
    $summary = (Get-Content -LiteralPath $Inventory -Raw | ConvertFrom-Json).summary
    return [pscustomobject]@{
        findingCount = $summary.findingCount
        dependentFindingCount = $summary.dependentFindingCount
        mappingCount = $summary.mappingCount
        analyzedTypeCount = $summary.analyzedTypeCount
    }
}

$legActions = @{
    'comment-proof' = {
        $log = Join-Path $runDirectory 'comment-proof-build.log'
        Assert-CleanBuild $log (Invoke-Dotnet $log (@('build', 'eng/tools/ParityFix/ParityFix.csproj') + $buildArguments))
        $proofDll = 'artifacts/Debug/bin/ParityFix/net10.0/ParityFix.dll'
        Assert-CommentOnlyProof (Join-Path $runDirectory 'comment-proof.log') (Join-Path $runDirectory 'comment-proof.json')
    }
    'winforms' = {
        $log = Join-Path $runDirectory 'winforms.log'
        Assert-CleanBuild $log (Invoke-Dotnet $log (@('build', 'GitExtensions.slnx') + $buildArguments))
    }
    'edit-build' = {
        $log = Join-Path $runDirectory 'edit-build.log'
        Assert-CleanBuild $log (Invoke-Dotnet $log (@('build', $testProject, '-p:BuildAvalonia=true') + $buildArguments))
    }
    'avalonia-build' = {
        $log = Join-Path $runDirectory 'avalonia-build.log'
        Assert-CleanBuild $log (Invoke-Dotnet $log (@('build', 'GitExtensions.Avalonia.slnx', '-p:BuildAvalonia=true') + $buildArguments))
    }
    'focused' = {
        $testFilter = if ($Level -eq 'Exit') { $wholeAssemblyFilter } else { $Filter }
        Invoke-WindowsTests 'focused' $testProject $testFilter -Expected $ExpectedCases -Avalonia
    }
    'construction' = {
        # Its own process: another fixture's initialized services can hide a constructor's bootstrap dependency.
        Invoke-WindowsTests 'construction' $testProject $constructionFilter -Avalonia
    }
    'tool-tests' = {
        $results = foreach ($project in $ToolTestProject) {
            $name = 'tool-' + [IO.Path]::GetFileNameWithoutExtension($project)
            $log = Join-Path $runDirectory "$name-build.log"
            Assert-CleanBuild $log (Invoke-Dotnet $log (@('build', $project) + $buildArguments))
            Invoke-WindowsTests $name $project 'FullyQualifiedName~.'
        }
        [pscustomobject]@{ total = ($results.total | Measure-Object -Sum).Sum; passed = ($results.passed | Measure-Object -Sum).Sum; skipped = ($results.skipped | Measure-Object -Sum).Sum; failed = 0 }
    }
    'linux-tool-tests' = {
        $results = foreach ($project in $ToolTestProject) {
            $name = 'linux-tool-' + [IO.Path]::GetFileNameWithoutExtension($project)
            $exitCode = Invoke-WslScript $name @"
bash '$(ConvertTo-WslPath $repo)/eng/avalonia/wsl-gate.sh' run dotnet test '$project' -m:1 -v:minimal --logger 'trx;LogFileName=$name.trx' --results-directory '$(ConvertTo-WslPath $runDirectory)' -- NUnit.AssemblySelectLimit=100000
"@
            Assert-CleanBuild (Join-Path $runDirectory "$name.log") $exitCode
            $result = Read-Trx (Join-Path $runDirectory "$name.trx") $exitCode
            Assert-Tests $result $exitCode 0
            $result
        }
        [pscustomobject]@{ total = ($results.total | Measure-Object -Sum).Sum; passed = ($results.passed | Measure-Object -Sum).Sum; skipped = ($results.skipped | Measure-Object -Sum).Sum; failed = 0 }
    }
    'inventory' = {
        $log = Join-Path $runDirectory 'inventory-build.log'
        $project = if ($CommentOnly) { 'eng/tools/ParityInventory/ParityInventory.csproj' } else { $inventoryProject }
        Assert-CleanBuild $log (Invoke-Dotnet $log (@('build', $project) + $buildArguments))
        $tests = if ($CommentOnly) { [pscustomobject]@{} } else {
            Invoke-WindowsTests 'inventory-tests' $inventoryProject 'FullyQualifiedName~GitExtensions.ParityInventory.Tests'
        }
        $latest = Join-Path $gateRoot 'latest-inventory'
        if (Test-Path -LiteralPath $latest) { Remove-Item -LiteralPath $latest -Recurse }
        $null = New-Item -ItemType Directory -Path $latest
        $output = Join-Path $latest 'current-inventory.json'
        $exitCode = Invoke-Dotnet (Join-Path $runDirectory 'inventory-sweep.log') @(
            $inventoryDll, 'sweep', '--portmap', 'eng/avalonia/portmap.json', '--original-root', 'src/app/GitUI',
            '--twin-root', 'src/app/GitUI.Avalonia', '--translations', 'src/app/GitUI/Translation/English.xlf',
            '--analyzed-commit', $head, '--output', $output, '--framework-adaptations', 'eng/avalonia/framework-adaptations.json')
        if ($exitCode -ne 0) { throw "inventory sweep failed (exit $exitCode); a stale or drifted adaptation also fails it" }
        & (Join-Path $PSScriptRoot 'parity-dashboard.ps1') -Inventory $output *> (Join-Path $runDirectory 'dashboard.log')
        $inventory = Read-InventorySummary $output
        $tests | Add-Member -NotePropertyName inventory -NotePropertyValue $inventory
        $tests
    }
    'linux' = {
        $testFilter = if ($Level -eq 'Exit') { $wholeAssemblyFilter } else { $Filter }
        $quoted = "'" + $testFilter.Replace("'", "'\''") + "'"
        $exitCode = Invoke-WslScript 'linux' @"
export GITEXTENSIONS_DEBUG_FAIL_FAST=1
export GITEXTENSIONS_GATE_RESULTS='$(ConvertTo-WslPath $runDirectory)'
bash '$(ConvertTo-WslPath $repo)/eng/avalonia/wsl-gate.sh' gate $quoted
"@
        $log = Join-Path $runDirectory 'linux.log'
        $warnings = @(Select-String -LiteralPath $log -Pattern ':\s+warning\s+[A-Z]+\d+\s*:' | Select-Object -First 1)
        if ($warnings.Count) { throw "Linux build has warnings, for example: $($warnings[0].Line.Trim())" }
        $result = Read-Trx (Join-Path $runDirectory 'linux-focused.trx') $exitCode
        Assert-Tests $result $exitCode $ExpectedCases
        $result
    }
    'linux-inventory' = {
        $windows = Join-Path $gateRoot 'latest-inventory'
        if (-not (Test-Path -LiteralPath (Join-Path $windows 'current-inventory.json'))) { throw 'run the inventory leg first' }
        $output = Join-Path $runDirectory 'linux-inventory'
        $exitCode = Invoke-WslScript 'linux-inventory' @"
bash '$(ConvertTo-WslPath $repo)/eng/avalonia/wsl-gate.sh' run bash -c 'dotnet build $inventoryProject -m:1 -v:minimal && dotnet $inventoryDll sweep --portmap eng/avalonia/portmap.json --original-root src/app/GitUI --twin-root src/app/GitUI.Avalonia --translations src/app/GitUI/Translation/English.xlf --analyzed-commit $head --output "$(ConvertTo-WslPath $output)/current-inventory.json" --framework-adaptations eng/avalonia/framework-adaptations.json'
"@
        if ($exitCode -ne 0) { throw "Linux inventory failed (exit $exitCode)" }
        $difference = @(Compare-Object `
            @(Get-ChildItem -LiteralPath $windows -Recurse -File | ForEach-Object { '{0} {1}' -f [IO.Path]::GetRelativePath($windows, $_.FullName), (Get-FileHash -LiteralPath $_.FullName).Hash }) `
            @(Get-ChildItem -LiteralPath $output -Recurse -File | ForEach-Object { '{0} {1}' -f [IO.Path]::GetRelativePath($output, $_.FullName), (Get-FileHash -LiteralPath $_.FullName).Hash }))
        if ($difference.Count) { throw "Linux and Windows inventories differ in $($difference.Count) file entries" }
    }
    'windows-smoke' = {
        & (Join-Path $PSScriptRoot 'windows-runtime-smoke.ps1') -BrowseWindow -CheckPlugins -EvidenceDirectory (Join-Path $runDirectory 'windows-smoke') *> (Join-Path $runDirectory 'windows-smoke.log')
    }
    'linux-runtime' = {
        $runtimeEvidence = ConvertTo-WslPath (Join-Path $runDirectory 'linux-runtime')
        $exitCode = Invoke-WslScript 'linux-runtime' @"
export GITEXTENSIONS_RUNTIME_EVIDENCE='$runtimeEvidence'
bash '$(ConvertTo-WslPath $repo)/eng/avalonia/wsl-gate.sh' runtime
"@
        if ($exitCode -ne 0) { throw "Linux runtime smoke failed (exit $exitCode)" }
        foreach ($backend in 'wayland', 'x11') {
            $manifestPath = Join-Path $runDirectory "linux-runtime/$backend/smoke.json"
            if (-not (Test-Path -LiteralPath $manifestPath)) { throw "Linux runtime evidence is missing: $manifestPath" }
            $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
            if ($manifest.backend -ne $backend -or $manifest.screenshotSha256 -cnotmatch '^[0-9a-f]{64}$') {
                throw "Invalid Linux $backend runtime manifest: $manifestPath"
            }
        }
    }
}

if ($CleanOutputs) {
    foreach ($relative in 'artifacts\Debug\bin\tests', 'artifacts\Debug\bin\GitExtensions.Avalonia') {
        $target = Join-Path $repo $relative
        if (Test-Path -LiteralPath $target) {
            Remove-Item -LiteralPath $target -Recurse
            "removed $relative"
        }
    }
    "free: $(Get-FreeGb) GB"
    exit 0
}

$tree = Get-WorkTree
$head = (git rev-parse HEAD).Trim()

if ($Verify) {
    $passing = @(Get-PriorRuns | Where-Object {
        $_.status -eq 'passed' -and $_.level -in 'Batch', 'Exit' -and -not $_.partial -and
            (-not $_.commentOnly -or @($_.legs | Where-Object { $_.name -eq 'comment-proof' -and $_.status -eq 'passed' -and -not $_.reusedFrom }).Count -eq 1)
    })
    $exact = @($passing | Where-Object tree -EQ $tree)
    if ($exact.Count) {
        "verified: $($exact[0].level) run $($exact[0].id) passed for this exact tree ($tree)"
        exit 0
    }
    foreach ($run in $passing) {
        $changed = Get-ChangedPaths $run.tree $tree
        if ($null -ne $changed) {
            "NOT VERIFIED: the latest passing run $($run.id) covers another tree. Changed since then:"
            $changed | Select-Object -First 20 | ForEach-Object { "  $_" }
            'Run the Batch gate again; legs these paths cannot affect are reused.'
            exit 1
        }
    }
    'NOT VERIFIED: no passing Batch or Exit receipt exists.'
    exit 1
}

$changedSinceBase = Get-ChangedPaths $Base $tree
if ($null -eq $changedSinceBase) { throw "cannot compare against -Base $Base" }
$winFormsReasons = @($changedSinceBase | Where-Object { $_ -notmatch $cannotAffectWinForms })

$focusedFilter = if ($Level -eq 'Exit') { $wholeAssemblyFilter } else { $Filter }
$skipWinForms = if ($Level -eq 'Batch' -and -not $winFormsReasons.Count) { "none of the $($changedSinceBase.Count) changed paths can affect it" } else { $null }
$legs = if ($CommentOnly) { Get-CommentOnlyLegs } else { switch ($Level) {
    'Edit' {
        @{ Name = 'edit-build'; Ignored = $anyPath }
        @{ Name = 'focused'; Ignored = $cannotAffectProduct; TestFilter = $focusedFilter }
        if ($ToolTestProject.Count) { @{ Name = 'tool-tests'; Ignored = $anyPath; TestFilter = ($ToolTestProject -join '|') } }
    }
    default {
        @{ Name = 'winforms'; Ignored = $cannotAffectWinForms; Skip = $skipWinForms }
        @{ Name = 'avalonia-build'; Ignored = $cannotAffectProduct }
        @{ Name = 'focused'; Ignored = $cannotAffectProduct; TestFilter = $focusedFilter }
        @{ Name = 'construction'; Ignored = $cannotAffectProduct }
        if ($ToolTestProject.Count) { @{ Name = 'tool-tests'; Ignored = $anyPath; TestFilter = ($ToolTestProject -join '|') } }
        @{ Name = 'inventory'; Ignored = $cannotAffectInventory }
        @{ Name = 'linux'; Ignored = $cannotAffectProduct; TestFilter = $focusedFilter }
        if ($ToolTestProject.Count) { @{ Name = 'linux-tool-tests'; Ignored = $anyPath; TestFilter = ($ToolTestProject -join '|') } }
        if ($Level -eq 'Exit') {
            @{ Name = 'linux-inventory'; Ignored = $anyPath }
            @{ Name = 'windows-smoke'; Ignored = $anyPath }
            @{ Name = 'linux-runtime'; Ignored = $anyPath }
        }
    }
} }
$legs = @($legs | ForEach-Object { [pscustomobject]$_ })
if ($Only) {
    $legs = @($Only | Select-Object -Unique | ForEach-Object {
        $name = $_
        $existing = @($legs | Where-Object Name -EQ $name)
        if ($existing.Count) { $existing[0] | Add-Member -NotePropertyName Skip -NotePropertyValue $null -Force -PassThru }
        else { [pscustomobject]@{ Name = $name; Ignored = $anyPath; TestFilter = if ($name -in 'focused', 'linux') { $focusedFilter } else { $null }; Skip = $null } }
    })
}
$needsFilter = @($legs | Where-Object { $_.Name -in 'focused', 'linux' -and -not $_.Skip }).Count -gt 0
if ($needsFilter -and [string]::IsNullOrWhiteSpace($focusedFilter)) { throw 'This level needs -Filter with a focused dotnet test filter.' }

# Marks each leg to run, skip or reuse.
$priorRuns = Get-PriorRuns
foreach ($leg in $legs) {
    $leg | Add-Member -NotePropertyName Reused -NotePropertyValue $null -Force
    if (-not $leg.PSObject.Properties['Skip']) { $leg | Add-Member -NotePropertyName Skip -NotePropertyValue $null -Force }
    if (-not $leg.PSObject.Properties['TestFilter']) { $leg | Add-Member -NotePropertyName TestFilter -NotePropertyValue $null -Force }
    if ($leg.Skip -or $CommentOnly -or $NoReuse -or $Only -or $Level -ne 'Batch') { continue }
    foreach ($run in $priorRuns) {
        if ($run.commentOnly) { continue }
        $prior = @($run.legs | Where-Object { $_.name -eq $leg.Name -and $_.status -eq 'passed' -and $_.filter -eq $leg.TestFilter })
        if (-not $prior.Count) { continue }
        $changed = Get-ChangedPaths $run.tree $tree
        if ($null -eq $changed) { continue }
        if (-not @($changed | Where-Object { $_ -notmatch $leg.Ignored }).Count) {
            $leg.Reused = [pscustomobject]@{ run = $run.id; result = $prior[0] }
            break
        }
    }
}

if ($PlanOnly) {
    "plan: $Level on tree $tree, $($changedSinceBase.Count) paths changed since $Base"
    foreach ($leg in $legs) {
        $action = if ($leg.Skip) { "skip   ($($leg.Skip))" } elseif ($leg.Reused) { "reuse  (passed in $($leg.Reused.run))" } else { 'run' }
        '  {0,-16} {1}' -f $leg.Name, $action
    }
    if ($winFormsReasons.Count) { "  winforms inputs changed, for example: $($winFormsReasons[0])" }
    exit 0
}

# Guards. Disk only matters when a leg will really run.
$free = Get-FreeGb
if (@($legs | Where-Object { -not $_.Skip -and -not $_.Reused }).Count) {
    if ($free -lt $refuseBelowGb) { throw "only $free GB free; a build can fill the disk. Free space or run -CleanOutputs." }
    if ($free -lt $warnBelowGb) { Write-Warning "only $free GB free" }
}
$liveHosts = @(Get-CimInstance Win32_Process -Filter "Name='testhost.exe' OR Name='vstest.console.exe' OR Name='dotnet.exe'" |
    Where-Object { $_.CommandLine -and $_.CommandLine -match 'testhost|vstest\.console' -and $_.CommandLine -like "*$repo*" })
if ($liveHosts.Count) { throw "a test host from this repository is still running (PID $($liveHosts.ProcessId -join ', ')); a retry would look hung. Stop it first." }
if ($Level -ne 'Edit') {
    $tracked = @(git ls-files -- AGENTS.md PLAN.md CLAUDE.md port-rules)
    if ($tracked.Count) { throw "local-only documents became tracked: $($tracked -join ', ')" }
    $protected = @($changedSinceBase | Where-Object { $_ -match $readOnlyOriginals })
    if ($protected.Count -and -not $AllowProtected) { throw "read-only original sources changed: $($protected -join ', ')" }
}

$null = New-Item -ItemType Directory -Path $gateRoot -Force
$lockPath = Join-Path $gateRoot 'gate.lock'
if (Test-Path -LiteralPath $lockPath) {
    $owner = (Get-Content -LiteralPath $lockPath -Raw).Trim()
    if ($owner -and (Get-Process -Id $owner -ErrorAction SilentlyContinue)) { throw "another gate is running (PID $owner)." }
    Remove-Item -LiteralPath $lockPath
}
$lock = [IO.File]::Open($lockPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::Read)
$lockBytes = [Text.Encoding]::ASCII.GetBytes("$PID")
$lock.Write($lockBytes, 0, $lockBytes.Length)
$lock.Flush()

$id = '{0}-{1}' -f (Get-Date).ToUniversalTime().ToString('yyyyMMdd-HHmmss'), $Level.ToLowerInvariant()
$runDirectory = Join-Path $gateRoot $id
$null = New-Item -ItemType Directory -Path $runDirectory
$started = Get-Date
$records = [Collections.Generic.List[object]]::new()
$status = 'passed'
$oldFailFast = $env:GITEXTENSIONS_DEBUG_FAIL_FAST
$env:GITEXTENSIONS_DEBUG_FAIL_FAST = '1'

try {
    foreach ($leg in $legs) {
        $record = [ordered]@{ name = $leg.Name; filter = $leg.TestFilter; status = ''; seconds = 0 }
        if ($status -ne 'passed') {
            $record.status = 'notRun'
        }
        elseif ($leg.Skip) {
            $record.status = 'skipped'
            $record.reason = $leg.Skip
            '[skip] {0,-16} {1}' -f $leg.Name, $leg.Skip
        }
        elseif ($leg.Reused) {
            $record.status = 'passed'
            $record.reusedFrom = $leg.Reused.run
            foreach ($key in 'total', 'passed', 'skipped', 'failed', 'inventory') {
                if ($leg.Reused.result.PSObject.Properties[$key]) { $record[$key] = $leg.Reused.result.$key }
            }
            '[reuse] {0,-15} passed in {1}' -f $leg.Name, $leg.Reused.run
        }
        else {
            $legStart = Get-Date
            try {
                $result = & $legActions[$leg.Name] | Select-Object -Last 1
                if ($result -and $result.PSObject.Properties['total']) {
                    foreach ($key in 'total', 'passed', 'skipped', 'failed') { $record[$key] = $result.$key }
                }
                if ($result -and $result.PSObject.Properties['inventory']) { $record.inventory = $result.inventory }
                $record.status = 'passed'
            }
            catch {
                $record.status = 'failed'
                $record.error = "$_"
                $status = 'failed'
            }
            $record.seconds = [math]::Round(((Get-Date) - $legStart).TotalSeconds)
            $detail = if ($record.Contains('total')) { "$($record.passed) passed, $($record.skipped) skipped" } else { '' }
            if ($record.Contains('inventory')) { $detail += ", inventory $($record.inventory.findingCount)+$($record.inventory.dependentFindingCount)" }
            if ($record.status -eq 'failed') { $detail = $record.error }
            '[{0}] {1,-16} {2,5}s  {3}' -f $(if ($record.status -eq 'passed') { 'ok' } else { 'FAIL' }), $leg.Name, $record.seconds, $detail
        }
        $records.Add([pscustomobject]$record)
    }
}
finally {
    $env:GITEXTENSIONS_DEBUG_FAIL_FAST = $oldFailFast
    $lock.Dispose()
    Remove-Item -LiteralPath $lockPath -ErrorAction SilentlyContinue
}

$notRun = @('native captures and paired comparison', 'manual side-by-side walk', 'IDE preview', 'macOS', 'Flatpak (run eng/avalonia/wsl-gate.sh flatpak when the batch can affect it)')
if ($CommentOnly) { $notRun += 'all test legs and Linux: comment-only mode requires proof, Windows build and inventory instead' }
if ($Level -ne 'Exit') { $notRun += @('runtime smokes', 'Linux inventory comparison') }
foreach ($assemblyLeg in @(@{ Name = 'focused'; Platform = 'Windows'; Trx = 'focused.trx' }, @{ Name = 'linux'; Platform = 'Linux'; Trx = 'linux-focused.trx' })) {
    $wholeAssemblyRan = @($records | Where-Object {
        $_.name -eq $assemblyLeg.Name -and $_.filter -eq $wholeAssemblyFilter -and
            ($_.status -eq 'passed' -or
                ($_.status -eq 'failed' -and (Test-AssemblyCompleted (Join-Path $runDirectory $assemblyLeg.Trx))))
    }).Count -gt 0
    if (-not $wholeAssemblyRan) { $notRun += "$($assemblyLeg.Platform) whole test assembly" }
}
if ((Get-WorkTree) -ne $tree) {
    $status = 'failed'
    Write-Warning 'The working tree changed during the gate; this receipt cannot verify the final tree.'
}
[pscustomobject]@{
    schema = 1
    id = $id
    level = $Level
    commentOnly = [bool]$CommentOnly
    partial = [bool]$Only
    status = $status
    head = $head
    tree = $tree
    base = $Base
    filter = $Filter
    toolTestProjects = $ToolTestProject
    changedPaths = $changedSinceBase.Count
    startedUtc = $started.ToUniversalTime().ToString('o')
    seconds = [math]::Round(((Get-Date) - $started).TotalSeconds)
    freeGb = Get-FreeGb
    legs = $records
    notRun = $notRun
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $runDirectory 'receipt.json') -Encoding utf8

# Receipts and retained evidence stay forever; prune old successful logs/results.
Remove-ExpiredGateLogs $gateRoot $keepPassingLogs

$total = [math]::Round(((Get-Date) - $started).TotalMinutes, 1)
"$($status.ToUpperInvariant()) $Level tree $tree in $total min, $(Get-FreeGb) GB free; receipt: eng/avalonia/parity-evidence/gates/$id/receipt.json"
if ($status -ne 'passed') { exit 1 }
