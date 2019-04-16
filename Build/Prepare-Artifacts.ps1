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

# -------------------------------
# build artifacts
# -------------------------------
& Setup\BuildInstallers.cmd
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
& Setup\Set-Portable.ps1 -IsPortable
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
& Setup\MakePortableArchive.cmd Release $env:APPVEYOR_BUILD_VERSION
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
& Setup\Set-Portable.ps1
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

# -------------------------------
# sign artifacts
# -------------------------------
# do not sign artifacts for non-release branches
if ($env:APPVEYOR_PULL_REQUEST_TITLE -eq $true) {
    return
}

# get files
$msi = (Resolve-Path ./Setup/GitExtensions-*.msi)[0].Path;
$zip = (Resolve-Path ./Setup/GitExtensions-Portable-*.zip)[0].Path;
$vsix = (Resolve-Path ./GitExtensionsVSIX/bin/Release/GitExtensionsVSIX.vsix)[0].Path;

# archive files so we send them all in one go
$combined = ".\combined.$($env:APPVEYOR_BUILD_VERSION)-unsigned.zip"
Compress-Archive -LiteralPath $msi, $zip, $vsix -CompressionLevel NoCompression -DestinationPath $combined -Force

# move the pdbs to the root folder, so the published artifact looks nicer
Move-Item -Path ./Setup/GitExtensions-pdbs-*.zip -Destination . -Force
Move-Item -Path ./GitExtensionsVSIX/bin/Release/GitExtensionsVSIX.vsix -Destination . -Force

if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
