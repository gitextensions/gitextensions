<#
.SYNOPSIS
    Verifies the repository: builds the full solution and runs all unit tests.

.DESCRIPTION
    Single source of truth for "the repo is verified". Used both locally (run it
    at any time, from any directory) and by CI (.github/workflows/fork-ci.yml),
    which is a thin wrapper around this script.

    Scope: builds GitExtensions.slnx and runs the unit test projects under
    tests/app/UnitTests and tests/plugins/UnitTests. Integration tests
    (tests/app/IntegrationTests) are excluded on purpose: they instantiate real
    WinForms forms and are known to be fragile on CI runners.

    TRX result files are written to artifacts/<Configuration>/TestResults/.

.PARAMETER Configuration
    Build configuration: Release (default) or Debug. Use Debug locally to reuse
    your incremental development build.

.EXAMPLE
    .\eng\Verify.ps1
    .\eng\Verify.ps1 -Configuration Debug
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

# Resolve paths relative to this script so it works from any current directory.
$repoRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repoRoot 'GitExtensions.slnx'
$testResultsDir = Join-Path $repoRoot "artifacts\$Configuration\TestResults"

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

Write-Host ''
Write-Host "=== Verify: build ($Configuration) ===" -ForegroundColor Cyan
dotnet build $solution -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host ''
    Write-Host "VERIFY FAILED: build failed (exit code $LASTEXITCODE). Tests were not run." -ForegroundColor Red
    exit 1
}

# Discover unit test projects. Integration tests are excluded by design.
$unitTestRoots = @(
    (Join-Path $repoRoot 'tests\app\UnitTests'),
    (Join-Path $repoRoot 'tests\plugins\UnitTests')
)
$testProjects = $unitTestRoots |
    ForEach-Object { Get-ChildItem -Path $_ -Recurse -Filter '*.csproj' } |
    Sort-Object Name

Write-Host ''
Write-Host "=== Verify: unit tests ($($testProjects.Count) projects) ===" -ForegroundColor Cyan

$results = @()
foreach ($project in $testProjects) {
    Write-Host ''
    Write-Host "--- $($project.BaseName)" -ForegroundColor Cyan
    # --no-build: the solution build above already compiled every test project.
    dotnet test $project.FullName -c $Configuration --no-build `
        --logger "trx;LogFileName=$($project.BaseName).trx" `
        --results-directory $testResultsDir
    $results += [pscustomobject]@{
        Project = $project.BaseName
        Passed  = ($LASTEXITCODE -eq 0)
    }
}

$stopwatch.Stop()
$failed = @($results | Where-Object { -not $_.Passed })

Write-Host ''
Write-Host '=== Verify: summary ===' -ForegroundColor Cyan
foreach ($result in $results) {
    if ($result.Passed) {
        Write-Host "  OK   $($result.Project)" -ForegroundColor Green
    }
    else {
        Write-Host "  FAIL $($result.Project)" -ForegroundColor Red
    }
}
Write-Host ''
Write-Host ("Elapsed: {0:mm\:ss}. TRX logs: {1}" -f $stopwatch.Elapsed, $testResultsDir)

if ($failed.Count -gt 0) {
    Write-Host "VERIFY FAILED: $($failed.Count) of $($results.Count) test projects failed." -ForegroundColor Red
    exit 1
}

Write-Host "VERIFY OK: build clean and all $($results.Count) unit test projects passed." -ForegroundColor Green
exit 0
