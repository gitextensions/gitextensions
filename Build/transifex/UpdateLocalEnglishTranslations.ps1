[CmdletBinding()]
param (
   [string] $Configuration = 'Release'
)

Push-Location $PSScriptRoot
try {
    $translationsFolder = Resolve-Path .\
    $releaseTranslationsFolder = Resolve-Path ..\..\GitExtensions\bin\$Configuration\Translation
    Write-Host "Copying the latest English translation before the update..."
    Write-Host " > $releaseTranslationsFolder --> $translationsFolder"
    Copy-Item -Path "$releaseTranslationsFolder\English*.xlf" -Destination "$translationsFolder" -Force
    if ($LASTEXITCODE -ne 0) {
        exit -1
    }
    
    $src = Resolve-Path ..\..\GitExtensions\bin\$Configuration
    Push-Location "$src"
    try {
        Write-Host "Updating the English translation..."
        Start-Process -FilePath "$src\TranslationApp.exe" -ArgumentList "update" -Wait
        if ($LASTEXITCODE -ne 0) {
            exit -1
        }
    }
    finally {
        Pop-Location
    }

    Write-Host "Copying the updated English translation to commit..."
    Write-Host " > $translationsFolder --> $releaseTranslationsFolder"
    Move-Item -Path "$translationsFolder\English*.xlf" -Destination "$releaseTranslationsFolder" -Force
    if ($LASTEXITCODE -ne 0) {
        exit -1
    }
}
finally {
    Pop-Location
}
