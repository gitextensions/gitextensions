[CmdletBinding(PositionalBinding=$false)]
Param(
  [string] $version,
  [string] $logFileName = "build.binlog",
  [string][Alias('c')] $configuration = "Debug",
  [string][Alias('v')] $verbosity = "minimal",
  [switch] $restore,
  [switch][Alias('b')]$build,
  [switch] $rebuild,
  [switch] $buildNative,
  [switch][Alias('l')] $launch,
  [switch] $clean,
  [switch] $publish,
  [switch] $loc,
  [switch] $ci,
  [switch][Alias('t')] $test,
  [switch][Alias('it')] $integrationTest,
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
$toolsetBuildProj = Resolve-Path 'scripts\tools\Build.proj'
 
function Build {

  if ($buildNative) {
    $bl = if ($binaryLog) { "/bl:" + (Join-Path $LogDir "GitExtSshAskPass.binlog") } else { "" }
    MSBuild $toolsetBuildProj `
        $bl `
        /p:Platform=Win32 `
        /p:Configuration=$configuration `
        /p:RepoRoot=$RepoRoot `
        /p:Projects=$RepoRoot\GitExtSshAskPass\GitExtSshAskPass.sln `
        /p:Build=$buildNative `
        /p:Rebuild=$rebuild `
        @properties;

    # force rebuild both configurations, otherwise there is a risk of failure with
    # error BK4502: truncated .SBR file
    $bl = if ($binaryLog) { "/bl:" + (Join-Path $LogDir "GitExtensionsShellEx32.binlog") } else { "" }
    MSBuild $toolsetBuildProj `
        $bl `
        /p:Platform=Win32 `
        /p:Configuration=$configuration `
        /p:RepoRoot=$RepoRoot `
        /p:Projects=$RepoRoot\GitExtensionsShellEx\GitExtensionsShellEx.sln `
        /p:Build=$buildNative `
        /p:Rebuild=true `
        @properties;

    $bl = if ($binaryLog) { "/bl:" + (Join-Path $LogDir "GitExtensionsShellEx64.binlog") } else { "" }
    MSBuild $toolsetBuildProj `
        $bl `
        /p:Platform=x64 `
        /p:Configuration=$configuration `
        /p:RepoRoot=$RepoRoot `
        /p:Projects=$RepoRoot\GitExtensionsShellEx\GitExtensionsShellEx.sln `
        /p:Build=$buildNative `
        /p:Rebuild=true `
        @properties;
  }

  # build the solution
  $bl = if ($binaryLog) { "/bl:" + (Join-Path $LogDir $logFileName) } else { "" }
  $platformArg = if ($platform) { "/p:Platform=$platform" } else { "" }

  MSBuild $toolsetBuildProj `
    $bl `
    $platformArg `
    /p:Configuration=$configuration `
    /p:RepoRoot=$RepoRoot `
    /p:Restore=$restore `
    /p:Build=$build `
    /p:Rebuild=$rebuild `
    /p:Test=$test `
    /p:Publish=$publish `
    /p:IntegrationTest=$integrationTest `
    /p:Localise=$loc `
    /p:ContinuousIntegrationBuild=$ci `
    @properties;

  $exitCode = $LastExitCode;
  if ($exitCode -ne 0) {
    Exit $exitCode
  }

  # launch the app once it is built
  if ($launch) {
    $gitExtensionsExe = "$ArtifactsDir\bin\GitExtensions\$configuration\net461\GitExtensions.exe";
    if (Test-Path $gitExtensionsExe) {
      & $gitExtensionsExe
    }
  }
}

try {
  Push-Location $PSScriptRoot\..\

  if ($clean) {
    if (Test-Path $ArtifactsDir) {
      Remove-Item -Recurse -Force $ArtifactsDir
      Write-Host 'Artifacts directory deleted.'
    }

    MSBuild $toolsetBuildProj `
      /p:RepoRoot=$RepoRoot `
      /p:Clean=$clean `
      @properties;

    Exit 0
  }

  Build
}
catch {
  Write-Error $_.Exception
  Exit -1
}
finally {
  Pop-Location
}
