#Requires -Version 7.0
# parity-scaffolding: Tests the gate's actual cleanup function without starting builds or desktop processes.
[CmdletBinding()]
param([string]$Gate = (Join-Path $PSScriptRoot 'gate.ps1'))

$ErrorActionPreference = 'Stop'
$ast = [Management.Automation.Language.Parser]::ParseFile($Gate, [ref]$null, [ref]$null)
$definition = $ast.Find({ param($node)
    $node -is [Management.Automation.Language.FunctionDefinitionAst] -and
        $node.Name -eq 'Remove-ExpiredGateLogs'
}, $true)
if (-not $definition) { throw 'The production cleanup function was not found.' }
Invoke-Expression $definition.Extent.Text

function Assert-Retained([string]$Path, [string]$Contents) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Evidence removed: $Path" }
    if ([IO.File]::ReadAllText($Path) -cne $Contents) { throw "Evidence changed: $Path" }
}

function Write-FixtureFile([string]$Path, [string]$Contents) {
    $null = New-Item -ItemType Directory -Force -Path ([IO.Path]::GetDirectoryName($Path))
    [IO.File]::WriteAllText($Path, $Contents)
}

$fixture = Join-Path ([IO.Path]::GetTempPath()) ('GateRetention-' + [guid]::NewGuid().ToString('N'))
$gateRoot = Join-Path $fixture 'gates'
try {
    $protected = [ordered]@{}
    foreach ($number in 1..7) {
        $run = Join-Path $gateRoot ('20261106-{0:d6}-batch' -f $number)
        $receipt = '{"status":"passed","tree":"unchanged"}'
        $protected[(Join-Path $run 'receipt.json')] = $receipt
        foreach ($relative in @(
            'windows-smoke/window.png', 'windows-smoke/plugins.json', 'windows-smoke/stderr.log',
            'linux-runtime/wayland/smoke.json', 'linux-runtime/wayland/window.png',
            'linux-runtime/x11/smoke.json', 'linux-runtime/x11/stderr.log',
            'inventory/report.json', 'evidence.log/proof.json', 'proof.json', 'linux.sh')) {
            $protected[(Join-Path $run $relative)] = "Evidence $number/$relative"
        }
        Write-FixtureFile (Join-Path $run 'focused.log') "Log $number"
        Write-FixtureFile (Join-Path $run 'focused.trx') '<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><TestSettings><Deployment runDeploymentRoot="test-deployment" /></TestSettings></TestRun>'
        Write-FixtureFile (Join-Path $run 'test-deployment/In/result.txt') "Test result $number"
    }
    foreach ($name in '20261106-000000-failed', '20261106-000008-failed') {
        $run = Join-Path $gateRoot $name
        $protected[(Join-Path $run 'receipt.json')] = '{"status":"failed"}'
        $protected[(Join-Path $run 'focused.log')] = 'Retain failed log'
        $protected[(Join-Path $run 'focused.trx')] = 'Retain failed result'
    }
    $protected[(Join-Path $gateRoot 'unfinished/focused.log')] = 'Retain unfinished log'
    $protected[(Join-Path $fixture 'night-20261105/exit-accepted/plugins.json')] = 'Accepted Exit evidence'
    foreach ($entry in $protected.GetEnumerator()) { Write-FixtureFile $entry.Key $entry.Value }

    # Retain two passing runs; failed and unfinished runs never consume those slots.
    Remove-ExpiredGateLogs $gateRoot 2
    foreach ($entry in $protected.GetEnumerator()) { Assert-Retained $entry.Key $entry.Value }
    foreach ($number in 1..7) {
        $run = Join-Path $gateRoot ('20261106-{0:d6}-batch' -f $number)
        foreach ($relative in 'focused.log', 'focused.trx', 'test-deployment') {
            $exists = Test-Path -LiteralPath (Join-Path $run $relative)
            if ($exists -ne ($number -gt 5)) { throw "Wrong retention: $number/$relative" }
        }
    }

    # A result file cannot redirect recursive deletion outside its run or into runtime evidence.
    $old = Join-Path $gateRoot '20261106-000001-batch'
    foreach ($target in '../outside', 'windows-smoke', 'linux-runtime') {
        Write-FixtureFile (Join-Path $old 'unsafe.trx') "<TestRun><TestSettings><Deployment runDeploymentRoot=`"$target`" /></TestSettings></TestRun>"
        $protected[(Join-Path $gateRoot 'outside/proof.json')] = 'Outside test deployment'
        Write-FixtureFile (Join-Path $gateRoot 'outside/proof.json') 'Outside test deployment'
        Remove-ExpiredGateLogs $gateRoot 2
        foreach ($entry in $protected.GetEnumerator()) { Assert-Retained $entry.Key $entry.Value }
    }
    Remove-ExpiredGateLogs $gateRoot 2
    foreach ($entry in $protected.GetEnumerator()) { Assert-Retained $entry.Key $entry.Value }
    'PASS: old logs/results pruned; receipts, both runtime backends, plugin evidence, recent/failed/unfinished runs and accepted Exit retained; unsafe deployment roots ignored; cleanup idempotent.'
}
finally {
    $resolved = [IO.Path]::GetFullPath($fixture)
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $resolved.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($resolved) -notmatch '^GateRetention-[0-9a-f]{32}$') {
        throw "Refusing unexpected fixture cleanup path: $resolved"
    }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
