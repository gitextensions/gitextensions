[CmdletBinding()]
param (
   [string] $Configuration = 'Release'
)

pushd $PSScriptRoot
try {
    Write-Host "Copying the latest English translation before the update..."
    $translationsFolder = Resolve-Path "$PSScriptRoot\..\..\GitUI\Translation";
    $releaseTranslationsFolder = Resolve-Path ..\..\GitExtensions\bin\$Configuration\*\Translation
    Write-Host "Copying '$translationsFolder\English*.xlf' to '$releaseTranslationsFolder'"
    xcopy "$translationsFolder\English*.xlf" "$releaseTranslationsFolder" /Y

    $src = Split-Path -Path $releaseTranslationsFolder -Parent
    pushd "$src"
    try {
        Write-Host "Updating the English translation..."
        Start-Process -FilePath "$src\TranslationApp.exe" -ArgumentList "update" -Wait
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[ERROR] Failed to update English translations..."
            exit -1
        }
    }
    finally {
        popd
    }

    Write-Host "Copying '$releaseTranslationsFolder\English*.xlf' to '$translationsFolder'"
    xcopy "$releaseTranslationsFolder\English*.xlf" "$translationsFolder"  /Y
}
finally {
    popd
}
