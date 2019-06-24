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
