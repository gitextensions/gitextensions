pushd $PSScriptRoot

Write-Host "Copying the latest English translation before the update..."
$translationsFolder = Resolve-Path .\
$releaseTranslationsFolder = Resolve-Path ..\..\GitExtensions\bin\Release\Translation
Write-Debug " > $translationsFolder`r`n > $releaseTranslationsFolder"
xcopy "$translationsFolder\English*.xlf" "$releaseTranslationsFolder" /Y

$src = Resolve-Path ..\..\GitExtensions\bin\Release
pushd "$src"
Write-Host "Updating the English translation..."
Start-Process -FilePath "$src\TranslationApp.exe" -ArgumentList "update" -Wait
if ($LASTEXITCODE -ne 0) {
    popd
    exit -1
}
popd

Write-Host "Copying the updated English translation to commit..."
Write-Debug " > $releaseTranslationsFolder`r`n > $translationsFolder"
xcopy "$releaseTranslationsFolder\English*.xlf" "$translationsFolder"  /Y
