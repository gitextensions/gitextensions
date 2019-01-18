pushd .\Setup

[Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
Invoke-WebRequest -Uri https://app.signpath.io/API/v1/Tools/SignPath.psm1 -OutFile .\SignPath.psm1
if (!(Test-Path .\SignPath.psm1)) {
    throw 'Unable to download https://app.signpath.io/API/v1/Tools/SignPath.psm1'
}

Import-Module .\SignPath.psm1 -Force;

# get files
$msi = (Resolve-Path GitExtensions-*.msi)[0].Path;
$zip = (Resolve-Path GitExtensions-Portable-*.zip)[0].Path;

# archive files so we send them all in one go
$combined = ".\combined.$($env:APPVEYOR_BUILD_VERSION).zip"
$combinedSigned = ".\combined.$($env:APPVEYOR_BUILD_VERSION).signed.zip"
Compress-Archive -LiteralPath $msi, $zip -CompressionLevel NoCompression -DestinationPath $combined -Force

# sign
$description = "https://ci.appveyor.com/project/gitextensions/gitextensions/builds/$env:APPVEYOR_BUILD_ID\n$env:APPVEYOR_REPO_COMMIT_MESSAGE\n$env:APPVEYOR_REPO_BRANCH from $env:APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH";
try {
    Submit-SigningRequest `
        -InputArtifactPath $combined `
        -Description $description `
        -CIUserToken $env:spciuser `
        -OrganizationId '7c19b2cf-90f7-4d15-9b12-1b615f7c18c4' `
        -SigningPolicyId '5c9879c7-0dea-4303-8e5b-fc4192a7b0de' `
        -WaitForCompletion `
        -WaitForCompletionTimeoutInSeconds 180 `
        -Force
}
catch {
    # no-op
}

# extract signed artifacts to Signed folder
if (Test-Path $combinedSigned) {
    Write-Host "Publishing signed artifacts"
    Expand-Archive  -LiteralPath $combinedSigned -DestinationPath .\Signed

    # -------------------------------
    # publish signed artifacts
    # -------------------------------
    Get-ChildItem .\Signed\*.* | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }

    $pdb = (Resolve-Path GitExtensions-pdbs-*.zip)[0].Path;
    Push-AppveyorArtifact $pdb
    return;
}

# signing failed, most likely it wasn't approved
Write-Host "Publishing non-signed artifacts"
# publish unsigned artifacts
$zip = (Resolve-Path GitExtensions-Portable-*.zip)[0].Path;
Push-AppveyorArtifact $zip
$pdb = (Resolve-Path GitExtensions-pdbs-*.zip)[0].Path;
Push-AppveyorArtifact $pdb
