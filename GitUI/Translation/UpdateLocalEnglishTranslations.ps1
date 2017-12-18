Write-Host "Copying latest english translation before update..."
robocopy .\ ..\..\GitExtensions\bin\Release\Translation English*.xlf

pushd ..\..\GitExtensions\bin\Release
Write-Host "Updating english translation..."
Start-Process -FilePath "TranslationApp.exe" -ArgumentList "update" -Wait
popd

Write-Host "Copying updated english translation for commit..."
robocopy ..\..\GitExtensions\bin\Release\Translation .\ English*.xlf
