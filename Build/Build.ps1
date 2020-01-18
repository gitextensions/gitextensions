[CmdletBinding(PositionalBinding=$false)]
Param(
  [string] $version,
  [string][Alias('c')] $configuration = "Debug",
  [string][Alias('v')] $verbosity = "minimal",
  [switch] $restore,
  [switch][Alias('b')]$build,
  [switch] $rebuild,
  [switch] $clean,
  [switch][Alias('t')] $test,
  [switch][Alias('bl')] $binaryLog,
  [string] $platform = $null,
  [switch] $help,
  [Parameter(ValueFromRemainingArguments=$true)][String[]]$properties
)

. $PSScriptRoot\tools.ps1

# break on errors
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$PSDefaultParameterValues['*:ErrorAction'] = 'Stop'

$env:SKIP_PAUSE=1
$TfmConfiguration = "$Configuration\net461";


function Build {
  $toolsetBuildProj = Resolve-Path 'Build\tools\Build.proj'
  Write-Host $toolsetBuildProj

  # build the solution
  $bl = if ($binaryLog) { "/bl:" + (Join-Path $LogDir "build.binlog") } else { "" }
  $platformArg = if ($platform) { "/p:Platform=$platform" } else { "" }

  MSBuild $toolsetBuildProj `
    $bl `
    $platformArg `
    /p:Configuration=$configuration `
    /p:RepoRoot=$RepoRoot `
    /p:Restore=$restore `
    /p:Build=$build `
    /p:Rebuild=$rebuild `
    @properties;
}

try {
  Push-Location $PSScriptRoot\..\

  if ($clean) {
    if (Test-Path $ArtifactsDir) {
      Remove-Item -Recurse -Force $ArtifactsDir
      Write-Host 'Artifacts directory deleted.'
    }
    exit 0
  }

  Build
}
catch {
  Write-Host $_.Exception -ForegroundColor Red
  return -1
}
finally {
  Pop-Location
}
