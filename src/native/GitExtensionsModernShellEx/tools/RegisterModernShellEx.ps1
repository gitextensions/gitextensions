[CmdletBinding()]
param(
    [string]$PackagePath,
    [string]$ExternalLocation,
    [switch]$Unregister
)

$scriptDir = Split-Path -Parent $PSCommandPath
if (-not $PackagePath) {
    $PackagePath = Join-Path $scriptDir "GitExtensionsModernShellEx.msix"
}

if (-not $ExternalLocation) {
    $ExternalLocation = $scriptDir
}

$packageName = "GitExtensions.ModernShellEx"

if ($Unregister) {
    $package = Get-AppxPackage -Name $packageName
    if (-not $package) {
        Write-Host "$packageName is not registered."
        return
    }

    Write-Host "Removing $($package.PackageFullName)"
    Remove-AppxPackage -Package $package.PackageFullName
    return
}

if (-not (Test-Path $PackagePath)) {
    throw "Unable to locate $PackagePath."
}

if (-not (Test-Path $ExternalLocation)) {
    throw "Unable to locate $ExternalLocation."
}

Write-Host "Registering $PackagePath with external location $ExternalLocation"
Add-AppxPackage -Path $PackagePath -ExternalLocation $ExternalLocation -ForceApplicationShutdown -ForceUpdateFromAnyVersion
