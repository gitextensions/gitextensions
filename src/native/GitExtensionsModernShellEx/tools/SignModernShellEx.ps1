[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [Parameter(Mandatory = $true)][string]$CertificatePath,
    [string]$DllPath,
    [switch]$Force,
    [string]$SignToolPath
)

function Get-SignToolPath {
    param([string]$OverridePath)
    if ($OverridePath -and (Test-Path $OverridePath)) {
        return $OverridePath
    }

    $kitRoot = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
    if (-not (Test-Path $kitRoot)) {
        return $null
    }

    $candidates = Get-ChildItem -Path $kitRoot -Directory | Sort-Object -Property Name -Descending
    foreach ($candidate in $candidates) {
        $toolPath = Join-Path $candidate.FullName "x64\signtool.exe"
        if (Test-Path $toolPath) {
            return $toolPath
        }
    }

    return $null
}

function Ensure-Certificate {
    param([string]$Path)

    $directory = Split-Path -Parent $Path
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    if (Test-Path $Path) {
        return
    }

    $certificate = New-SelfSignedCertificate -Subject "CN=GitExtensions" -Type CodeSigningCert -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeyLength 2048 -HashAlgorithm sha256 -NotAfter (Get-Date).AddYears(5)
    $password = ConvertTo-SecureString -String "gitextensions" -AsPlainText -Force
    Export-PfxCertificate -Cert $certificate -FilePath $Path -Password $password | Out-Null
}

function Get-DefaultDllPath {
    param([string]$Path)
    $directory = Split-Path -Parent $Path
    return (Join-Path $directory "GitExtensionsModernShellEx.dll")
}

function Test-ValidSignature {
    param([string]$Path)
    if (-not (Test-Path $Path)) {
        throw "Unable to locate $Path."
    }

    $signature = Get-AuthenticodeSignature -FilePath $Path
    return $signature.Status -eq "Valid"
}

$resolvedDllPath = if ($DllPath) { $DllPath } else { Get-DefaultDllPath -Path $PackagePath }
$pathsToCheck = @($PackagePath, $resolvedDllPath)

$invalidPaths = @()
foreach ($path in $pathsToCheck) {
    if (-not (Test-ValidSignature -Path $path)) {
        $invalidPaths += $path
    }
}

if ($invalidPaths.Count -eq 0 -and -not $Force) {
    Write-Host "Signatures are valid for all artifacts."
    return
}

$response = Read-Host "Unsigned artifacts detected. Create or reuse a self-signed certificate for development signing? (y/N)"
if ($response -notin @("y", "Y")) {
    Write-Host "Skipping signing."
    return
}

Ensure-Certificate -Path $CertificatePath

$signTool = Get-SignToolPath -OverridePath $SignToolPath
if (-not $signTool) {
    throw "Unable to locate signtool.exe. Specify -SignToolPath to override."
}

foreach ($path in $pathsToCheck) {
    if (-not $Force -and (Test-ValidSignature -Path $path)) {
        Write-Host "Skipping $path because it is already signed."
        continue
    }

    Write-Host "Signing $path using $signTool"
    & $signTool sign /fd SHA256 /a /f $CertificatePath /p "gitextensions" $path
}
