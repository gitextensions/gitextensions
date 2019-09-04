[CmdletBinding()]
param (
    [string] $Configuration = 'Release',
    [string] $BuildType = 'Rebuild'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$PSDefaultParameterValues['*:ErrorAction']='Stop'


# -------------------------------
# debugging
# -------------------------------
if ($env:ARTIFACT_DEBUG_ENABLED -eq $true) {
    Write-Host "[INFO]: GitStatus.txt is to help find dirty status.  File should say repo and submodules are clean."
    Write-Host "[INFO]: Update the skip-worktree section in this script to fix CI builds."
    & git status > GitStatus.txt
    & git submodule foreach --recursive git status >> GitStatus.txt
    Push-AppveyorArtifact .\GitStatus.txt
    & tree /F /A > tree.txt
    Push-AppveyorArtifact .\tree.txt
}

# ----------------------------------------------------------------------
# download PluginManager
# ----------------------------------------------------------------------
Push-Location $PSScriptRoot

& .\Download-PluginManager.ps1 -ExtractRootPath '..\Plugins\GitExtensions.PluginManager'

Pop-Location

# -------------------------------
# build artifacts
# -------------------------------
Push-Location $PSScriptRoot/../Setup

$hMSBuild = Resolve-Path hMSBuild.bat

$Version = '3.2.0.0';
if (![string]::IsNullOrWhiteSpace($env:APPVEYOR_BUILD_VERSION)) {
    $Version = $env:APPVEYOR_BUILD_VERSION
}

try {
    Write-Host "Creating installers for Git Extensions $Version";

    $msi = "../GitExtensions-$Version.msi";
    if (Test-Path $msi) {
        Write-Host "Removing GitExtensions-$Version.msi"
        Remove-Item -Path $msi -Force | Out-Null
    }

    try {
        Write-Host "----------------------------------------------------------------------"
        Write-Host "Building GitExtensionsShellEx"
        Write-Host "----------------------------------------------------------------------"
        Push-Location ..\GitExtensionsShellEx
        & $hMSBuild GitExtensionsShellEx.sln -notamd64 /p:Platform=Win32 /p:Configuration=$Configuration /t:Rebuild /nologo /v:m
        if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
        & $hMSBuild GitExtensionsShellEx.sln -notamd64 /p:Platform=x64 /p:Configuration=$Configuration /t:Rebuild /nologo /v:m
        if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    }
    finally {
        Pop-Location
    }

    try {
        Write-Host "----------------------------------------------------------------------"
        Write-Host "Building GitExtSshAskPass"
        Write-Host "----------------------------------------------------------------------"
        Push-Location ..\GitExtSshAskPass
        & $hMSBuild GitExtSshAskPass.sln -notamd64 /p:Platform=Win32 /p:Configuration=$Configuration /t:Rebuild /nologo /v:m
    }
    finally {
        Pop-Location
    }

    try {
        Write-Host "----------------------------------------------------------------------"
        Write-Host "Building GitExtensionsVSIX"
        Write-Host "----------------------------------------------------------------------"
        Push-Location ..\GitExtensionsVSIX

        $path = "..\GitExtensionsVSIX\bin\$Configuration";
        if (Test-Path $path) {
            Remove-Item -Path $path -Force -Recurse | Out-Null
        }

        & $hMSBuild -notamd64 /p:Configuration=$Configuration /t:Rebuild /nologo /v:m /bl
        if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    }
    finally {
        Pop-Location
    }

    Write-Host "----------------------------------------------------------------------"
    Write-Host "Building msi"
    Write-Host "----------------------------------------------------------------------"
    & $hMSBuild -notamd64 Setup.wixproj /t:$BuildType /p:Version=$Version /p:NumericVersion=$Version /p:Configuration=$Configuration /nologo /v:m /bl
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

    Move-Item -Path .\bin\$Configuration\GitExtensions.msi -Destination "$PSScriptRoot/../GitExtensions-$Version.msi" -Force

    & .\Set-Portable.ps1 -IsPortable
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

    & .\MakePortableArchive.cmd $Configuration $Version
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

    & .\Set-Portable.ps1
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

    # move the pdbs to the root folder, so the published artifact looks nicer
    Move-Item -Path $PSScriptRoot/../Setup/GitExtensions-pdbs-*.zip -Destination $PSScriptRoot/../ -Force
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Move-Item -Path $PSScriptRoot/../Setup/GitExtensions-Portable-*.zip -Destination $PSScriptRoot/../ -Force
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Move-Item -Path $PSScriptRoot/../GitExtensionsVSIX/bin/$Configuration/GitExtensionsVSIX.vsix -Destination $PSScriptRoot/../ -Force
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

}
finally {
    Pop-Location
}


# get files
$msi = (Resolve-Path $PSScriptRoot/../GitExtensions-*.msi)[0].Path;
$zip = (Resolve-Path $PSScriptRoot/../GitExtensions-Portable-*.zip)[0].Path;
$vsix = (Resolve-Path $PSScriptRoot/../GitExtensionsVSIX.vsix)[0].Path;


# do not sign artifacts for non-release branches
if ($env:APPVEYOR_PULL_REQUEST_TITLE) {
    Write-Host "[INFO]: Do not sign non-release branches"
    Get-ChildItem $zip | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
    Get-ChildItem $vsix | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
    Exit-AppVeyorBuild
    return
}

# -------------------------------
# sign artifacts
# -------------------------------

# archive files so we send them all in one go
$combined = ".\combined.$Version-unsigned.zip"
Compress-Archive -LiteralPath $msi, $zip, $vsix -CompressionLevel NoCompression -DestinationPath $combined -Force

if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
