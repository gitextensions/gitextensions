<#
.SYNOPSIS
    Script that runs inside Windows Sandbox to install dependencies and kick off the setup-repo script.
.DESCRIPTION
    Sets up initial environment for testing Git Extensions in a clean sandbox.
.NOTES
    Alter to fit your desired initial environment as needed.
#>

$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'

function ShowFileExtensions() {
    # http://superuser.com/questions/666891/script-to-set-hide-file-extensions
    Push-Location
    Set-Location HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced
    Set-ItemProperty . HideFileExt "0"
    Pop-Location
    Stop-Process -processName: Explorer -force # This will restart the Explorer service to make this work.
}
 
ShowFileExtensions

Set-Variable -Name 'testID'-Scope 'script' -Value $([guid]::NewGuid().ToString("N")) 
Set-Variable -Name 'TestResults' -Scope 'script' -Value "$env:USERPROFILE\Desktop\TestResults\$testID"
mkdir $TestResults 
Write-Output "Test Result Location: $TestResults"

#Install chocolatey
& { Set-ExecutionPolicy Bypass -Scope Process -Force;
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; 
    Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1')); 
    Import-Module "$env:ProgramData\chocolatey\helpers\chocolateyInstaller.psm1";
    Update-SessionEnvironment;    
}
#install dependencies
choco upgrade git p4merge dotnet-6.0-desktopruntime vscode -y

#adjust path environment variable
$paths = $env:Path -split ';'
$paths += 'C:\Program Files\Git\usr\bin\'
$paths += 'C:\Users\WDAGUtilityAccount\Desktop\artifacts\Debug\bin\GitExtensions\net6.0-windows'
[Environment]::SetEnvironmentVariable('Path', $paths -join ';', [EnvironmentVariableTarget]::User)
Update-SessionEnvironment

#configure git
git config --global user.name 'Tester'
git config --global user.email 'Test@test.com'
git config --global rebase.autosquash 'true'
git config --global rebase.autostash 'true'
git config --global rebase.updaterefs 'true'
git config --global i18n.filesencoding 'utf-8'
git config --global core.autocrlf 'true'
git config --global difftool.p4merge.path 'C:/Program Files/Perforce/p4merge.exe'
git config --global difftool.p4merge.cmd '\"C:/Program Files/Perforce/p4merge.exe\" \"$LOCAL\" \"$REMOTE\"'
git config --global mergetool.p4merge.path 'C:/Program Files/Perforce/p4merge.exe'
git config --global mergetool.p4merge.cmd '\"C:/Program Files/Perforce/p4merge.exe\" \"$BASE\" \"$LOCAL\" \"$REMOTE\" \"$MERGED\"'
git config --global merge.guitool 'p4merge'
git config --global diff.guitool 'p4merge'


#create shortcut to Git Extensions
$TargetFile = "C:\Users\WDAGUtilityAccount\Desktop\artifacts\Debug\bin\GitExtensions\net6.0-windows\GitExtensions.exe"
$ShortcutFile = "$env:Public\Desktop\Git Extensions.lnk"
$WScriptShell = New-Object -ComObject WScript.Shell
$Shortcut = $WScriptShell.CreateShortcut($ShortcutFile)
$Shortcut.TargetPath = $TargetFile
$Shortcut.Save()



Copy-Item "$env:USERPROFILE\Desktop\wsb\psr.exe" "$env:SystemRoot\system32\"
Set-Alias psr "$env:SystemRoot\system32\psr.exe"


#setup repo in sandbox
& $PSScriptRoot\Setup-Repo.ps1

Copy-Item .\setup.log $TestResults


$output = "$env:USERPROFILE\Desktop\TestResults\$testID.zip"
Compress-Archive -Path $TestResults -DestinationPath $output -Verbose 4>&1

Write-Output ''
Write-Output "Test results created at $output." 


