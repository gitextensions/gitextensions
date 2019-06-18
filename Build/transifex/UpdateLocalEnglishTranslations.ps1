[CmdletBinding()]
param (
   [string] $Configuration = 'Release'
)

pushd $PSScriptRoot

Write-Host "Copying the latest English translation before the update..."
$translationsFolder = Resolve-Path "$PSScriptRoot\..\..\GitUI\Translation";
$releaseTranslationsFolder = Resolve-Path ..\..\GitExtensions\bin\$Configuration\Translation
Write-Host " > $translationsFolder`r`n > $releaseTranslationsFolder"
xcopy "$translationsFolder\English*.xlf" "$releaseTranslationsFolder" /Y

$src = Resolve-Path ..\..\GitExtensions\bin\$Configuration
pushd "$src"
Write-Host "Updating the English translation..."
Start-Process -FilePath "$src\TranslationApp.exe" -ArgumentList "update" -Wait
if ($LASTEXITCODE -ne 0) {
    popd
    exit -1
}
popd

Write-Host "Copying the updated English translation to commit..."
Write-Host " > $releaseTranslationsFolder`r`n > $translationsFolder"
xcopy "$releaseTranslationsFolder\English*.xlf" "$translationsFolder"  /Y
