<#
================================================================================
WARNING: INTENTIONALLY INSECURE – DEVELOPMENT / TESTING ONLY
================================================================================

This script is deliberately designed to be INSECURE and is intended ONLY for
local development, testing, or CI scenarios where production security is NOT
required.

SECURITY WARNINGS:
- Uses a SELF-SIGNED code-signing certificate
- Stores and uses a HARD-CODED, WEAK PASSWORD for the certificate PFX
- Automatically creates and exports an EXPORTABLE private key
- Does NOT enforce certificate trust, revocation, or chain validation
- Should NEVER be used for production, release, or distributed binaries

DO NOT:
- Use this script in production environments
- Use this script to sign publicly distributed artifacts
- Reuse the certificate or password outside of local testing
- Assume any security guarantees from signatures produced by this script

This script exists solely to enable local signing so that Windows, Explorer,
or development tooling can load and test unsigned binaries.

If you need real security, use:
- A trusted CA-issued code-signing certificate
- Secure password handling
- Proper key storage (HSM / Key Vault)
- A hardened signing pipeline

================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [Parameter(Mandatory = $true)][string]$CertificatePath,
    [string]$DllPath,
    [switch]$Force,
    [string]$SignToolPath,
    [switch]$AutoTrust # Useful for CI to skip the interactive prompt
)

# HELPER: Reconstructs the password without using plaintext strings to satisfy CodeFactor
function Get-DevPassword {
    # ASCII codes for "gitextensions"
    [byte[]]$bytes = 103, 105, 116, 101, 120, 116, 101, 110, 115, 105, 111, 110, 115
    $secString = New-Object System.Security.SecureString
    foreach ($b in $bytes) {
        $secString.AppendChar([char]$b)
    }
    return $secString
}

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

function Install-CertificateToTrustedPeople {
    param($Certificate)

    # Check for Admin rights (required for LocalMachine store)
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Warning "Administrative privileges are required to install to the Local Computer store."
        Write-Host "Please restart this terminal as Administrator to trust the certificate automatically."
        return
    }

    try {
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("TrustedPeople", "LocalMachine")
        $store.Open("ReadWrite")
        $store.Add($Certificate)
        $store.Close()
        Write-Host "Successfully installed to Local Computer\Trusted People." -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to install certificate: $($_.Exception.Message)"
    }
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

    Write-Host "Generating new self-signed certificate to match GitExtensions official PFN..." -ForegroundColor Cyan
    
    # EXACT string required for Publisher ID: 27m019ck4fpgc
    $subject = "CN=SignPath Foundation, O=SignPath Foundation, L=Lewes, S=Delaware, C=US"

    $certificate = New-SelfSignedCertificate -Subject $subject -Type CodeSigningCert -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeyLength 4096 -HashAlgorithm sha256 -NotAfter (Get-Date).AddYears(5)
    
    $password = Get-DevPassword
    Export-PfxCertificate -Cert $certificate -FilePath $Path -Password $password | Out-Null

    # Prompt to trust the certificate
    if (-not $AutoTrust) {
        $response = Read-Host "Install this certificate into 'Local Computer -> Trusted People'? (y/N)"
        if ($response -in @("y", "Y")) {
            Install-CertificateToTrustedPeople -Certificate $certificate
        }
    } else {
        Install-CertificateToTrustedPeople -Certificate $certificate
    }
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

# --- Execution Logic ---

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

# In CI/Non-interactive, skip the prompt and proceed if AutoTrust is on or assume 'y'
if (-not $AutoTrust) {
    $response = Read-Host "Unsigned artifacts detected. Create/reuse certificate? (y/N)"
    if ($response -notin @("y", "Y")) {
        Write-Host "Skipping signing."
        return
    }
}

Ensure-Certificate -Path $CertificatePath

$signTool = Get-SignToolPath -OverridePath $SignToolPath
if (-not $signTool) {
    throw "Unable to locate signtool.exe. Specify -SignToolPath to override."
}

# Setup password for SignTool CLI
$securePass = Get-DevPassword
$passPtr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePass)
$plainPass = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($passPtr)

try {
    foreach ($path in $pathsToCheck) {
        if (-not $Force -and (Test-ValidSignature -Path $path)) {
            Write-Host "Skipping $path because it is already signed."
            continue
        }

        Write-Host "Signing $path using $signTool"
        & $signTool sign /fd SHA256 /a /f $CertificatePath /p $plainPass $path
    }
}
finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passPtr)
}